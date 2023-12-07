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
    private float comeBackTime = 0.2f; // ダウン復帰時間
    [SerializeField]
    private GameObject explosionPrefab; // 爆風プレハブ
    [SerializeField]
    private MeteoType meteoTypte; // 隕石のタイプ

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
    private Calculator calc;
    private Common common;

    void Start()
    {
        // 必要な他コンポーネント取得
        common = GetComponent<Common>();
        calc = GetComponent<Calculator>();
        rb = GetComponent<Rigidbody2D>();

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
        if (!isDown)
        {
            // 衝突した部分のコライダーを取得
            ContactPoint2D contact = collision.contacts[0];

            // 衝突イベントを判定
            int ret = common.DecideEvent(contact, def, luck, calc);
            if (ret > 0)
            {
                // 0より大きい場合、爆風生成
                common.GenerateExplosion(contact, explosionPrefab);

                if (meteoTypte == MeteoType.Broken)
                {
                    // Brokenタイプの隕石の場合、ダメージ処理
                    currentHP = common.DecreaseHP(currentHP, ret);

                    // ダウンの状態に遷移
                    StartCoroutine(ComeBackFromDown());
                }
            }
        }
    }

    // ダウン中からの復帰
    IEnumerator ComeBackFromDown()
    {
        isDown = true;
        StartCoroutine(common.ComeBackFromDown(gameObject, comeBackTime));

        while (isDown)
        {
            // CommonのisDownがfalseになったとき
            // こっちのisDownもfalseにする
            if (!common.IsDown) isDown = false;
            yield return null;
        }
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
        // 削除する
        Destroy(gameObject, comeBackTime + 0.1f);

        // しばらくウェイト
        yield return new WaitForSeconds(comeBackTime);

        // 爆風を生成
        GameObject explosionObject = Instantiate(explosionPrefab, gameObject.transform.position, Quaternion.identity);
        explosionObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
    }
}
