public class PilotLogic
{
    // 方向キー2度押し判定
    public static bool IsDoubleTap(float angle, float diffTime)
    {
        bool doubleTap = diffTime < Regulator.DASH_THRESHOLD;
        bool isSameDirection = angle < Regulator.ANGLE_THRESHOLD;

        if (doubleTap && isSameDirection)
        {
            // ダッシュの条件を満たしている
            return true;
        }
        return false;
    }

    // タグからターゲットステーションを取得
    public static string GetStationNameByTag(string tagName)
    {
        return tagName == TagConst.BLUE ? "StationEnemy" : "Station";
    }
}
