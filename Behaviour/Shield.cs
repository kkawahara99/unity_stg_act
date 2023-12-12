using System.Collections;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [SerializeField]
    private int hitPoint = 20; // HP
    [SerializeField]
    private int def = 10; // 装甲
    [SerializeField]
    private float comeBackTime = 0.2f; // ダウン復帰時間
    [SerializeField]
    private GameObject explosionPrefab; // 爆風プレハブ
    [SerializeField]
    private Vector2 equipmentPosition; // 装備位置

    private Pilot pilot; // キャラクター情報
    private Machine machine; // マシン情報
    private bool isDown; // ダウン中かどうか
    private int currentHP; // 現在のHP
    private bool isAlive = true; // 死活状態
    public bool IsAlive { get => isAlive; } // 死活状態

    public int HitPoint { get => hitPoint; }
    public int Def { get => def; }
    public Vector2 EquipmentPosition { get => equipmentPosition; }

    void Start()
    {
        // 必要な他コンポーネント取得
        pilot = gameObject.transform.parent.parent.parent.parent.Find("Pilot").GetComponent<Pilot>();
        machine = gameObject.transform.parent.parent.parent.GetComponent<Machine>();
        
        // ステータス初期化
        currentHP = hitPoint;
    }

    void Update()
    {
        // 0のときクラッシュする
        if (currentHP == 0)
        {
            StartCoroutine(Crush());
        }
    }

    // 接触時の処理
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!machine.IsDown && !isDown)
        {
            // 衝突した部分のコライダーを取得
            ContactPoint2D contact = collision.contacts[0];

            // 衝突イベントを判定
            int ret = Common.Instance.DecideEvent(contact, def, pilot.Luck);
            if (ret > 0)
            {
                // 0より大きい場合、ダメージ処理
                currentHP = Common.Instance.DecreaseHP(currentHP, ret);

                // 爆風生成
                Common.Instance.GenerateExplosionWhenHitted(contact);

                // ダウンの状態に遷移
                StartCoroutine(ComeBackFromDown());
            }
        }
    }

    // ダウン中からの復帰
    IEnumerator ComeBackFromDown()
    {
        isDown = true;
        StartCoroutine(Common.Instance.ComeBackFromDown(gameObject, comeBackTime, isDown));

        do
        {
            // CommonのisDownがfalseになったとき
            // こっちのisDownもfalseにする
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
        yield return new WaitForSeconds(comeBackTime);

        // 爆風を生成
        GameObject explosionObject = Instantiate(explosionPrefab, gameObject.transform.position, Quaternion.identity);
        explosionObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

        // シールドしに
        isAlive = false;
    }
}
