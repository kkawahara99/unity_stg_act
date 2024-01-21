using System;

public class Calculator
{
    // ダメージ計算
    public static int CalcDamage(int weaponAtk, int offenseLuck, int deffenceDef, int deffenceLuck)
    {
        Random random = new Random();

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
            criticalRate = Math.Clamp(((float)offenseLuck / deffenceLuck) + 4.0f, 0f, 30f);
        } 
        // クリティカル時は受け側の装甲が半分になりダメージ1.5倍
        if (criticalRate > random.NextDouble() * 100 )
        {
            deffenceDef = deffenceDef / 2;
            criticalFactor = 1.5f;
        }
        // (攻撃側の火力 - 受け側の装甲) * クリティカル係数 * 乱数
        // 最小ダメージ1、最大ダメージ99
        int damageValue = (int)Math.Clamp((weaponAtk- deffenceDef) * criticalFactor * (0.8 + (random.NextDouble() * 0.4)), 1f, 99f);

        return damageValue;
    }

    // 索敵範囲計算
    public static float CalcSearchRange(int searchCapacity)
    {
        return 4f + (float)searchCapacity / 50;
    }

    // spdから速度取得
    public static float GetVelocityBySpd(float spd, bool isDefence)
    {
        float speedDownFactor = 1.0f;
        if (isDefence)
        {
            speedDownFactor = Regulator.SPEED_DOWN_FACTOR;
        }
        return (float)spd / 10 * speedDownFactor;
    }

    // 加速度を取得
    public static float GetAcceleration(float deltaTime, int acceleration)
    {
        return deltaTime * (float)acceleration / 6;
    }

    // 推進剤チャージスピード計算
    public static float calculateChargeSpeed(int pp)
    {
        return (float)pp / 100;
    }

    // カメラ拡大率計算
    public static float CalcZoomRate(int searchCapacity)
    {
        return CalcSearchRange(searchCapacity) / 2;
    }

    // 背景拡大率計算
    public static float CalcBackgroundSize(int searchCapacity)
    {
        return CalcSearchRange(searchCapacity) / 8;
    }

    // 射撃、斬撃待機時間計算
    public static float CalcWaitTime(int arg)
    {
        return 0.71f - (float)arg / 140;
    }

    // 斬撃振り下ろし速度計算
    public static float CalcSwingSpeed(int arg)
    {
        return 9f + (float)arg / 10;
    }

    // 距離から斬撃可能範囲かを判定
    public static bool IsSlashRangeByDistance(float x, float y)
    {
        return Math.Abs(x) < 0.7f && Math.Abs(y) < 0.3f;
    }
}
