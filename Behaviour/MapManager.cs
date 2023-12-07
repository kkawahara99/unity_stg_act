using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField]
    private GameObject borderPrefab;
    [SerializeField]
    private float maxX = 8f;
    public float MaxX { get => maxX; }
    [SerializeField]
    private float minX = -8f;
    public float MinX { get => minX; }
    [SerializeField]
    private float maxY = 3f;
    public float MaxY { get => maxY; }
    [SerializeField]
    private float minY = -3f;
    public float MinY { get => minY; }

    public System.Type targetType = typeof(Unit);
    private float borderOffset = 0.125f;

    void Start()
    {
        displayBorders();
        DeployUnits();
    }

    // ボーダーラインを表示
    void displayBorders()
    {
        // ボーダーの位置設定
        Vector2 upBorderPosition = new Vector2(maxX + minX, maxY + borderOffset);
        Vector2 downBorderPosition = new Vector2(maxX + minX, minY - borderOffset);
        Vector2 rightBorderPosition = new Vector2(maxX + borderOffset, maxY + minY);
        Vector2 leftBorderPosition = new Vector2(minX - borderOffset, maxY + minY);

        // ボーダー表示
        GameObject upBorderObject = Instantiate(borderPrefab, upBorderPosition, Quaternion.identity, gameObject.transform);
        GameObject downBorderObject = Instantiate(borderPrefab, downBorderPosition, Quaternion.identity, gameObject.transform);
        GameObject rightBorderObject = Instantiate(borderPrefab, rightBorderPosition, Quaternion.identity, gameObject.transform);
        GameObject leftBorderObject = Instantiate(borderPrefab, leftBorderPosition, Quaternion.identity, gameObject.transform);

        // ボーダーの長さ設定
        upBorderObject.transform.localScale = new Vector2(maxX - minX + borderOffset * 4, borderOffset * 2);
        downBorderObject.transform.localScale = new Vector2(maxX - minX + borderOffset * 4, borderOffset * 2);
        rightBorderObject.transform.localScale = new Vector2(borderOffset * 2, maxY - minY + borderOffset * 4);
        leftBorderObject.transform.localScale = new Vector2(borderOffset * 2, maxY - minY + borderOffset * 4);
    }

    // ユニットを展開する
    void DeployUnits()
    {
        Component[] components  = GetComponentsInChildren(targetType, true);
        foreach (Component component in components)
        {
            Unit unit = component.gameObject.GetComponent<Unit>();
            unit.DeployMachine();
            unit.DeployPilot();
        }
    }
}
