using System;
using UnityEngine;

public class Ballet : MonoBehaviour
{
    private int power; // 弾の火力
    public int Power { get => power; }
    private bool isEnemy; // 敵の弾かどうか
    public bool IsEnemy { get => isEnemy; }

    private Rigidbody2D rb;
    private Vector2 currentVelocity;  // 現在の速度
    private Weapon weapon; 
    private Pilot pilot; 
    public Pilot Pilot { get => pilot; } 

    // 発射された
    void Start()
    {
        // 必要な他コンポーネント取得
        rb = GetComponent<Rigidbody2D>();

        // 回転を制御
        rb.freezeRotation = true;  // 回転を固定

        // 自軍との衝突を無効にする
        string color = isEnemy ? "Red" : "Blue";
        GameObject[] objectsToIgnore = GameObject.FindGameObjectsWithTag(color);
        foreach (GameObject obj in objectsToIgnore)
        {
            Transform machineTransform = obj.transform.Find("Machine");
            if (machineTransform != null)
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), machineTransform.GetComponent<Collider2D>());

                // 盾を持っている場合は盾も無効
                Transform shieldTransform = Common.Instance.FindObjectRecursively(machineTransform, "Shield");
                if (shieldTransform != null)
                    Physics2D.IgnoreCollision(GetComponent<Collider2D>(), shieldTransform.GetComponent<Collider2D>());
            }
            else if (obj.GetComponent<Station>() != null)
            {
                // ステーションも無効
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), obj.GetComponent<Collider2D>());
            }
        }

        // 名前変更
        gameObject.name = "Ballet";

        // しばらくすると消える
        Destroy(this.gameObject, this.weapon.ActiveTime);
    }

    void FixedUpdate()
    {
        // 弾の移動速度を更新
        rb.velocity = currentVelocity;
    }

    // 敵の弾かどうかを設定
    public void SetIsEnemy(bool isEnemy)
    {
        this.isEnemy = isEnemy;
    }

    // 弾の火力を設定
    public void SetPower(int power)
    {
        this.power = power;
    }

    // 発射させたパイロット情報を設定
    public void SetPilot(Pilot pilot)
    {
        this.pilot = pilot;
    }

    // 発射させた武器情報を設定
    public void SetWeapon(Weapon weapon)
    {
        this.weapon = weapon;
    }

    // 弾の速度変更
    public void SetSpeed(Vector2 direction)
    {
        // 速度変更
        currentVelocity = Calculator.Instance.calculateTargetVelocity(direction, weapon.MaxSpeed, false);
    }

    // 接触時の処理
    void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(this.gameObject);

        // if (collision.transform.parent != null)
        // {
        //     if (!collision.transform.parent.CompareTag(gameObject.tag))
        //     {
        //         // ToDo: 何らかの処理
        //         // 自軍以外と接触時のみ被弾にする
        //         gameObject.GetComponent<Collider2D>().isTrigger = false;
        //         Destroy(this.gameObject);
        //     }
        // }
    }
}
