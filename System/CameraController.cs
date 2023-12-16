using UnityEngine;

public class CameraController : MonoBehaviour
{
    const float CAMERA_TRACKING_SPEED = 1.0f;
    private Camera mainCamera;

    void Awake()
    {
        // メインカメラ取得
        mainCamera = Camera.main;
    }
    void Update()
    {
        trackingPlayer(true);
    }

    public void trackingPlayer(bool isLerp)
    {
        // isCpuがfalseのユニットを追跡する（仮）
        Vector2 charaPosition = transform.position;
        int searchCapacity = 10;
        Unit[] units = FindObjectsOfType<Unit>();
        foreach (Unit unit in units)
        {
            if (unit != null)
            {
                if (!unit.IsCpu)
                {
                    // ユニットポジションを取得
                    charaPosition = unit.transform.position;

                    // ユニットの索敵能力を取得
                    searchCapacity = unit.transform.Find("Pilot").GetComponent<Pilot>().SearchCapacity;
                    break;
                }
            }
        }
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
