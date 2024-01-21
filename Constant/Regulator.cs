[System.Serializable]
public class Regulator
{
    public const float DASH_THRESHOLD = 0.25f; // ダッシュ操作の許容時間（秒）
    public const float ANGLE_THRESHOLD = 45f; // ダッシュ操作の許容角度（度）
    public const float BOUNCE_FACTOR = 1.1f; // 反発係数
    public const float SPEED_DOWN_FACTOR = 0.8f; // 防御中のスピード低下率

}
