using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class MachineController : MonoBehaviour
{
    /* マシンパラメータ */
    [SerializeField] private string machineName; // マシン名
    [SerializeField] private int hitPoint; // 耐久力（HP）
    [SerializeField] private int propellantPoint; // 推進力（PP）
    [SerializeField] private int atk; // 火力（Act）
    [SerializeField] private int def; // 装甲（Def）
    [SerializeField] private int spd; // 機動性（Speed）

    const float COMEBACK_TIME = 0.2f; // ダウンからの復帰時間

    private Rigidbody2D rb;
    private MapManager mapManager; // マップ情報
    private Unit unit; // パラメータ
    private MachineModel model;
    public MachineModel Model
    {
        get { return model; }
    }

    void Start()
    {
        // 必要な他コンポーネント取得
        rb = gameObject.transform.parent.GetComponent<Rigidbody2D>();
        mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();
        unit = gameObject.transform.parent.GetComponent<Unit>();
        if (model == null) InitializeData();

        // ステータス初期化
        model.SetParameter(machineName, hitPoint, propellantPoint, atk, def, spd);
        model.CurrentHP = model.HitPoint;
        model.CurrentPP = (float)model.PropellantPoint;

        // メイン武器装備
        Equip(MachineConst.MAIN_WEAPON);

        // 近距離武器装備
        Equip(MachineConst.HAND_WEAPON);

        // 盾装備
        Equip(MachineConst.SHIELD);

        // 回転を制御
        rb.freezeRotation = true;  // 回転を固定

        // 右向きのときは右を向く
        if (unit.IsRight)
        {
            model.IsRight = false;
            Turn(1f);
        }

        // スプライトの色を変更
        Common.SetColors(unit.Color, transform);

        // 推進剤チャージコルーチン開始
        StartCoroutine(ChargePropellant());
    }

    void Update()
    {
        // 0のときクラッシュする
        if (model.CurrentHP == 0 && !model.IsDead)
        {
            model.IsDead = true;
            StartCoroutine(Crush());
        }
    }

    void FixedUpdate()
    {
        // 機体の移動速度を更新
        rb.velocity = model.CurrentVelocity;

        // 移動位置を制限
        Common.RestrictMovePosition(rb, mapManager.MaxX, mapManager.MinX, mapManager.MaxY, mapManager.MinY);
    }

    // 装備
    void Equip(string equipmentType)
    {
        GameObject equipmentObject = null;
        Vector2 equipmentPosition = Vector2.zero;
        Vector2 parentPosition = Vector2.zero;
        Transform parent;
        
        // 右腕の武器、左腕のtransformをそれぞれ取得
        Transform weaponsTransform = transform.Find(MachineConst.ARM_BACK_GROUND).Find(MachineConst.ELBOW).Find(MachineConst.WEAPONS);
        Transform leftElbowTransform = transform.Find(MachineConst.ARM_FORE_GROUND).Find(MachineConst.ELBOW);

        // 装備種別によって事前準備
        if (equipmentType == MachineConst.MAIN_WEAPON)
        {
            equipmentObject = model.MainWeaponPrefab;
            equipmentPosition = equipmentObject.GetComponent<Weapon>().EquipmentPosition;
            parent = weaponsTransform;
        }
        else if (equipmentType == MachineConst.HAND_WEAPON)
        {
            equipmentObject = model.HandWeaponPrefab;
            equipmentPosition = equipmentObject.GetComponent<Weapon>().EquipmentPosition;
            parent = weaponsTransform;
        }
        else if (equipmentType == MachineConst.SHIELD)
        {
            equipmentObject = model.ShieldPrefab;
            if (equipmentObject == null) return;
            equipmentPosition = equipmentObject.GetComponent<Shield>().EquipmentPosition;
            parent = leftElbowTransform;
        }
        else
        {
            return;
        }

        // 装備位置を設定
        Vector2 newPosition = new Vector2(
            Util.GetSumByFloat(parent.position.x, equipmentPosition.x),
            Util.GetSumByFloat(parent.position.y, equipmentPosition.y)
        );

        // 装備生成
        GameObject newObject = Instantiate(equipmentObject, newPosition, Quaternion.identity, parent);

        // 装備の後処理
        newObject.name = equipmentType;
        if (equipmentType == MachineConst.HAND_WEAPON)
        {
            MoveJoint(newObject.transform, 45f);
            newObject.SetActive(false);
        }
        else if (equipmentType == MachineConst.SHIELD)
        {
            model.Shield = newObject.GetComponent<Shield>();
            newObject.SetActive(false);
        }
    }

    // 接触時の処理
    void OnCollisionEnter2D(Collision2D collision)
    {
        // ダウン中は処理しない
        if (model.IsDown) return;

        // 衝突した部分のコライダーを取得
        ContactPoint2D contact = collision.contacts[0];

        // 衝突したオブジェクトとの跳ね返りを計算
        model.CurrentVelocity = Common.CalculateVelocity(contact, model.CurrentVelocity, model.Spd);

        // 衝突イベントを判定
        int ret = Common.DecideEvent(contact, model.Def, model.Pilot.Model.Luck);
        if (ret > 0)
        {
            // 攻撃相手ユニットの情報を取得する
            Ballet collidedBallet = contact.collider.gameObject.GetComponent<Ballet>();
            Weapon collidedWeapon = contact.collider.gameObject.GetComponent<Weapon>();
            if (collidedBallet != null)
            {
                // 弾に被弾した時
                model.OpponentUnit = collidedBallet.Pilot.Unit;
            }
            else if (collidedWeapon != null)
            {
                // 武器に被弾したとき
                model.OpponentUnit = collidedWeapon.Pilot.Unit;
            }

            // 0より大きい場合、ダメージ処理
            model.CurrentHP = Common.DecreaseHP(model.CurrentHP, ret);

            // HPゲージ更新
            UpdateHPUI();

            // 爆風生成
            Common.GenerateExplosionWhenHitted(contact);

            // ダウンの状態に遷移
            StartCoroutine(ComeBackFromDown());
        }
    }

    // HPを回復する
    public void RecoverHP(int value)
    {
        // 全快のときはreturn
        if (model.HitPoint == model.CurrentHP) return;

        // HPを増やす
        model.CurrentHP = Common.IncreaseHP(model.HitPoint, model.CurrentHP, value);

        // HPゲージ更新
        UpdateHPUI();

    }

    // HPゲージ更新
    void UpdateHPUI()
    {
        ChargeUI chargeUI = transform.parent.Find("ParamUI").Find("HPGauge").GetComponent<ChargeUI>();
        chargeUI.UpdateChargeUI(model.CurrentHP, model.HitPoint);

        if (model.HitPoint == model.CurrentHP)
        {
            // HPがMAXの時はゲージを隠す
            chargeUI.ShowUI(false);
        }
    }

    // クラッシュする
    IEnumerator Crush()
    {
        // しばらくウェイト
        yield return new WaitForSeconds(COMEBACK_TIME);

        // 爆風を生成
        Instantiate(MasterData.Instance.ExplosionPrefab, transform.position, Quaternion.identity);

        // プレイヤー機のときはゲームオーバーToDo
        string pilotName = model.Pilot.Model.PilotName;
        if (pilotName == "アナタ") MonoCommon.Instance.Failed();

        // 撃破ユニットの撃破数、EXPを増やさせる
        model.OpponentUnit.IncrementKillCount();
        model.OpponentUnit.IncreaseEarnedExp(Common.GetGrantExp(transform.parent));

        // アイテム生成
        MonoCommon.Instance.GenerateItem(unit.DropItem, transform);

        // 本体を削除する
        Destroy(transform.parent.gameObject);
    }

    // ダウン中からの復帰
    IEnumerator ComeBackFromDown()
    {
        model.IsDown = true;
        StartCoroutine(Common.ComeBackFromDown(gameObject, COMEBACK_TIME, model.IsDown));

        do
        {
            // コライダーが有効になったときisDownをfalseにする
            if (gameObject.GetComponent<Collider2D>().enabled) model.IsDown = false;
            yield return null;
        } while (model.IsDown);
    }

    // パイロット情報を設定
    public void SetPilot(PilotController pilot)
    {
        model.Pilot = pilot;
    }

    // 機体の速度変更
    public void SetSpeed(Vector2 direction, bool isDashing)
    {
        model.IsDashing = isDashing;

        if (isDashing && !(model.IsDown || model.IsAction) && model.CurrentPP > 0)
        {
            DashBehaviour(true);

            // ダッシュ時はスピード+10、加速2倍
            int dashSpeed = MachineLogic.GetDashSpeed(model.Spd);
            int dashAccel = MachineLogic.GetDashAccel(model.Pilot.Model.Acceleration);

            // 目標速度を計算
            Vector2 targetVelocity = Common.GetTargetVelocity(direction, dashSpeed, model.IsDefence);

            // 加速度に応じた速度の適用
            model.CurrentVelocity = Common.GetCurrentVelocity(model.CurrentVelocity, targetVelocity, dashAccel);

            // PPを減らす
            model.CurrentPP = Common.DecreasePP(model.CurrentPP);

            // PPゲージ更新
            UpdatePPUI();
        }
        else
        {
            // 目標速度を計算
            Vector2 targetVelocity = Common.GetTargetVelocity(direction, model.Spd, model.IsDefence);

            // 速度変更
            model.CurrentVelocity = Common.GetCurrentVelocity(model.CurrentVelocity, targetVelocity, model.Pilot.Model.Acceleration);

            DashBehaviour(false);
        }
    }

    // PPゲージ更新
    void UpdatePPUI()
    {
        ChargeUI chargeUI = gameObject.transform.parent.Find("ParamUI").Find("PPGauge").GetComponent<ChargeUI>();
        chargeUI.UpdateChargeUI((int)model.CurrentPP, model.PropellantPoint);
    }

    // 機体を旋回
    public void Turn(float horizontal)
    {
        if (model.IsDown || model.IsAction)
        {
            // ダウン中もしくは、アクション中は動かせない
            return;
        }

        float angle;
        int newOrderInLayer;
        if (model.IsRight && horizontal < 0)
        {
            // 右向きの状態で左方向を押したとき
            // 左向きに変更
            angle = -180f;
            newOrderInLayer = 1;
        }
        else if (!model.IsRight && horizontal > 0)
        {
            // 左向きの状態で右方向を押したとき
            // 右向きに変更
            angle = 180f;
            newOrderInLayer = 4;
        }
        else
        {
            return;
        }
        model.IsRight = !model.IsRight;
        string rightArm = Util.GetArm(model.IsRight);
        string leftArm = Util.GetArm(!model.IsRight);

        // 旋回
        Vector3 current = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(current.x, current.y + angle, current.z);
        // 右腕の位置変更
        MoveJoint(transform.Find(rightArm), -45f);
        MoveJoint(transform.Find(rightArm).Find(MachineConst.ELBOW), 0f);
        // 武器、盾を持ち替える
        Transform anotherArm = transform.Find(rightArm).Find(MachineConst.ELBOW);
        Transform weaponsTransform = transform.Find(leftArm).Find(MachineConst.ELBOW).Find(MachineConst.WEAPONS);
        weaponsTransform.parent = anotherArm;
        weaponsTransform.Find(MachineConst.MAIN_WEAPON).GetComponent<Renderer>().sortingOrder = newOrderInLayer;
        weaponsTransform.Find(MachineConst.HAND_WEAPON).GetComponent<Renderer>().sortingOrder = newOrderInLayer;
        Transform shieldTransform = transform.Find(rightArm).Find(MachineConst.ELBOW).Find(MachineConst.SHIELD);
        if (shieldTransform != null)
        {
            shieldTransform.parent = transform.Find(leftArm).Find(MachineConst.ELBOW);
        }
        // 左腕の位置変更
        MoveJoint(transform.Find(leftArm), 0f);
        MoveJoint(transform.Find(leftArm).Find(MachineConst.ELBOW), 0f);
    }

    // 機体の関節を動かす（指定した角度に）
    void MoveJoint(Transform parts, float angle)
    {
        Vector3 currentRotation = parts.rotation.eulerAngles;
        parts.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, angle);
    }

    // ブースト時の挙動
    public void BoostBehaviour(bool isStartBoost)
    {
        if ((model.IsDown || model.IsAction) && !model.IsDefence)
        {
            // ダウン中もしくは、アクション中は動かせない
            // ただし防御中は動かせる
            return;
        }

        float angle;
        if (!model.IsBoosting && isStartBoost)
        {
            // 加速時に手足を傾けブースター表示
            model.IsBoosting = true;
            angle = 30f;
        }
        else if (model.IsBoosting && !isStartBoost)
        {
            // 停止時に手足を戻しブースター非表示
            model.IsBoosting = false;
            angle = 0f;
        }
        else
        {
            return;
        }

        string leftArm = Util.GetArm(!model.IsRight);

        if (!model.IsDefence)
            MoveJoint(transform.Find(leftArm), angle);
        MoveJoint(transform.Find(MachineConst.FOOTS), angle);
        GameObject booster = transform.Find(MachineConst.BODY).Find(MachineConst.BOOSTER).gameObject;
        booster.SetActive(model.IsBoosting);
        float rate = 0.1f;
        booster.transform.localScale = new Vector3(rate, rate, rate);
    }

    // ダッシュ時の挙動
    public void DashBehaviour(bool isDashing)
    {
        // ダッシュ時はブースト大きく
        GameObject booster = transform.Find(MachineConst.BODY).Find(MachineConst.BOOSTER).gameObject;
        if (booster.activeSelf)
        {
            float rate = isDashing ? 0.2f : 0.1f;
            booster.transform.localScale = new Vector3(rate, rate, rate);
        }
    }

    // 射撃
    public IEnumerator Shoot(GameObject target, Vector2 direction)
    {
        if (model.IsDown || model.IsAction)
        {
            // ダウン中もしくは、アクション中は動かせない
            yield break;
        }

        // 射撃中
        model.IsAction = true;

        // ターゲットに武器を向ける
        string rightArm = Util.GetArm(model.IsRight);
        Transform rightArmTransform = transform.Find(rightArm);
        if (target != null)
        {
            // ターゲットが決まっている場合（オート操作時）、ターゲットの方向に撃つ
            direction = target.transform.position - rightArmTransform.position;
        }
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // angle（腕の可動域）の範囲を制限
        float armAngle;
        if (model.IsRight)
        {
            if (angle < -90f || 90f < angle)
            {
                // 射撃方向が反対方向のときは正面に向かって撃つ
                angle = 0f;
            }
            angle = Mathf.Clamp(angle, -90f, 90f);
            armAngle = angle;
        }
        else
        {
            if (-90f < angle && angle < 90f)
            {
                // 射撃方向が反対方向のときは正面に向かって撃つ
                angle = 180f;
            }
            if (angle < 0f)
                angle += 360f;
            angle = Mathf.Clamp(angle, 90f, 270f);
            armAngle = 180f - angle;
        }
        Vector3 currentRotation = rightArmTransform.rotation.eulerAngles;
        MoveJoint(rightArmTransform, currentRotation.z - armAngle);

        // 発射
        Weapon weapon = rightArmTransform.Find(MachineConst.ELBOW).Find(MachineConst.WEAPONS).Find(MachineConst.MAIN_WEAPON).GetComponent<Weapon>();
        weapon.Launch(angle);

        // しばらくウェイト（射撃スキル依存）
        float waitTime = MachineLogic.CalculateWaitTime(model.Pilot.Model.Shootability);
        yield return new WaitForSeconds(waitTime);

        // 右腕を元の位置へ
        MoveJoint(rightArmTransform, currentRotation.z);
        model.IsAction = false;
    }

    public IEnumerator Slash()
    {
        if (model.IsDown || model.IsAction)
        {
            // ダウン中もしくは、アクション中は動かせない
            yield break;
        }

        // 斬撃中
        model.IsAction = true;

        // 右腕を振り上げる
        string rightArm = Util.GetArm(model.IsRight);
        Transform rightArmTransform = transform.Find(rightArm);
        Vector3 currentRotation = rightArmTransform.rotation.eulerAngles;
        MoveJoint(rightArmTransform, -105f);
        yield return null;
        yield return null;

        // サーベルに持ち替える
        rightArmTransform.Find(MachineConst.ELBOW).Find(MachineConst.WEAPONS).Find(MachineConst.MAIN_WEAPON).gameObject.SetActive(false);
        rightArmTransform.Find(MachineConst.ELBOW).Find(MachineConst.WEAPONS).Find(MachineConst.HAND_WEAPON).gameObject.SetActive(true);

        // 効果音
        SoundManager.Instance.PlaySE(SESoundData.SE.Slash1);

        // 右腕を振り下ろす
        float upRotation = -105f;
        float swingSpeed = Calculator.CalcSwingSpeed(model.Pilot.Model.Slashability);
        while(upRotation < -1f)
        {
            float newRotation = Mathf.LerpAngle(upRotation, 0f, Time.deltaTime * swingSpeed);
            MoveJoint(rightArmTransform, newRotation);
            upRotation = newRotation;
            yield return null;
        }
        // しばらくウェイト（斬撃スキル依存）
        rightArmTransform.Find(MachineConst.ELBOW).Find(MachineConst.WEAPONS).Find(MachineConst.HAND_WEAPON).gameObject.SetActive(false);
        float waitTime = Calculator.CalcWaitTime(model.Pilot.Model.Slashability);
        yield return new WaitForSeconds(waitTime);

        // メイン武器に持ち替える
        rightArmTransform.Find(MachineConst.ELBOW).Find(MachineConst.WEAPONS).Find(MachineConst.MAIN_WEAPON).gameObject.SetActive(true);

        // 右腕を元の位置へ
        MoveJoint(rightArmTransform, currentRotation.z);
        model.IsAction = false;
    }

    // 推進剤のチャージ
    IEnumerator ChargePropellant()
    {
        while (true)
        {
            ChargeUI chargeUI = gameObject.transform.parent.Find("ParamUI").Find("PPGauge").GetComponent<ChargeUI>();

            float waitTime = 0.1f;
            yield return new WaitForSeconds(waitTime);

            // PPが最大値に達していない、かつダッシュ中でない場合のみチャージ
            if (model.CurrentPP < model.PropellantPoint && !model.IsDashing)
            {
                // チャージ速度に応じて PPを増やす
                float chargeSpeed = Calculator.calculateChargeSpeed(model.PropellantPoint);
                model.CurrentPP = Mathf.Min(model.CurrentPP + chargeSpeed, model.PropellantPoint);
                chargeUI.UpdateChargeUI((int)model.CurrentPP, model.PropellantPoint);
            }
            else
            {
                // MAXの場合、waitTimeをPropellantPointUIに渡す
                chargeUI.HideTime(waitTime);
            }
        }
    }

    // 防御
    public IEnumerator Defence()
    {
        if (model.IsDown || model.IsAction)
        {
            // ダウン中もしくは、アクション中は動かせない
            yield break;
        }
        
        string leftArm = Util.GetArm(!model.IsRight);
        Transform shieldTransform = transform.Find(leftArm).Find(MachineConst.ELBOW).Find(MachineConst.SHIELD);
        if (shieldTransform == null)
        {
            // 盾装備なしのときは防御できない
            yield break;
        }
        if (!model.Shield.IsAlive)
        {
            // 盾装備なしのときは防御できない
            yield break;
        }

        // 防御中
        model.IsAction = true;
        model.IsDefence = true;

        string rightArm = Util.GetArm(model.IsRight);
        Transform rightArmTransform = transform.Find(rightArm);
        Vector3 currentRightArmRotation = rightArmTransform.rotation.eulerAngles;
        Vector3 currentRightElbow = rightArmTransform.Find(MachineConst.ELBOW).rotation.eulerAngles;

        Transform leftArmTransform = transform.Find(leftArm);
        Vector3 currentLeftArmRotation = leftArmTransform.rotation.eulerAngles;
        Vector3 currentLeftElbow = leftArmTransform.Find(MachineConst.ELBOW).rotation.eulerAngles;

        // 左腕を前に出す
        MoveJoint(leftArmTransform, -75f);
        MoveJoint(leftArmTransform.Find(MachineConst.ELBOW), 0f);
        // 右腕を直す
        MoveJoint(rightArmTransform, 0f);
        MoveJoint(rightArmTransform.Find(MachineConst.ELBOW), 0f);
        // メイン武器を外す
        rightArmTransform.Find(MachineConst.ELBOW).Find(MachineConst.WEAPONS).Find(MachineConst.MAIN_WEAPON).gameObject.SetActive(false);
        // 盾を構える
        leftArmTransform.Find(MachineConst.ELBOW).Find(MachineConst.SHIELD).gameObject.SetActive(true);

        while(model.IsDefence)
        {
            // 防御中
            yield return null;

            // シールドが壊れたら防御解除
            if (!model.Shield.IsAlive)
            {
                model.IsDefence = false;
            }
        }

        // 防御解除
        // 盾外す
        leftArmTransform.Find(MachineConst.ELBOW).Find(MachineConst.SHIELD).gameObject.SetActive(false);
        // メイン武器構える
        rightArmTransform.Find(MachineConst.ELBOW).Find(MachineConst.WEAPONS).Find(MachineConst.MAIN_WEAPON).gameObject.SetActive(true);
        // 右腕元の位置へ
        MoveJoint(rightArmTransform, currentRightArmRotation.z);
        MoveJoint(rightArmTransform.Find(MachineConst.ELBOW), currentRightElbow.z);
        // 左腕元の位置へ
        MoveJoint(leftArmTransform, currentLeftArmRotation.z);
        MoveJoint(leftArmTransform.Find(MachineConst.ELBOW), currentLeftElbow.z);

        model.IsAction = false;
    }

    // 防御解除
    public void OffDefence()
    {
        model.IsDefence = false;
    }

    // 最大HP設定
    public void SetHitPoint(int hitPoint)
    {
        model.HitPoint = hitPoint;
    }

    // データ初期化
    public void InitializeData()
    {
        model = new MachineModel();
        Unit unit = transform.parent.GetComponent<Unit>();
        model.MainWeaponPrefab = Common.GetPrefabMapping(unit.MainWeaponKey, MasterData.Instance.MainWeaponMaster);
        model.HandWeaponPrefab = Common.GetPrefabMapping(unit.HandWeaponKey, MasterData.Instance.HandWeaponMaster);
        model.ShieldPrefab = Common.GetPrefabMapping(unit.ShieldKey, MasterData.Instance.ShieldMaster);
    }
}
