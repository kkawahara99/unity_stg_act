using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteo : MonoBehaviour
{
    [SerializeField]
    private int hitPoint = 1; // HP
    [SerializeField]
    private int def = 0; // 装甲
    [SerializeField]
    private int luck = 0; // 運
    [SerializeField]
    const float COMEBACK_TIME = 0.2f; // ダウン復帰時間
    const float ITEM_WAIT_TIME = 0.5f; // アイテム待機時間
    [SerializeField]
    private GameObject explosionPrefab; // 爆風プレハブ
    [SerializeField]
    private MeteoType meteoTypte; // 隕石のタイプ
    [SerializeField]
    private List<ItemBean> dropItem = new List<ItemBean>(); // ドロップアイテム

    public enum MeteoType
    {
        Static,  // 静的
        Dynamic, // 動的
        Broken   // 壊れる
    }

    private bool isDown; // ダウン中かどうか
    private int currentHP; // 現在のHP

    public int HitPoint { get => hitPoint; }

    private Rigidbody2D rb;
    private bool isDead;

    void Start()
    {
        // 必要な他コンポーネント取得
        rb = GetComponent<Rigidbody2D>();

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

                if (meteoTypte == MeteoType.Broken)
                {
                    // Brokenタイプの隕石の場合、ダメージ処理
                    currentHP = Common.Instance.DecreaseHP(currentHP, ret);

                    // ダウンの状態に遷移
                isDown = true;
                StartCoroutine(Common.Instance.ComeBackFromDown(gameObject, COMEBACK_TIME, isDown));
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Meteo Strike");
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

    // クラッシュする
    IEnumerator Crush()
    {
        // しばらくウェイト
        yield return new WaitForSeconds(COMEBACK_TIME);

        // 爆風を生成
        GameObject explosionObject = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        explosionObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

        // アイテム生成
        Common.Instance.GenerateItem(dropItem, transform);

        // 削除する
        Destroy(gameObject);
    }
}
