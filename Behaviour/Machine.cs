using System.Collections;
using UnityEngine;

public class Machine : MonoBehaviour
{

    /* マシンパラメータ */
    [SerializeField] private string machineName; // マシン名
    public string MachineachineName { get => machineName; }
    [SerializeField] private int hitPoint; // 耐久力（HP）
    public int HitPoint { get => hitPoint; }
    [SerializeField] private int propellantPoint; // 推進力（PP）
    public int PropellantPoint { get => propellantPoint; }
    [SerializeField] private int atc; // 火力（Act）
    public int Atc { get => atc; }
    [SerializeField] private int def; // 装甲（Def）
    public int Def { get => def; }
    [SerializeField] private int spd; // 機動性（Speed）
    public int Spd { get => spd; }
    [SerializeField] private GameObject mainWeaponPrefab; // メイン装備
    public GameObject MainWeaponPrefab { get => mainWeaponPrefab; }
    [SerializeField] private GameObject handWeaponPrefab; // 近距離装備
    public GameObject HandWeaponPrefab { get => handWeaponPrefab; }
    [SerializeField] private GameObject shieldPrefab; // 盾装備
    public GameObject ShieldPrefab { get => shieldPrefab; }
    [SerializeField] private GameObject explosionPrefab;

    private Unit unit; // パラメータ
    const float COMEBACK_TIME = 0.2f; // ダウンからの復帰時間
    private bool isRight; // 右向きかどうか

    private Rigidbody2D rb;
    private bool isBoosting = false; // ブースト中かどうか
    private bool isDown; // ダウン中かどうか
    public bool IsDown  { get => isDown; }
    private bool isAction; // アクション中かどうか
    private bool isDashing; // ダッシュ中かどうか
    private bool isDefence; // 防御中かどうか
    private Vector2 currentVelocity;  // 現在の速度
    private int currentHP; // 現在のHP
    private float currentPP; // 現在の推進力
    private Shield shield; // シールド情報
    private MapManager mapManager; // マップ情報
    private Pilot pilot; // パイロット情報
    private bool isDead; // 死んでるかどうか
    private Unit _opponentUnit; // ダメージ食らわされた相手ユニット

    void Start()
    {
        // 必要な他コンポーネント取得
        rb = gameObject.transform.parent.GetComponent<Rigidbody2D>();
        mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();
        unit = gameObject.transform.parent.GetComponent<Unit>();

        // ステータス初期化
        currentHP = hitPoint;
        currentPP = (float)propellantPoint;

        // メイン武器装備
        Equip("MainWeapon");

        // 近距離武器装備
        Equip("HandWeapon");

        // 盾装備
        Equip("Shield");

        // 回転を制御
        rb.freezeRotation = true;  // 回転を固定

        // 右向きのときは右を向く
        if (unit.IsRight)
        {
            isRight = false;
            Turn(1f);
        }

        // スプライトの色を変更
        Common.Instance.SetColors(unit.Color, transform);

        // 推進剤チャージコルーチン開始
        StartCoroutine(ChargePropellant());

    }

    void Update()
    {
        // 0のときクラッシュする
        if (currentHP == 0 && !isDead)
        {
            isDead = true;
            StartCoroutine(Crush());
        }
    }

