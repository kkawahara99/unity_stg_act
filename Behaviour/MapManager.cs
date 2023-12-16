using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField]
    private GameObject borderPrefab;
    [SerializeField]
    private float maxX;
    public float MaxX { get => maxX; }
    [SerializeField]
    private float minX;
    public float MinX { get => minX; }
    [SerializeField]
    private float maxY;
    public float MaxY { get => maxY; }
    [SerializeField]
    private float minY;
    public float MinY { get => minY; }

    public System.Type targetType = typeof(Unit);
    const float BORDER_OFFSET = 0.125f; // ボーダーの幅の半分の値

    void Start()
    {
        displayBorders();
        DeployUnits();
    }

    // ボーダーラインを表示
    void displayBorders()
    {
        // ボーダーの位置設定
        Vector2 upBorderPosition = new Vector2(maxX + minX, maxY + BORDER_OFFSET);
        Vector2 downBorderPosition = new Vector2(maxX + minX, minY - BORDER_OFFSET);
        Vector2 rightBorderPosition = new Vector2(maxX + BORDER_OFFSET, maxY + minY);
        Vector2 leftBorderPosition = new Vector2(minX - BORDER_OFFSET, maxY + minY);

        // ボーダー表示
        GameObject upBorderObject = Instantiate(borderPrefab, upBorderPosition, Quaternion.identity, gameObject.transform);
        GameObject downBorderObject = Instantiate(borderPrefab, downBorderPosition, Quaternion.identity, gameObject.transform);
        GameObject rightBorderObject = Instantiate(borderPrefab, rightBorderPosition, Quaternion.identity, gameObject.transform);
        GameObject leftBorderObject = Instantiate(borderPrefab, leftBorderPosition, Quaternion.identity, gameObject.transform);

        // ボーダーの長さ設定
        float twoTimes = BORDER_OFFSET * 2; // ボーダーの幅
        float fourTimes = BORDER_OFFSET * 4; // ボーダーの幅2つ分
        upBorderObject.transform.localScale = new Vector2(maxX - minX + fourTimes, twoTimes);
        downBorderObject.transform.localScale = new Vector2(maxX - minX + fourTimes, twoTimes);
        rightBorderObject.transform.localScale = new Vector2(twoTimes, maxY - minY + fourTimes);
        leftBorderObject.transform.localScale = new Vector2(twoTimes, maxY - minY + fourTimes);

        // タグ設定
        upBorderObject.tag = "Wall";
        downBorderObject.tag = "Wall";
        rightBorderObject.tag = "Wall";
        leftBorderObject.tag = "Wall";
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
