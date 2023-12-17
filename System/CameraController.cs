using UnityEngine;

public class CameraController : MonoBehaviour
{
    const float CAMERA_TRACKING_SPEED = 1.0f;
    private Camera mainCamera;
    private Unit trackingUnit; // 追跡するユニット
    private GameManager gameManager; // ゲーム管理

    void Awake()
    {
        // メインカメラ取得
        mainCamera = Camera.main;
    }

    void Start()
    {
        // 必要な他コンポーネント取得
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void Update()
    {
        // ポーズ中はカメラ移動停止する
        if (gameManager.IsPaused) return;

        trackingPlayer(true);
    }

    public void SetUnit()
    {
        // isCpuがfalseのユニットを追跡する（仮）
        Unit[] units = FindObjectsOfType<Unit>();
        foreach (Unit unit in units)
        {
            if (unit != null)
            {
                if (!unit.IsCpu)
                {
                    trackingUnit = unit;
                    break;
                }
            }
        }
    }

    public void trackingPlayer(bool isLerp)
    {
        // isCpuがfalseのユニットを追跡する（仮）
        Vector2 charaPosition = trackingUnit.transform.position;
        int searchCapacity = trackingUnit.transform.Find("Pilot").GetComponent<Pilot>().SearchCapacity;

        // カメラ位置変更
        Vector3 targetPosition = new Vector3(charaPosition.x, charaPosition.y, -10);
        if (isLerp)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * CAMERA_TRACKING_SPEED);
        }
        else
        {
            transform.position = targetPosition;
        }
        
        // カメラサイズをキャラの索敵能力に合わせて変更
        mainCamera.orthographicSize = Calculator.Instance.CalculateZoomRate(searchCapacity);
        float scale = Calculator.Instance.CalculateBackgroundSize(searchCapacity);
        transform.Find("Background").localScale = new Vector3(scale, scale, scale);
    }

    // 画面の前景色を設定
    public void SetForeground(Color color)
    {
        SpriteRenderer spriteRenderer = transform.Find("Foreground").GetComponent<SpriteRenderer>();
        spriteRenderer.color = color;
    }
}
