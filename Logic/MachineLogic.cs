public class MachineLogic
{
    // ダッシュ時のスピード返却
    public static int GetDashSpeed(int spd)
    {
        // ダッシュ時は通常スピード+10
        return spd + 10;
    }

    // ダッシュ時の加速度返却
    public static int GetDashAccel(int acceleration)
    {
        // ダッシュ時は通常加速度2倍
        return acceleration * 2;
    }

    // 射撃、斬撃待機時間計算
    public static float CalculateWaitTime(int arg)
    {
        return 0.71f - (float)arg / 140;
    }
}
