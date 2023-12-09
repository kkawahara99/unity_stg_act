using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pilot : MonoBehaviour
{
    private Unit unit;

    [SerializeField] private int shootability; // 射撃スキル
    public int Shootability { get => shootability; }
    [SerializeField] private int slashability; // 斬撃スキル
    public int Slashability { get => slashability; }
    [SerializeField] private int acceleration; // 操縦スキル
    public int Acceleration { get => acceleration; }
    [SerializeField] private int luck; // 運
    public int Luck { get => luck; }
    [SerializeField] private int searchCapacity; // 索敵能力
    public int SearchCapacity { get => searchCapacity; }
    [SerializeField] private AIMode aiMode; // AIモード

    public enum AIMode
    {
        Simple,    // 単純
        Assault,   // 突撃
        Avoidance, // 回避
        Shooting,  // 射撃
        Balance,   // バランス
        Defence    // 防衛
    }

    private Vector2 cpuDirection; // CPUの移動方向
    private float cpuPhaseTime; // CPUの行動フェーズ切替時間
    private Machine machine; // Machineスクリプト
    private bool isDoubleTap; // 方向キー2連続押しかどうか
    private bool isDashing; // ダッシュ中かどうか
    private Vector2 lastDirection = Vector2.zero; // 前回の方向
    private float lastKeyPressTime = 0f; // 前回のキー押下時間
    private float dashThreshold = 0.25f; // ダッシュ閾値
    private Vector2 currentDirection; // 現在の入力方向
    private Controller controller; // コントローラ
    private GameManager gameManager; // ゲーム管理

    // コンストラクタ
    public Pilot(
        int shootability,
        int slashability,
        int acceleration,
        int luck,
        int searchCapacity,
        AIMode aiMode
    )
    {
        this.shootability = shootability;
        this.slashability = slashability;
        this.acceleration = acceleration;
        this.luck = luck;
        this.searchCapacity = searchCapacity;
        this.aiMode = aiMode;
    }

    void Start()
    {
        // 必要な他コンポーネント取得
        gameManager = GameObject.Find("EventSystem").GetComponent<GameManager>();
        controller = GameObject.Find("EventSystem").GetComponent<Controller>();
        machine = gameObject.transform.parent.Find("Machine").GetComponent<Machine>();
        unit = gameObject.transform.parent.GetComponent<Unit>();

        // マシンにパイロット情報を渡す
        machine.SetPilot(GetComponent<Pilot>());

        // メタ情報の初期化
        lastKeyPressTime = 0f;
        dashThreshold = 0.25f;
    }

    void Update()
    {
        // ポーズ中はプレイヤー操作を受け付けない
        if (gameManager.IsPaused) return;

        if (unit.IsCpu)
        {
            currentDirection = CpuInput();
        }
        else
        {
            OnMove();
            OnShoot();
            OnSlash();
            OnShield();
        }
        MoveMachine();
    }

    // 移動方向取得
    public void OnMove()
    {
        currentDirection = controller._direction;
        if (controller.MovePhase == InputActionPhase.Started)
        {
            controller.SetMovePhase(InputActionPhase.Performed);
            isDoubleTap = IsDoubleTap();
        }
        
    }

    // CPU操作
    Vector2 CpuInput()
    {
        if (aiMode == AIMode.Simple)
        {
            return CpuSimple();
        }

        return Vector2.zero;
   }

    // 方向キー2度押し判定
    bool IsDoubleTap()
    {
        currentDirection = currentDirection.normalized;
        bool doubleTap = Time.time - lastKeyPressTime < dashThreshold;
        bool isSameDirection = currentDirection == lastDirection;

        if (doubleTap && isSameDirection)
        {
            // ダッシュの条件を満たしている
            return true;
        }

        // キーが押された方向を記憶
        if (currentDirection.magnitude > 0)
        {
            lastDirection = currentDirection;
            lastKeyPressTime = Time.time;
        }
        return false;
    }

    // 機体の移動
    void MoveMachine()
    {
        // 移動方向がある場合のみ速度を更新
        if (currentDirection.magnitude > 0)
        {
            // 旋回させる
            machine.Turn(currentDirection.x);

            // ブーストさせる
            machine.BoostBehaviour(true);

            if ((isDoubleTap || isDashing) && !unit.IsCpu)
            {
                // ダッシュ状態へ
                isDashing = true;
                machine.SetSpeed(currentDirection.normalized, true);
            }
            else
            {
                // 通常移動
                machine.SetSpeed(currentDirection.normalized, false);
            }
        }
        else
        {
            // ブースト解除
            machine.BoostBehaviour(false);

            // ダッシュ解除
            isDashing = false;

            // 速度を下げる
            machine.SetSpeed(currentDirection.normalized, false);
        }
    }

    // 射撃命令
    public void OnShoot()
    {
        if (controller.ShootPhase == InputActionPhase.Started)
        {
            controller.SetShootPhase(InputActionPhase.Performed);
            GameObject target = SearchTarget(searchCapacity);
            StartCoroutine(machine.Shoot(target, currentDirection));
        }
    }

    // 斬撃命令
    public void OnSlash()
    {
        if (controller.SlashPhase == InputActionPhase.Started)
        {
            controller.SetSlashPhase(InputActionPhase.Performed);
            StartCoroutine(machine.Slash());
        }
    }

    // 防御命令
    public void OnShield()
    {
        if (controller.ShieldPhase == InputActionPhase.Started)
        {
            controller.SetShieldPhase(InputActionPhase.Performed);
            // 防御命令
            StartCoroutine(machine.Defence());
        }
        else if (controller.ShieldPhase == InputActionPhase.Canceled)
        {
            // 防御解除命令
            machine.OffDefence();
        }
    }

    // 索敵
    GameObject SearchTarget(int searchCapacity)
    {
        // 索敵範囲
        float searchRange = Calculator.Instance.CalculateSearchRange(searchCapacity);

        if (unit.IsManual)
        {
            // マニュアル操作時ターゲットは設定しない
            return null;
        }

        // 自身のタグを確認し、敵のタグのオブジェクトを取得する
        string allyTag = gameObject.transform.parent.tag;
        string enemyTag;
        if (allyTag == "Blue")
        {
            enemyTag = "Red";
        }
        else
        {
            enemyTag = "Blue";
        }

        // 一番近い敵にターゲットを切り替える
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        float nearest = 999f;
        GameObject nearestObject = null;
        Vector2 myPosition = gameObject.transform.position;
        foreach (GameObject enemy in enemies)
        {
            Vector2 enemyPosition = enemy.transform.position;
            float distance = Vector2.Distance(enemyPosition, myPosition);
            if (distance < nearest && distance <= searchRange)
            {
                // 最短距離更新
                nearest = distance;
                nearestObject = enemy;
            }
        }
        return nearestObject;
    }

    Vector2 CpuSimple()
    {
        // 相手と自分の距離を求める
        Vector2 myPosition = Vector2.zero;
        Vector2 yourPosition = Vector2.zero;
        float distance;

        if (cpuPhaseTime < 0.5f)
        {
            // 少し止まる
            cpuDirection = Vector2.zero;
            cpuPhaseTime += Time.deltaTime;
        }
        else if (cpuPhaseTime < 1.5f)
        {
            // 少し動く
            myPosition = gameObject.transform.position;
            GameObject target = SearchTarget(searchCapacity);
            if (target != null)
            {
                yourPosition = target.transform.position;
                cpuDirection = yourPosition - myPosition;
            }
            cpuPhaseTime += Time.deltaTime;
        }
        else if (cpuPhaseTime < 2.0f)
        {
            // 少し止まる
            cpuDirection = Vector2.zero;
            cpuPhaseTime += Time.deltaTime;
        }
        else if (cpuPhaseTime < 2.5f)
        {
            // 少し動く
            myPosition = gameObject.transform.position;
            GameObject target = SearchTarget(searchCapacity);
            if (target != null)
            {
                yourPosition = target.transform.position;
                cpuDirection = yourPosition - myPosition;
            }
            cpuPhaseTime += Time.deltaTime;
        }
        else
        {
            myPosition = gameObject.transform.position;
            GameObject target = SearchTarget(searchCapacity);
            if (target != null)
            {
                yourPosition = target.transform.position;
                distance = Vector2.Distance(yourPosition, myPosition);
                if (distance < 1.2f)
                {
                    // 近くにいる場合斬撃
                    StartCoroutine(machine.Slash());
                }
                else
                {
                    // 離れている場合射撃
                    target = SearchTarget(searchCapacity);
                    yourPosition = target.transform.position;
                    cpuDirection = yourPosition - myPosition;
                    StartCoroutine(machine.Shoot(target, cpuDirection));
                }
            }

            cpuPhaseTime = 0f;
        }

        return cpuDirection;
    }
}