    // 装備
    void Equip(string equipmentType)
    {
        GameObject equipmentObject = null;
        Vector2 equipmentPosition = Vector2.zero;
        Vector2 parentPosition = Vector2.zero;
        Transform parent;
        
        // 右腕の武器、左腕のtransformをそれぞれ取得
        Transform weaponsTransform = transform.Find("ArmBackGround").Find("Elbow").Find("Weapons");
        Transform leftElbowTransform = transform.Find("ArmForeGround").Find("Elbow");

        // 装備種別によって事前準備
        if (equipmentType == "MainWeapon")
        {
            equipmentObject = mainWeaponPrefab;
            equipmentPosition = equipmentObject.GetComponent<Weapon>().EquipmentPosition;
            parent = weaponsTransform;
        }
        else if (equipmentType == "HandWeapon")
        {
            equipmentObject = handWeaponPrefab;
            equipmentPosition = equipmentObject.GetComponent<Weapon>().EquipmentPosition;
            parent = weaponsTransform;
        }
        else if (equipmentType == "Shield")
        {
            equipmentObject = shieldPrefab;
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
            parent.position.x + equipmentPosition.x,
            parent.position.y + equipmentPosition.y);

        // 装備生成
        GameObject newObject = Instantiate(equipmentObject, newPosition, Quaternion.identity, parent);

        // 装備の後処理
        newObject.name = equipmentType;
        if (equipmentType == "HandWeapon")
        {
            MoveJoint(newObject.transform, 45f);
            newObject.SetActive(false);
        }
        else if (equipmentType == "Shield")
        {
            shield = newObject.GetComponent<Shield>();
            newObject.SetActive(false);
        }
    }

    void FixedUpdate()
    {
        // 機体の移動速度を更新
        rb.velocity = currentVelocity;

        // 移動位置を制限
        Common.Instance.RestrictMovePosition(rb, mapManager.MaxX, mapManager.MinX, mapManager.MaxY, mapManager.MinY);
    }

