using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    private int power = 5; // 火力
    [SerializeField]
    private int balletCost = 25; // 弾コスト
    [SerializeField]
    private GameObject balletPrefab; // 弾のプレハブ
    [SerializeField]
    private float balletOffset = 0.45f; // 弾の生成位置オフセット
    [SerializeField]
    private int chargeSpeed = 2; // 弾のチャージ速度(stock/100ms)
    [SerializeField]
    private int maxSpeed = 40; // 最大速度
    [SerializeField]
    private float activeTime = 1.0f; // 有効時間
    [SerializeField]
    private Vector2 equipmentPosition; // 装備位置

    private int stock = 100; // 弾のストック

    public int Power { get => power; }
    public int BalletCost { get => balletCost; }
    public GameObject BalletPrefab { get => balletPrefab; }
    public float BalletOffset { get => balletOffset; }
    public int ChargeSpeed { get => chargeSpeed; }
    public int Stock { get => stock; }

    private ChargeUI chargeUI;
    private Pilot pilot;
    public Pilot Pilot { get => pilot; }
    private Machine machine;
    public Machine Machine { get => machine; }
    public int MaxSpeed { get => maxSpeed; }
    public float ActiveTime { get => activeTime; }
    public Vector2 EquipmentPosition { get => equipmentPosition; }

    void Start()
    {
        // 他のコンポーネントから情報取得
        chargeUI = gameObject.transform.parent.parent.parent.parent.parent.Find("ParamUI").Find("ChargeGauge").GetComponent<ChargeUI>();
        pilot = gameObject.transform.parent.parent.parent.parent.parent.Find("Pilot").GetComponent<Pilot>();
        machine = gameObject.transform.parent.parent.parent.parent.GetComponent<Machine>();

        // 弾のチャージコルーチン開始
        StartCoroutine(ChargeBalletCost());
    }

    private void OnEnable()
    {
        // 無効→有効になったときにStart処理を再度行う
        Start();
    }

    // 弾のチャージ
    IEnumerator ChargeBalletCost()
    {
        while (true)
        {
            float waitTime = 0.1f;
            yield return new WaitForSeconds(waitTime);

            // stockが最大値に達していない場合のみチャージ
            if (stock < 100)
            {
                // チャージ速度に応じて stock を増やす
                stock = Mathf.Min(stock + chargeSpeed, 100);
                chargeUI.UpdateChargeUI(stock, 100);
            }
            else
            {
                // MAXの場合、waitTimeをChargeUIに渡す
                chargeUI.HideTime(waitTime);
            }
        }
    }

    // 弾の発射
    public void Launch(float angle)
    {
        if (stock < balletCost)
        {
            // ストックが足りない場合は発射されない
            return;
        }

        // ストックを減らす
        stock -= balletCost;
        
        // directionをangleの角度に合わせて回転
        Vector2 direction = Quaternion.Euler(0f, 0f, angle) * Vector2.right;

        // 弾の生成
        Vector2 shootPosition = CalculatePosition(transform.parent.parent.parent.position, angle, balletOffset);
        GameObject balletObject = Instantiate(balletPrefab, shootPosition, Quaternion.identity);

        // 弾の角度調整
        balletObject.transform.rotation = Quaternion.Euler(0, 0, angle);

        // 弾に情報渡す
        Ballet ballet = balletObject.GetComponent<Ballet>();
        ballet.SetPower(machine.Atc + power);
        ballet.SetPilot(pilot);
        ballet.SetWeapon(gameObject.GetComponent<Weapon>());

        // 弾の発射
        ballet.SetSpeed(direction.normalized);

        // 効果音
        SoundManager.Instance.PlaySE(SESoundData.SE.Shoot1);
    }

    // 発射位置を計算
    Vector2 CalculatePosition(Vector2 startPoint, float angleInDegrees, float distance)
    {
        // 角度をラジアンに変換
        float angleInRadians = Mathf.Deg2Rad * angleInDegrees;

        // 指定した角度と距離から直交座標を計算
        float x = startPoint.x + distance * Mathf.Cos(angleInRadians);
        float y = startPoint.y + distance * Mathf.Sin(angleInRadians);

        return new Vector2(x, y);
    }
}
