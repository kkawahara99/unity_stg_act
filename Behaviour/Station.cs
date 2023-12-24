using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Station : MonoBehaviour
{
    [SerializeField] private int hitPoint; // 耐久力（HP）
    public int HitPoint { get => hitPoint; }
    [SerializeField] private int atc; // 火力（Act）
    public int Atc { get => atc; }
    [SerializeField] private int def; // 装甲（Def）
    public int Def { get => def; }
    [SerializeField] private int luck; // 運

    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject unitPrefab;

    [SerializeField] private List<Unit> units;
    public List<Unit> Units { get => units; }
    [SerializeField] private List<GameObject> machineObjects;
    public List<GameObject> MachineObjects { get => machineObjects; }
    [SerializeField] private List<GameObject> pilotObjects;
    public List<GameObject> PilotObjects { get => pilotObjects; }
    [SerializeField] private float deployRate; // デプロイHP率

    const float COME_BACK_TIME = 0.2f; // ダウン復帰時間
    private bool isDown; // ダウン中かどうか
    private int currentHP; // 現在のHP
    private bool isDead;
    private bool isDeploy;

    void Start()
    {
        // 必要な他コンポーネント取得

        // ステータス初期化
        currentHP = hitPoint;

        // 初期ユニット登録(仮)
        Vector2 unitPosition = new Vector2(transform.position.x + 0.5f, transform.position.y);
        if (gameObject.tag == "Blue")
        {
            units = new List<Unit>();
            GameObject unitObject = Instantiate(unitPrefab, unitPosition, Quaternion.identity);
            Unit unit = unitObject.GetComponent<Unit>();
            unit.SetMachinePrefab(machineObjects[0]);
            unit.SetPilotPrefab(pilotObjects[0]);
            unit.SetColor(new Color(0.5f, 0.5f, 1f, 1f));

            AddUnit(unit);

            // ユニットを展開する
            DeployUnits();

            // プレイヤーにカメラを合わせる
            CameraController cameraController = GameObject.Find("Main Camera").GetComponent<CameraController>();
            cameraController.SetUnit();
            cameraController.trackingPlayer(false);
        }
    }

    void Update()
    {
        // 0のときクラッシュする
        if (currentHP == 0 && !isDead)
        {
            isDead = true;
            StartCoroutine(Crush());
        }

        if ((float)currentHP / (float)hitPoint <= deployRate && !isDeploy && !isDead)
        {
            isDeploy = true;
            // 残りHPがデプロイHP率以下の時ユニット展開する
            GenerateUnit(new Vector2(transform.position.x - 0.5f, transform.position.y));
            DeployUnits();
        }
    }

    // ユニット生成
    void GenerateUnit(Vector2 position)
    {
        GameObject unitObject = Instantiate(unitPrefab, position, Quaternion.identity, transform);
        Unit unit = unitObject.GetComponent<Unit>();
        unit.SetMachinePrefab(machineObjects[0]);
        unit.SetPilotPrefab(pilotObjects[0]);
        unit.SetColor(new Color(1f, 0.5f, 0.5f, 1f));

        AddUnit(unit);
    }

    // ユニットを展開する
    void DeployUnits()
    {
        foreach (Unit unit in units)
        {
            // unit.SetPosition(position);
            unit.DeployMachine();
            unit.DeployPilot();
        }
    }

    // 接触時の処理
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDown)
        {
            // 衝突した部分のコライダーを取得
            ContactPoint2D contact = collision.contacts[0];

            // 衝突イベントを判定
            int ret = Common.Instance.DecideEvent(contact, def, luck);
            if (ret > 0)
            {
                // 0より大きい場合、爆風生成
                Common.Instance.GenerateExplosionWhenHitted(contact);

                // ダメージ処理
                currentHP = Common.Instance.DecreaseHP(currentHP, ret);

                // HPゲージ更新
                UpdateHPUI();

                // ダウンの状態に遷移
                StartCoroutine(ComeBackFromDown());
            }
        }
    }

    // HPゲージ更新
    void UpdateHPUI()
    {
        ChargeUI chargeUI = gameObject.transform.Find("ParamUI").Find("HPGauge").GetComponent<ChargeUI>();
        chargeUI.UpdateChargeUI(currentHP, hitPoint);
    }

    // ダウン中からの復帰
    IEnumerator ComeBackFromDown()
    {
        isDown = true;
        StartCoroutine(Common.Instance.ComeBackFromDown(gameObject, COME_BACK_TIME, isDown));

        do
        {
            // コライダーが有効になったときisDownをfalseにする
            if (gameObject.GetComponent<Collider2D>().enabled) isDown = false;
            yield return null;
        } while (isDown);
    }

    // HPを減らす
    void DecreaseHP(int damageValue)
    {
        // 現在HPを減らす
        // 0以下にならないようにする
        currentHP = Mathf.Max(currentHP - damageValue, 0);

        // 0のときクラッシュする
        if (currentHP == 0)
        {
            StartCoroutine(Crush());
        }
    }
    // クラッシュする
    IEnumerator Crush()
    {
        // しばらくウェイト
        yield return new WaitForSeconds(COME_BACK_TIME);

        // 爆風を生成
        GameObject explosionObject = Instantiate(explosionPrefab, gameObject.transform.position, Quaternion.identity);
        explosionObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

        // 自軍ステーションの場合ゲームオーバーToDo
        if (gameObject.tag == "Blue") Common.Instance.Failed();

        // 敵軍ステーションの場合クリアToDo
        if (gameObject.tag == "Red") Common.Instance.Succeeded();

        // 削除する
        Destroy(gameObject);
    }

    public void AddUnit(Unit unit)
    {
        units.Add(unit);
    }

    public void AddMachineObjects(GameObject machineObject)
    {
        machineObjects.Add(machineObject);
    }

    public void AddPilotObjects(GameObject pilotObject)
    {
        pilotObjects.Add(pilotObject);
    }

    public void RemoveUnit(Unit unit)
    {
        units.Remove(unit);
    }

    public void RemoveMachineObjects(GameObject machineObject)
    {
        machineObjects.Remove(machineObject);
    }

    public void RemovePilotObjects(GameObject pilotObject)
    {
        pilotObjects.Remove(pilotObject);
    }
}