    // 接触時の処理
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDown)
        {
            // 衝突した部分のコライダーを取得
            ContactPoint2D contact = collision.contacts[0];

            // 衝突したオブジェクトとの跳ね返りを計算
            currentVelocity = Common.Instance.CalculateVelocity(contact, currentVelocity, spd);

            // 衝突イベントを判定
            int ret = Common.Instance.DecideEvent(contact, def, pilot.Luck);
            if (ret > 0)
            {
                // 攻撃相手ユニットの情報を取得する
                Ballet collidedBallet = contact.collider.gameObject.GetComponent<Ballet>();
                Weapon collidedWeapon = contact.collider.gameObject.GetComponent<Weapon>();
                if (collidedBallet != null)
                {
                    // 弾に被弾した時
                    _opponentUnit = collidedBallet.Pilot.Unit;
                }
                else if (collidedWeapon != null)
                {
                    // 武器に被弾したとき
                    _opponentUnit = collidedWeapon.Pilot.Unit;
                }

                // 0より大きい場合、ダメージ処理
                currentHP = Common.Instance.DecreaseHP(currentHP, ret);

                // HPゲージ更新
                UpdateHPUI();

                // 爆風生成
                Common.Instance.GenerateExplosionWhenHitted(contact);

                // ダウンの状態に遷移
                StartCoroutine(ComeBackFromDown());
            }
        }
    }

    // HPを回復する
    public void RecoverHP(int recoveryValue)
    {
        // 全快のときはreturn
        if (hitPoint == currentHP) return;

        // HPを増やす
        currentHP = Common.Instance.IncreaseHP(hitPoint, currentHP, recoveryValue);

        // HPゲージ更新
        UpdateHPUI();

    }

    // HPゲージ更新
    void UpdateHPUI()
    {
        ChargeUI chargeUI = gameObject.transform.parent.Find("ParamUI").Find("HPGauge").GetComponent<ChargeUI>();
        chargeUI.UpdateChargeUI(currentHP, hitPoint);

        if (hitPoint == currentHP)
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
        Instantiate(explosionPrefab, gameObject.transform.position, Quaternion.identity);

        // ユキノのときはゲームオーバーToDo
        string pilotName = transform.parent.Find("Pilot").GetComponent<Pilot>().PilotName;
        if (pilotName == "Yukino") Common.Instance.Failed();

        // 撃破ユニットの撃破数を増やさせる
        _opponentUnit.IncrementKillCount();

        // アイテム生成
        Common.Instance.GenerateItem(unit.DropItem, transform);

        // 本体を削除する
        Destroy(gameObject.transform.parent.gameObject);
    }

    // ダウン中からの復帰
    IEnumerator ComeBackFromDown()
    {
        isDown = true;
        StartCoroutine(Common.Instance.ComeBackFromDown(gameObject, COMEBACK_TIME, isDown));

        do
        {
            // コライダーが有効になったときisDownをfalseにする
            if (gameObject.GetComponent<Collider2D>().enabled) isDown = false;
            yield return null;
        } while (isDown);
    }

    // パイロット情報を設定
    public void SetPilot(Pilot pilot)
    {
        this.pilot = pilot;
    }

    // 機体の速度変更
    public void SetSpeed(Vector2 direction, bool isDashing)
    {
        this.isDashing = isDashing;

        if (isDashing && !(isDown || isAction) && currentPP > 0)
        {
            DashBehaviour(true);

            // ダッシュ時はスピード+10、加速2倍
            int dashSpeed = Calculator.Instance.GetDashSpeed(spd);
            int dashAccel = Calculator.Instance.GetDashAccel(pilot.Acceleration);

            // 目標速度を計算
            Vector2 targetVelocity = Calculator.Instance.calculateTargetVelocity(direction, dashSpeed, isDefence);

            // 加速度に応じた速度の適用
            currentVelocity = Calculator.Instance.calculateCurrentVelocity(currentVelocity, targetVelocity, dashAccel);

            // PPを減らす
            currentPP = Common.Instance.DecreasePP(currentPP);

            // PPゲージ更新
            UpdatePPUI();
        }
        else
        {
            // 目標速度を計算
            Vector2 targetVelocity = Calculator.Instance.calculateTargetVelocity(direction, spd, isDefence);

            // 速度変更
            currentVelocity = Calculator.Instance.calculateCurrentVelocity(currentVelocity, targetVelocity, pilot.Acceleration);

            DashBehaviour(false);
        }
    }

    // PPゲージ更新
    void UpdatePPUI()
    {
        ChargeUI chargeUI = gameObject.transform.parent.Find("ParamUI").Find("PPGauge").GetComponent<ChargeUI>();
        chargeUI.UpdateChargeUI((int)currentPP, propellantPoint);
    }

    // 機体を旋回
    public void Turn(float horizontal)
    {
        if (isDown || isAction)
        {
            // ダウン中もしくは、アクション中は動かせない
            return;
        }

        float angle;
        string rightArm;
        string leftArm;
        int newOrderInLayer;
        if (isRight && horizontal < 0)
        {
            // 右向きの状態で左方向を押したとき
            // 左向きに変更
            isRight = false;
            angle = -180f;
            rightArm = "ArmBackGround";
            leftArm = "ArmForeGround";
            newOrderInLayer = 1;
        }
        else if (!isRight && horizontal > 0.01f)
        {
            // 左向きの状態で右方向を押したとき
            // 右向きに変更
            isRight = true;
            angle = 180f;
            rightArm = "ArmForeGround";
            leftArm = "ArmBackGround";
            newOrderInLayer = 4;
        }
        else
        {
            return;
        }

        // 旋回
        Vector3 current = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(current.x, current.y + angle, current.z);
        // 右腕の位置変更
        MoveJoint(transform.Find(rightArm), -45f);
        MoveJoint(transform.Find(rightArm).Find("Elbow"), 0f);
        // 武器、盾を持ち替える
        Transform anotherArm = transform.Find(rightArm).Find("Elbow");
        Transform weaponsTransform = transform.Find(leftArm).Find("Elbow").Find("Weapons");
        weaponsTransform.parent = anotherArm;
        weaponsTransform.Find("MainWeapon").GetComponent<Renderer>().sortingOrder = newOrderInLayer;
        weaponsTransform.Find("HandWeapon").GetComponent<Renderer>().sortingOrder = newOrderInLayer;
        Transform shieldTransform = transform.Find(rightArm).Find("Elbow").Find("Shield");
        if (shieldTransform != null)
        {
            shieldTransform.parent = transform.Find(leftArm).Find("Elbow");
        }
        // 左腕の位置変更
        MoveJoint(transform.Find(leftArm), 0f);
        MoveJoint(transform.Find(leftArm).Find("Elbow"), 0f);
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
        if ((isDown || isAction) && !isDefence)
        {
            // ダウン中もしくは、アクション中は動かせない
            // ただし防御中は動かせる
            return;
        }

        float angle;
        if (!isBoosting && isStartBoost)
        {
            // 加速時に手足を傾けブースター表示
            isBoosting = true;
            angle = 30f;
        }
        else if (isBoosting && !isStartBoost)
        {
            // 停止時に手足を戻しブースター非表示
            isBoosting = false;
            angle = 0f;
        }
        else
        {
            return;
        }

        string leftArm;
        if (isRight)
            leftArm = "ArmBackGround";
        else
            leftArm = "ArmForeGround";

        if (!isDefence)
            MoveJoint(transform.Find(leftArm), angle);
        MoveJoint(transform.Find("Foots"), angle);
        GameObject booster = transform.Find("Body").Find("Booster").gameObject;
        booster.SetActive(isBoosting);
        float rate = 0.1f;
        booster.transform.localScale = new Vector3(rate, rate, rate);
    }

    // ダッシュ時の挙動
    public void DashBehaviour(bool isDashing)
    {
        // ダッシュ時はブースト大きく
        GameObject booster = transform.Find("Body").Find("Booster").gameObject;
        if (booster.activeSelf)
        {
            float rate = isDashing ? 0.2f : 0.1f;
            booster.transform.localScale = new Vector3(rate, rate, rate);
        }
    }

    // 射撃
    public IEnumerator Shoot(GameObject target, Vector2 direction)
    {
        if (isDown || isAction)
        {
            // ダウン中もしくは、アクション中は動かせない
            yield break;
        }

        // 射撃中
        isAction = true;

        // ターゲットに武器を向ける
        string rightArm = isRight ? "ArmForeGround" : "ArmBackGround";
        Transform rightArmTransform = transform.Find(rightArm);
        if (target != null)
        {
            // ターゲットが決まっている場合（オート操作時）、ターゲットの方向に撃つ
            direction = target.transform.position - rightArmTransform.position;
        }
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // angle（腕の可動域）の範囲を制限
        float armAngle;
        if (isRight)
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
        Weapon weapon = rightArmTransform.Find("Elbow").Find("Weapons").Find("MainWeapon").GetComponent<Weapon>();
        weapon.Launch(angle);

        // しばらくウェイト（射撃スキル依存）
        float waitTime = Calculator.Instance.CalculateWaitTime(pilot.Shootability);
        yield return new WaitForSeconds(waitTime);

        // 右腕を元の位置へ
        MoveJoint(rightArmTransform, currentRotation.z);
        isAction = false;
    }

    public IEnumerator Slash()
    {
        if (isDown || isAction)
        {
            // ダウン中もしくは、アクション中は動かせない
            yield break;
        }

        // 斬撃中
        isAction = true;

        // 右腕を振り上げる
        string rightArm = isRight ? "ArmForeGround" : "ArmBackGround";
        Transform rightArmTransform = transform.Find(rightArm);
        Vector3 currentRotation = rightArmTransform.rotation.eulerAngles;
        MoveJoint(rightArmTransform, -105f);
        yield return null;
        yield return null;

        // サーベルに持ち替える
        rightArmTransform.Find("Elbow").Find("Weapons").Find("MainWeapon").gameObject.SetActive(false);
        rightArmTransform.Find("Elbow").Find("Weapons").Find("HandWeapon").gameObject.SetActive(true);

        // 効果音
        SoundManager.Instance.PlaySE(SESoundData.SE.Slash1);

        // 右腕を振り下ろす
        float upRotation = -105f;
        float swingSpeed = Calculator.Instance.CalculateSwingSpeed(pilot.Slashability);
        while(upRotation < -1f)
        {
            float newRotation = Mathf.LerpAngle(upRotation, 0f, Time.deltaTime * swingSpeed);
            MoveJoint(rightArmTransform, newRotation);
            upRotation = newRotation;
            yield return null;
        }
        // しばらくウェイト（斬撃スキル依存）
        rightArmTransform.Find("Elbow").Find("Weapons").Find("HandWeapon").gameObject.SetActive(false);
        float waitTime = Calculator.Instance.CalculateWaitTime(pilot.Slashability);
        yield return new WaitForSeconds(waitTime);

        // メイン武器に持ち替える
        rightArmTransform.Find("Elbow").Find("Weapons").Find("MainWeapon").gameObject.SetActive(true);

        // 右腕を元の位置へ
        MoveJoint(rightArmTransform, currentRotation.z);
        isAction = false;
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
            if (currentPP < propellantPoint && !isDashing)
            {
                // チャージ速度に応じて PPを増やす
                float chargeSpeed = Calculator.Instance.calculateChargeSpeed(propellantPoint);
                currentPP = Mathf.Min(currentPP + chargeSpeed, propellantPoint);
                chargeUI.UpdateChargeUI((int)currentPP, propellantPoint);
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
        if (isDown || isAction)
        {
            // ダウン中もしくは、アクション中は動かせない
            yield break;
        }
        
        string leftArm = isRight ? "ArmBackGround" : "ArmForeGround";
        Transform shieldTransform = transform.Find(leftArm).Find("Elbow").Find("Shield");
        if (shieldTransform == null)
        {
            // 盾装備なしのときは防御できない
            yield break;
        }
        if (!shield.IsAlive)
        {
            // 盾装備なしのときは防御できない
            yield break;
        }

        // 防御中
        isAction = true;
        isDefence = true;

        string rightArm = isRight ? "ArmForeGround" : "ArmBackGround";
        Transform rightArmTransform = transform.Find(rightArm);
        Vector3 currentRightArmRotation = rightArmTransform.rotation.eulerAngles;
        Vector3 currentRightElbow = rightArmTransform.Find("Elbow").rotation.eulerAngles;

        Transform leftArmTransform = transform.Find(leftArm);
        Vector3 currentLeftArmRotation = leftArmTransform.rotation.eulerAngles;
        Vector3 currentLeftElbow = leftArmTransform.Find("Elbow").rotation.eulerAngles;

        // 左腕を前に出す
        MoveJoint(leftArmTransform, -75f);
        MoveJoint(leftArmTransform.Find("Elbow"), 0f);
        // 右腕を直す
        MoveJoint(rightArmTransform, 0f);
        MoveJoint(rightArmTransform.Find("Elbow"), 0f);
        // メイン武器を外す
        rightArmTransform.Find("Elbow").Find("Weapons").Find("MainWeapon").gameObject.SetActive(false);
        // 盾を構える
        leftArmTransform.Find("Elbow").Find("Shield").gameObject.SetActive(true);

        while(isDefence)
        {
            // 防御中
            yield return null;

            // シールドが壊れたら防御解除
            if (!shield.IsAlive)
            {
                isDefence = false;
            }
        }

        // 防御解除
        // 盾外す
        leftArmTransform.Find("Elbow").Find("Shield").gameObject.SetActive(false);
        // メイン武器構える
        rightArmTransform.Find("Elbow").Find("Weapons").Find("MainWeapon").gameObject.SetActive(true);
        // 右腕元の位置へ
        MoveJoint(rightArmTransform, currentRightArmRotation.z);
        MoveJoint(rightArmTransform.Find("Elbow"), currentRightElbow.z);
        // 左腕元の位置へ
        MoveJoint(leftArmTransform, currentLeftArmRotation.z);
        MoveJoint(leftArmTransform.Find("Elbow"), currentLeftElbow.z);

        isAction = false;
    }

    // 防御解除
    public void OffDefence()
    {
        isDefence = false;
    }
}
