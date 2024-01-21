using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Common
{
    [SerializeField] GameObject explosionPrefab; // 爆風プレハブ
    const float BASE_EXP = 10f;
    const float EXP_RATIO = 1.1f;

    // 色を変更
    public static void SetColors(Color color, Transform transform)
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
    public static void RestrictMovePosition(Rigidbody2D rb, float maxX, float minX, float maxY, float minY)
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

    // 斬撃攻撃範囲かどうかを判定
    public static bool IsSlashRange(Vector2 yourPosition, Vector2 myPosition)
    {
        float distanceX = yourPosition.x - myPosition.x;
        float distanceY = yourPosition.y - myPosition.y;
        return Calculator.IsSlashRangeByDistance(distanceX, distanceY);
    }

    // 衝突速度計算
    public static Vector2 GetBounceVelocity(Vector2 velocity)
    {
        return velocity * Regulator.BOUNCE_FACTOR;
    }

    // spdから最大目標速度を計算
    public static Vector2 GetTargetVelocity(Vector2 direction, int spd, bool isDefence)
    {
        return direction * Calculator.GetVelocityBySpd(spd, isDefence);
    }

    // 加速度に応じた速度の計算
    public static Vector2 GetCurrentVelocity(Vector2 currentVelocity, Vector2 targetVelocity, int acceleration)
    {
        return Vector2.Lerp(currentVelocity, targetVelocity, Calculator.GetAcceleration(Time.deltaTime, acceleration));
    }

    // ランダムなVector2を取得
    public static Vector2 GetRandomVector2()
    {
        return new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
    }

    // 衝突時のベクトルを計算
    public static Vector2 CalculateVelocity(ContactPoint2D contact, Vector2 currentVelocity, int spd)
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
                result = currentVelocity + GetBounceVelocity(collisionNormal);
            }
            else
            {
                // 逆方向なら反射
                result = GetBounceVelocity(Vector2.Reflect(currentVelocity, collisionNormal));
            }
        }

        // 跳ね返りベクトルは一定値以上にならないようにする
        Vector2 maxVelocity = GetBounceVelocity(
            GetTargetVelocity(
                Vector2.one.normalized, spd, false
            )
        );
        result.x = Mathf.Clamp(result.x, -maxVelocity.x, maxVelocity.x);
        result.y = Mathf.Clamp(result.y, -maxVelocity.y, maxVelocity.y);

        return result;
    }

    // 接触時のイベント分岐
    public static int DecideEvent(ContactPoint2D contact, int def, int deffenceLuck)
    {
        int damageValue = 0;

        // 衝突したオブジェクトから情報を取得
        Ballet collidedBallet = contact.collider.gameObject.GetComponent<Ballet>();
        Weapon collidedWeapon = contact.collider.gameObject.GetComponent<Weapon>();

        if (collidedBallet != null)
        {
            // 弾に被弾したとき
            int atk= collidedBallet.Power;
            int luck = collidedBallet.Pilot.Model.Luck;

            // ダメージ処理
            damageValue = Calculator.CalcDamage(atk, luck, def, deffenceLuck);
        }
        else if (collidedWeapon != null)
        {
            // 武器に被弾したとき
            int atk= collidedWeapon.Machine.Model.Atk+ collidedWeapon.Power;
            int luck = collidedWeapon.Pilot.Model.Luck;

            // ダメージ処理
            damageValue = Calculator.CalcDamage(atk, luck, def, deffenceLuck);
        }
        else
        {
            // その他と接触したとき
        }

        return damageValue;
    }

    // 被弾時爆風生成
    public static void GenerateExplosionWhenHitted(ContactPoint2D contact)
    {
        MonoCommon.Instance.GenerateExplosion(contact.point, 0.4f);
    }

    // ダウン中からの復帰
    public static IEnumerator ComeBackFromDown(GameObject downObject, float comeBackTime, bool isDown)
    {
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

    // 増やした後のHPを返す
    public static int IncreaseHP(int maxHp, int currentHP, int recoveryValue)
    {
        // 現在HPを増やす
        // 最大HP以上にならないようにする
        return Mathf.Min(currentHP + recoveryValue, maxHp);
    }

    // 減らした後のHPを返す
    public static int DecreaseHP(int currentHP, int damageValue)
    {
        // 現在HPを減らす
        // 0以下にならないようにする
        return Mathf.Max(currentHP - damageValue, 0);
    }

    // 減らした後のPPを返す
    public static float DecreasePP(float currentPP)
    {
        // 現在HPを減らす
        // 0以下にならないようにする
        return Mathf.Max(currentPP - 10 * Time.deltaTime, 0f);
    }

    //　オブジェクトの再起的探索
    public static Transform FindObjectRecursively(Transform parentTransform, string nameToFind)
    {
        Transform result = parentTransform.Find(nameToFind);

        if (result != null)
        {
            // ヒットすればそのtransformを返す
            return result;
        }

        // 再帰的に子オブジェクトを探索
        for (int i = 0; i < parentTransform.childCount; i++)
        {
            Transform child = parentTransform.GetChild(i);
            result = FindObjectRecursively(child, nameToFind);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    // オブジェクト名を指定してGameObjectを検索するメソッド
    public static GameObject FindObjectByName(List<GameObject> objectList, string name)
    {
        foreach (GameObject obj in objectList)
        {
            if (obj.name == name)
            {
                return obj;
            }
        }
        // 見つからなかった場合はnullを返します
        return null;
    }

    // PrefabMappingのvalueを取得する
    public static GameObject GetPrefabMapping(string key, List<PrefabMapping> datas)
    {
        foreach (var data in datas)
        {
           if (data.key == key)
               return data.prefab;
        }
        return null; // 該当するkeyが見つからなかった場合
    }

    // 次のレベルまでのEXPを取得する
    public static int GetNextExp(int level)
    {
        return GetExpCurrentLevel(level + 1) - GetExpCurrentLevel(level);
    }

    // 現在のレベルに必要な経験値を取得する
    public static int GetExpCurrentLevel(int level)
    {
        return (int)Math.Ceiling(BASE_EXP * (Math.Pow(EXP_RATIO, (level - 1)) - 1) / (EXP_RATIO - 1));
    }

    // ユニットの付与経験値を取得
    public static int GetGrantExp(Transform unitTransform)
    {
        float basic = 10;
        MachineController machine = unitTransform.Find(MachineConst.MACHINE).GetComponent<MachineController>();
        float hp = machine.Model.HitPoint;
        float pp = machine.Model.PropellantPoint;
        float atk = machine.Model.Atk;
        float def = machine.Model.Def;
        float spd = machine.Model.Spd;
        PilotController pilot = unitTransform.Find("Pilot").GetComponent<PilotController>();
        float sh = pilot.Model.Shootability;
        float sl = pilot.Model.Slashability;
        float ac = pilot.Model.Acceleration;
        float lk = pilot.Model.Luck;
        float sc = pilot.Model.SearchCapacity;

        float sum = basic + hp + pp + atk + def + spd + sh + sl + ac + lk + sc;
        return (int)Math.Ceiling(BASE_EXP * (Math.Pow(EXP_RATIO, (sum / 40f - 1)) - 1) / (EXP_RATIO - 1) / 4f);
    }
}
