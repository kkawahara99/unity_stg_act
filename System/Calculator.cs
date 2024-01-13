using UnityEngine;

public class Calculator : MonoBehaviour
{
    const float BOUNCE_FACTOR = 1.1f; // 反発係数

    public static Calculator Instance { get; private set; }

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

    // ダメージ計算
    public int CalculateDamage(int weaponAtk, int offenseLuck, int deffenceDef, int deffenceLuck)
    {
        float criticalRate;
        float criticalFactor = 1f;
        // クリティカル率(%) = (攻撃側の運 / 受け側の運)  + 4
        // 攻撃側の運が0の時はクリティカル率0%、受け側の運が0の時は受け側の運は1
        // クリティカル率最大30％
        if (offenseLuck == 0)
        {
            criticalRate = 0;
        }
        else
        {
            if (deffenceLuck == 0)
            {
                deffenceLuck = 1;
            }
            criticalRate = Mathf.Clamp(((float)offenseLuck / deffenceLuck) + 4.0f, 0f, 30f);
        } 
        // クリティカル時は受け側の装甲が半分になりダメージ1.5倍
        if (criticalRate > Random.value * 100 )
        {
            deffenceDef = deffenceDef / 2;
            criticalFactor = 1.5f;
        }
        // (攻撃側の火力 - 受け側の装甲) * クリティカル係数 * 乱数
        // 最小ダメージ1、最大ダメージ99
        int damageValue = (int)Mathf.Clamp((weaponAtk- deffenceDef) * criticalFactor * Random.Range(0.8f, 1.2f), 1f, 99f);

        return damageValue;
    }

    // 索敵範囲計算
    public float CalculateSearchRange(int searchCapacity)
    {
        return 4f + (float)searchCapacity / 50;
    }

    // 衝突速度計算
    public Vector2 calculateBounceVelocity(Vector2 velocity)
    {
        return velocity * BOUNCE_FACTOR;
    }

    // ダッシュ時のスピード返却
    public int GetDashSpeed(int spd)
    {
        // ダッシュ時は通常スピード+10
        return spd + 10;
    }

    // ダッシュ時の加速度返却
    public int GetDashAccel(int acceleration)
    {
        // ダッシュ時は通常加速度2倍
        return acceleration * 2;
    }

    // 目標速度を計算
    public Vector2 calculateTargetVelocity(Vector2 direction, int spd, bool isDefence)
    {
        float speedDownFactor;
        if (isDefence)
        {
            speedDownFactor = 0.8f;
        }
        else
        {
            speedDownFactor = 1.0f;
        }
        return direction * (float)spd / 10 * speedDownFactor;
    }

    // 加速度に応じた速度の計算
    public Vector2 calculateCurrentVelocity(Vector2 currentVelocity, Vector2 targetVelocity, int acceleration)
    {
        return Vector2.Lerp(currentVelocity, targetVelocity, Time.deltaTime * (float)acceleration / 6);
    }

    // 推進剤チャージスピード計算
    public float calculateChargeSpeed(int pp)
    {
        return (float)pp / 100;
    }

    // カメラ拡大率計算
    public float CalculateZoomRate(int searchCapacity)
    {
        return CalculateSearchRange(searchCapacity) / 2;
    }

    // 背景拡大率計算
    public float CalculateBackgroundSize(int searchCapacity)
    {
        return CalculateSearchRange(searchCapacity) / 8;
    }

    // 射撃、斬撃待機時間計算
    public float CalculateWaitTime(int arg)
    {
        return 0.71f - (float)arg / 140;
    }

    // 斬撃振り下ろし速度計算
    public float CalculateSwingSpeed(int arg)
    {
        return 9f + (float)arg / 10;
    }
}
