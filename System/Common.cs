using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Common : MonoBehaviour
{
    [SerializeField] GameObject explosionPrefab; // 爆風プレハブ
    private bool isDown;
    public bool IsDown { get => isDown; }

    public static Common Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 色を変更
    public void SetColors(Color color, Transform transform)
    {
        SpriteRenderer[] spriteRenderers = transform.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            List<string> noColorObjects = new List<string> {"MainWeapon", "HandWeapon", "Shiled"};
            List<string> colorObjects = new List<string> {"Head2", "Booster"};
            string objectName = spriteRenderer.gameObject.name;
            if (noColorObjects.Contains(objectName))
            {
                // 装備は色塗りしない
                continue;
            }

            if (colorObjects.Contains(objectName))
            {
                // メガネ、ブースターのみ色塗りする（仮）
                spriteRenderer.color = color;
            }
        }
    }

    // 移動位置を制限
    public void RestrictMovePosition(Rigidbody2D rb, float maxX, float minX, float maxY, float minY)
    {
        // 新しい位置を計算
        Vector2 newPosition = rb.position + rb.velocity * Time.deltaTime;

        // X座標を制約
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);

        // Y座標を制約
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        // 新しい位置を適用
        rb.MovePosition(newPosition);
    }

    // 衝突時のベクトルを計算
    public Vector2 CalculateVelocity(ContactPoint2D contact, Vector2 currentVelocity, int spd)
    {
        Vector2 result;
        GameObject collidedObject = contact.collider.gameObject;

        if (collidedObject.name == "ToBe")
        {
            // ToBe: アイテムなどとの衝突では跳ね返らない
            result =  currentVelocity;
        }
        else
        {
            // 衝突対象のベクトル取得
            Vector2 collisionNormal = contact.normal;

            // 衝突対象との速度ベクトルの内積を計算
            float dotProduct = Vector2.Dot(currentVelocity, collisionNormal);

            if (dotProduct > 0f)
            {
                // 同じ方向ならベクトル加算
                result = currentVelocity + Calculator.Instance.calculateBounceVelocity(collisionNormal);
            }
            else
            {
                // 逆方向なら反射
                result = Calculator.Instance.calculateBounceVelocity(Vector2.Reflect(currentVelocity, collisionNormal));
            }
        }

        // 跳ね返りベクトルは一定値以上にならないようにする
        Vector2 maxVelocity = Calculator.Instance.calculateBounceVelocity(Calculator.Instance.calculateTargetVelocity(Vector2.one.normalized, spd, false));
        result.x = Mathf.Min(result.x, maxVelocity.x);
        result.y = Mathf.Min(result.y, maxVelocity.y);

        return result;
    }

    // 接触時のイベント分岐
    public int DecideEvent(ContactPoint2D contact, int def, int deffenceLuck)
    {
        int damageValue = 0;

        // 衝突したオブジェクトから情報を取得
        Ballet collidedBallet = contact.collider.gameObject.GetComponent<Ballet>();
        Weapon collidedWeapon = contact.collider.gameObject.GetComponent<Weapon>();

        if (collidedBallet != null)
        {
            // 弾に被弾したとき
            int atc = collidedBallet.Power;
            int luck = collidedBallet.Pilot.Luck;

            // ダメージ処理
            damageValue = Calculator.Instance.CalculateDamage(atc, luck, def, deffenceLuck);
        }
        else if (collidedWeapon != null)
        {
            // 武器に被弾したとき
            int atc = collidedWeapon.Machine.Atc + collidedWeapon.Power;
            int luck = collidedWeapon.Pilot.Luck;

            // ダメージ処理
            damageValue = Calculator.Instance.CalculateDamage(atc, luck, def, deffenceLuck);
        }
        else
        {
            // その他と接触したとき
        }

        return damageValue;
    }

    // 被弾時爆風生成
    public void GenerateExplosionWhenHitted(ContactPoint2D contact)
    {
        GenerateExplosion(contact.point, 0.4f);
    }

    // 爆風生成
    public void GenerateExplosion(Vector2 position, float size)
    {
        GameObject explosionObject = Instantiate(explosionPrefab, position, Quaternion.identity);
        explosionObject.transform.localScale = new Vector3(size, size, size);
    }

    // ダウン中からの復帰
    public IEnumerator ComeBackFromDown(GameObject downObject, float comeBackTime)
    {
        isDown = true;
        bool isClearness = false;
        float downTime = 0f;
        float transparency;
        SpriteRenderer[] spriteRenderers = downObject.GetComponentsInChildren<SpriteRenderer>(false);

        // コライダーを一時的に無効化
        downObject.GetComponent<Collider2D>().enabled = false;

        while (isDown)
        {
            if (downTime > comeBackTime)
            {
                // ダウンタイムが復帰時間に達した場合、復帰
                isDown = false;
                if (!isClearness)
                {
                    // 透明になっていないときはそのまま処理終了
                    break;
                }
            }

            // 点滅
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                if (isClearness)
                {
                    // 透明のときは非透明化
                    transparency = 1f;
                }
                else
                {
                    // 非透明の時は透明化
                    transparency = 0f;
                }
                Color currentColor = spriteRenderer.color;
                currentColor.a = transparency;
                spriteRenderer.color = currentColor;
            }

            // 点滅フラグ反転
            isClearness = !isClearness;

            // ダウンタイムに加算
            downTime += Time.deltaTime;

            yield return null;
            yield return null;
        }

        // コライダーを有効化
        downObject.GetComponent<Collider2D>().enabled = true;
    }

    // 減らした後のHPを返す
    public int DecreaseHP(int currentHP, int damageValue)
    {
        // 現在HPを減らす
        // 0以下にならないようにする
        return Mathf.Max(currentHP - damageValue, 0);
    }

    // 減らした後のPPを返す
    public float DecreasePP(float currentPP)
    {
        // 現在HPを減らす
        // 0以下にならないようにする
        return Mathf.Max(currentPP - 10 * Time.deltaTime, 0f);
    }
}
