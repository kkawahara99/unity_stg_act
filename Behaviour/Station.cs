using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Station : MonoBehaviour
{
    [SerializeField] private int hitPoint; // 耐久力（HP）
    public int HitPoint { get => hitPoint; }
    [SerializeField] private int atk; // 火力（Act）
    public int Atk{ get => atk; }
    [SerializeField] private int def; // 装甲（Def）
    public int Def { get => def; }
    [SerializeField] private int luck; // 運
    [SerializeField] private StationData stationData;
    public StationData StationData { get => stationData; }

    [SerializeField] private int grantExp;
    [SerializeField] private int grantCoin;

    const float COME_BACK_TIME = 0.2f; // ダウン復帰時間
    const float DEPLOY_RATE = 0.99f; // 展開レート（残HPが99%以下のとき待機機展開）
    private bool isDown; // ダウン中かどうか
    private int currentHP; // 現在のHP
    private bool isDead;
    private bool isDeploy;

    void Start()
    {
        if (gameObject.tag == TagConst.BLUE)
        {
            // プレイヤーサイドのときの初期設定
            InitializeData();

            // 味方出撃
            GenerateUnit();
            isDeploy = true;

            // プレイヤーにカメラを合わせる
            CameraController cameraController = GameObject.Find("Main Camera").GetComponent<CameraController>();
            cameraController.SetUnit();
            cameraController.trackingPlayer(false);
        }

        // ステータス初期化
        currentHP = hitPoint;
    }

    void Update()
    {
        // 0のときクラッシュする
        if (currentHP == 0 && !isDead)
        {
            isDead = true;
            StartCoroutine(Crush());
        }

        if ((float)currentHP / (float)hitPoint <= DEPLOY_RATE && !isDeploy && !isDead)
        {
            isDeploy = true;
            // 残りHPがデプロイHP率以下の時ユニット展開する
            GenerateUnit();
        }
    }

    // ユニット生成
    void GenerateUnit()
    {
        // ユニット生成位置を定義
        int factor = transform.tag == TagConst.BLUE ? 1 : -1;
        Vector2[] generatePositions = new Vector2[5];
        generatePositions[0] = new Vector2(transform.position.x + 0.5f * factor, transform.position.y);
        generatePositions[1] = new Vector2(transform.position.x, transform.position.y + 0.75f);
        generatePositions[2] = new Vector2(transform.position.x, transform.position.y - 0.75f);
        generatePositions[3] = new Vector2(transform.position.x + 0.5f * factor, transform.position.y + 0.75f);
        generatePositions[4] = new Vector2(transform.position.x + 0.5f * factor, transform.position.y - 0.75f);

        GameObject unitPrefab = MasterData.Instance.UnitMaster;
        for (int i = 0; i < stationData.unitDatas.Count; i++)
        {
            // ユニットを生成する
            GameObject unitObject = Instantiate(unitPrefab, generatePositions[i], Quaternion.identity);
            unitObject.tag = transform.tag;
            Unit unit = unitObject.GetComponent<Unit>();
            unit.unitNo = i;
            unit.InitializeData();
            DeployUnit(unitObject);
        }
    }

    // ユニットを展開する
    void DeployUnit(GameObject unitObject)
    {
        Unit unit = unitObject.GetComponent<Unit>();
        unit.DeployMachine();
        unit.DeployPilot();
    }

    // 接触時の処理
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDown)
        {
            // 衝突した部分のコライダーを取得
            ContactPoint2D contact = collision.contacts[0];

            // 衝突イベントを判定
            int ret = Common.DecideEvent(contact, def, luck);
            if (ret > 0)
            {
                // 0より大きい場合、爆風生成
                Common.GenerateExplosionWhenHitted(contact);

                // ダメージ処理
                currentHP = Common.DecreaseHP(currentHP, ret);

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
        StartCoroutine(Common.ComeBackFromDown(gameObject, COME_BACK_TIME, isDown));

        do
        {
            // コライダーが有効になったときisDownをfalseにする
            if (gameObject.GetComponent<Collider2D>().enabled) isDown = false;
            yield return null;
        } while (isDown);
    }
    
    // クラッシュする
    IEnumerator Crush()
    {
        // しばらくウェイト
        yield return new WaitForSeconds(COME_BACK_TIME);

        // 爆風を生成
        GameObject explosionObject = Instantiate(MasterData.Instance.ExplosionPrefab, gameObject.transform.position, Quaternion.identity);
        explosionObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

        // 自軍ステーションの場合ゲームオーバーToDo
        if (gameObject.tag == TagConst.BLUE) MonoCommon.Instance.Failed();

        // 敵軍ステーションの場合クリアToDo
        if (gameObject.tag == TagConst.RED) MonoCommon.Instance.Succeeded();

        // 全ユニットのEXPを増やさせる
        Unit[] units = FindObjectsOfType<Unit>();
        foreach (Unit unit in units)
        {
            unit.IncreaseEarnedExp(grantExp);
        }
        // コインを増やす
        DataManager.Instance.currentCoinCount += grantCoin;

        // 削除する
        Destroy(gameObject);
    }

    // 敵が侵入してきたら待機機展開
    void OnTriggerEnter2D(Collider2D other)
    {
        // 展開済みの場合return
        if (isDeploy) return;

        string opponentTag = Util.GetOpponentTag(transform.tag);
        if (other.transform.parent == null) return;
        if (other.transform.parent.tag == opponentTag)
        {
            // 侵入してきたオブジェクトのタグが相手のとき展開
            isDeploy = true;
            GenerateUnit();
        }
    }

    // データ初期化
    void InitializeData()
    {
        StationData stationData = DataManager.Instance.stationData;
        this.hitPoint = stationData.hitPoint;
        this.atk= stationData.atk;
        this.def = stationData.def;
        this.luck = stationData.luck;
        this.stationData = stationData;
    }
}
