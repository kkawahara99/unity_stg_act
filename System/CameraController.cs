using UnityEngine;

public class CameraController : MonoBehaviour
{
    const float CAMERA_TRACKING_SPEED = 1.0f;
    private Camera mainCamera;
    private Unit trackingUnit; // 追跡するユニット
    private GameManager gameManager; // ゲーム管理
    public float targetAspectRatio = 16f / 9f; // 目標のアスペクト比（横:縦）

    void Awake()
    {
        // メインカメラ取得
        mainCamera = Camera.main;
        FitAspectRatio();
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

    void FitAspectRatio()
    {
        if (mainCamera == null)
        {
            Debug.LogError("No camera found");
            return;
        }

        // 目標のアスペクト比
        float targetAspect = targetAspectRatio;

        // 現在の画面のアスペクト比
        float windowAspect = (float)Screen.width / (float)Screen.height;

        // 目標のアスペクト比に合わせて、ビューポートの幅または高さを変更
        float scaleHeight = windowAspect / targetAspect;

        Camera cameraComponent = GetComponent<Camera>();

        // 現在の画面アスペクト比が目標よりも横に広い場合
        if (scaleHeight < 1.0f)
        {
            Rect rect = cameraComponent.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            cameraComponent.rect = rect;
        }
        else // 目標よりも縦に長い場合
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = cameraComponent.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            cameraComponent.rect = rect;
        }
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
        if (trackingUnit == null) return;

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
