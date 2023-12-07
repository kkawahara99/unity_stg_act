using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float cameraTrackingSpeed = 1.0f;
    private Camera mainCamera;
    private Calculator calc;

    void Start()
    {
        // メインカメラ取得
        mainCamera = Camera.main;

        // 必要な他コンポーネント取得
        calc = GetComponent<Calculator>();
    }
    void Update()
    {
        // isCpuがfalseのユニットを追跡する（仮）
        Vector2 charaPosition = transform.position;
        int searchCapacity = 10;
        Unit[] charaInfos = FindObjectsOfType<Unit>();
        foreach (Unit unit in charaInfos)
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
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * cameraTrackingSpeed);
        
        // カメラサイズをキャラの索敵能力に合わせて変更
        mainCamera.orthographicSize = calc.CalculateZoomRate(searchCapacity);
        float scale = calc.CalculateBackgroundSize(searchCapacity);
        transform.Find("Background").localScale = new Vector3(scale, scale, scale);
    }

    // 画面の前景色を設定
    public void SetForeground(Color color)
    {
        SpriteRenderer spriteRenderer = transform.Find("Foreground").GetComponent<SpriteRenderer>();
        spriteRenderer.color = color;
    }
}
