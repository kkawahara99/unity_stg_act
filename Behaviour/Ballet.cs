using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ballet : MonoBehaviour
{
    private int power; // 弾の火力
    public int Power { get => power; }

    private Rigidbody2D rb;
    private Vector2 currentVelocity;  // 現在の速度
    private Pilot pilot; 
    private Weapon weapon; 
    public Pilot Pilot { get => pilot; } 

    // 発射された
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 回転を制御
        rb.freezeRotation = true;  // 回転を固定

        // しばらくすると消える
        Destroy(this.gameObject, this.weapon.ActiveTime);
    }

    void FixedUpdate()
    {
        // 弾の移動速度を更新
        rb.velocity = currentVelocity;
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
        // ToDo: 何らかの処理

        // 消える
        Destroy(this.gameObject);
    }
}
