using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pilot : MonoBehaviour
{
    [SerializeField] private string pilotName; // パイロット名
    public string PilotName { get => pilotName; }
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

    public int objectNo;

    public enum AIMode
    {
        Simple,    // 単純
        Freedom,   // 自由
        Tracking,  // 追跡
        Assault,   // 突撃
        Avoidance, // 回避
        Shooting,  // 射撃
        Balance,   // バランス
        Defense,   // 防衛
        Follow,   // 追従
    }

    private Vector2 cpuDirection; // CPUの移動方向
    private float cpuPhaseTime; // CPUの行動フェーズ切替時間
    private bool isAttack; // CPUの行動が攻撃
    private Machine machine; // Machineスクリプト
    private bool isDoubleTap; // 方向キー2連続押しかどうか
    private bool isDashing; // ダッシュ中かどうか
    private Vector2 lastDirection = Vector2.zero; // 前回の方向
    private float lastKeyPressTime = 0f; // 前回のキー押下時間
    const float DASH_THRESHOLD = 0.25f; // ダッシュ閾値
    private Vector2 currentDirection; // 現在の入力方向
    private Controller controller; // コントローラ
    private GameManager gameManager; // ゲーム管理
    const float MACHINE_OFFSET = 0.39f; // Ray射出オフセット
    const float MIN_DISTANCE = 0.3f; // ノードへの到達みなし距離
    private DijkstraAlgorithm dijkstra; // 最短経路探索アルゴリズム
    private Node currentNode; // 現在のノード
    private Node nextNode; // 次のノード
    private Unit unit;
    public Unit Unit { get => unit; }
    private PilotData pilotData;
    public PilotData PilotData { get => pilotData; }

    void Start()
    {
        // 必要な他コンポーネント取得
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        controller = GameObject.Find("EventSystem").GetComponent<Controller>();
        dijkstra = GameObject.Find("MapManager").GetComponent<DijkstraAlgorithm>();
        machine = gameObject.transform.parent.Find("Machine").GetComponent<Machine>();
        unit = gameObject.transform.parent.GetComponent<Unit>();

        // マシンにパイロット情報を渡す
        machine.SetPilot(GetComponent<Pilot>());

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
        switch (aiMode)
        {
            case AIMode.Simple:
                return CpuSimple();
            case AIMode.Freedom:
                return CpuFreedom();
            case AIMode.Tracking:
                return CpuTracking();
            case AIMode.Balance:
                return CpuBalance();
            case AIMode.Follow:
                return CpuFollow();
            default:
                return Vector2.zero;
        }
   }

    // 方向キー2度押し判定
    bool IsDoubleTap()
    {
        currentDirection = currentDirection.normalized;
        bool doubleTap = Time.time - lastKeyPressTime < DASH_THRESHOLD;
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

            if ((isDoubleTap || isDashing) && machine.CurrentPP > 0)
            {
                // ダッシュ状態へ
                isDashing = true;
                machine.SetSpeed(currentDirection.normalized, isDashing);
            }
            else
            {
                // ダッシュ解除
                isDashing = false;
                isDoubleTap = false;

                machine.SetSpeed(currentDirection.normalized, isDashing);
            }
        }
        else
        {
            // ブースト解除
            machine.BoostBehaviour(false);

            // ダッシュ解除
            isDashing = false;

            // 速度を下げる
            machine.SetSpeed(currentDirection.normalized, isDashing);
        }
    }

    // 射撃命令
    public void OnShoot()
    {
        if (controller.ShootPhase == InputActionPhase.Started)
        {
            controller.SetShootPhase(InputActionPhase.Performed);
            string targetTag = transform.parent.tag =="Blue" ? "Red" : "Blue";
            GameObject target = SearchTarget(searchCapacity, targetTag, gameObject);
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
    public GameObject SearchTarget(int searchCapacity, string searchTag, GameObject searchObject)
    {
        // 索敵範囲
        float searchRange = Calculator.Instance.CalculateSearchRange(searchCapacity);

        if (unit.IsManual)
        {
            // マニュアル操作時ターゲットは設定しない
            return null;
        }

        // 一番近いターゲットを切り替える
        GameObject[] targets = GameObject.FindGameObjectsWithTag(searchTag);

        float nearest = 999f;
        GameObject nearestObject = null;
        Vector2 searchPosition = searchObject.transform.position;
        foreach (GameObject target in targets)
        {
            Vector2 targetPosition = target.transform.position;
            float distance = Vector2.Distance(targetPosition, searchPosition);
            if (distance < nearest && distance <= searchRange)
            {
                // 最短距離更新
                nearest = distance;
                nearestObject = target;
            }
        }
        return nearestObject;
    }

    // 弾の検知
    public GameObject SearchBallet(int searchCapacity, string searchTag, GameObject searchObject)
    {
        // 索敵範囲
        float searchRange = Calculator.Instance.CalculateSearchRange(searchCapacity / 2);

        // 一番近いターゲットを切り替える
        GameObject[] targets = GameObject.FindGameObjectsWithTag(searchTag);

        float nearest = 999f;
        GameObject nearestObject = null;
        Vector2 searchPosition = searchObject.transform.position;
        foreach (GameObject target in targets)
        {
            Vector2 targetPosition = target.transform.position;
            float distance = Vector2.Distance(targetPosition, searchPosition);

            // ターゲットの弾が敵のものかどうか
            bool isEnemy = transform.parent.tag == "Blue" ? target.GetComponent<Ballet>().IsEnemy : !target.GetComponent<Ballet>().IsEnemy;
            if (distance < nearest && distance <= searchRange && isEnemy)
            {
                // 最短距離更新
                nearest = distance;
                nearestObject = target;
            }
        }
        return nearestObject;
    }

    // 追従
    public GameObject SearchAlly(int searchCapacity, string searchTag, GameObject searchObject)
    {
        // 索敵範囲
        float searchRange = Calculator.Instance.CalculateSearchRange(searchCapacity);

        // 一番近いターゲットを切り替える
        GameObject[] targets = GameObject.FindGameObjectsWithTag(searchTag);

        float nearest = 999f;
        GameObject nearestObject = null;
        Vector2 searchPosition = searchObject.transform.position;
        foreach (GameObject target in targets)
        {
            Vector2 targetPosition = target.transform.position;
            float distance = Vector2.Distance(targetPosition, searchPosition);
            if (distance < nearest && distance <= searchRange && target.GetComponent<Unit>() != null)
            {
                // プレイヤーのみ追従
                if (!target.GetComponent<Unit>().IsCpu)
                {
                    // 最短距離更新
                    nearest = distance;
                    nearestObject = target;
                }
            }
        }
        return nearestObject;
    }

    // CPU行動モジュール
    // 停止する
    void Stop()
    {
        cpuDirection = Vector2.zero;
        cpuPhaseTime += Time.deltaTime;
    }
    // 近づく
    void Approach(Vector2 yourPosition, Vector2 myPosition)
    {
        cpuDirection = yourPosition - myPosition;
        cpuPhaseTime += Time.deltaTime;
    }
    // 離れる
    void Leave(Vector2 yourPosition, Vector2 myPosition)
    {
        cpuDirection = myPosition - yourPosition;
        cpuPhaseTime += Time.deltaTime;
    }
    // 避ける
    void Avert(Vector2 yourPosition, Vector2 myPosition)
    {
        // 弾に対して垂直方向に移動
        int randomValue = Random.Range(0, 2) * 2 - 1;
        cpuDirection = Vector2.Perpendicular(myPosition - yourPosition) * randomValue;
        cpuPhaseTime += Time.deltaTime;
    }
    // ランダムに動く
    void MoveRandom()
    {
        cpuDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        cpuPhaseTime += Time.deltaTime;
    }
    // 攻撃する
    void Attack(Vector2 yourPosition, Vector2 myPosition, GameObject target)
    {
        float distanceX = yourPosition.x - myPosition.x;
        float distanceY = yourPosition.y - myPosition.y;
        if (Mathf.Abs(distanceX) < 0.7f && Mathf.Abs(distanceY) < 0.3f)
        {
            // 近距離の場合斬撃
            StartCoroutine(machine.Slash());
        }
        else
        {
            // 離れている時射撃
            StartCoroutine(machine.Shoot(target, cpuDirection));
        }

        cpuPhaseTime += Time.deltaTime;
    }
    // 防御する
    void Defense()
    {
        StartCoroutine(machine.Defence());
        cpuPhaseTime += Time.deltaTime;
    }
    // 探索する
    void Explore(Vector2 myPosition, GameObject target)
    {
        // 障害物がある場合、ネットワーク沿いに移動
        GameObject myNearestNodeObject = SearchTarget(searchCapacity, "Node", gameObject);
        Node myNearestNode = myNearestNodeObject.GetComponent<Node>();
        Node targetNearestNode = SearchTarget(searchCapacity, "Node", target).GetComponent<Node>();
        Vector2 myNearestNodePosition = myNearestNodeObject.transform.position;

        if (currentNode == null)
        {
            // ネットワーク上にいない場合は一番近いノードに向かう
            nextNode = myNearestNode;
        }

        if (Vector2.Distance(myNearestNodePosition, myPosition) < MIN_DISTANCE && currentNode != myNearestNode)
        {
            // 一番近いノードに到達したら次のノードに向かう
            // 現在のノードを更新する
            currentNode = myNearestNode;
            nextNode = dijkstra.FindShortestPath(currentNode, targetNearestNode);
        }
        
        Debug.Log(nextNode);
        cpuDirection = (Vector2)nextNode.transform.position - myPosition;
    }

    // 単純なCPU
    Vector2 CpuSimple()
    {
        // 相手と自分の距離を求める
        Vector2 myPosition = gameObject.transform.position;
        string targetTag = transform.parent.tag =="Blue" ? "Red" : "Blue";
        GameObject target = SearchTarget(searchCapacity, targetTag, gameObject);
        if (target != null)
        {
            Vector2 yourPosition = target.transform.position;
            if (cpuPhaseTime < 0.5f)
            {
                // 少し止まる
                Stop();
            }
            else if (cpuPhaseTime < 1.5f)
            {
                // 少し動く
                Approach(yourPosition, myPosition);
            }
            else if (cpuPhaseTime < 2.0f)
            {
                // 少し止まる
                Stop();
            }
            else if (cpuPhaseTime < 2.5f)
            {
                // 少し動く
                Approach(yourPosition, myPosition);
            }
            else
            {
                // 攻撃
                Attack(yourPosition, myPosition, target);

                // 行動パターンをリセット
                cpuPhaseTime = 0;
            }
        }

        return cpuDirection;
    }

    // 自由なCPU
    Vector2 CpuFreedom()
    {
        // 動く
        Vector2 myPosition = gameObject.transform.position;
        string targetTag = transform.parent.tag == "Blue" ? "Red" : "Blue";
        GameObject target = SearchTarget(searchCapacity, targetTag, gameObject);
        if (target != null)
        {
            // ターゲットが見える場合
            Vector2 yourPosition = target.transform.position;
            if (cpuPhaseTime == 0f)
            {
                // 少し動く
                Approach(yourPosition, myPosition);
            }
            else if (cpuPhaseTime < 0.1f)
            {
                cpuPhaseTime += Time.deltaTime;
            }
            else if (cpuPhaseTime >= 0.1f)
            {
                // 攻撃
                Attack(yourPosition, myPosition, target);
                cpuPhaseTime = 0f;
            }
        }
        else
        {
            // ターゲットが見えない場合ランダムに動く
            if (cpuPhaseTime == 0f)
            {
                // 向きを変更
                MoveRandom();
            }
            else if (cpuPhaseTime > 0.5f)
            {
                cpuPhaseTime = 0f;
            }
            else
            {
                cpuPhaseTime += Time.deltaTime;
            }
        }

        return cpuDirection;
    }

    // 追跡するCPU
    Vector2 CpuTracking()
    {
        // 動く
        Vector2 myPosition = gameObject.transform.position;
        string targetTag = transform.parent.tag == "Blue" ? "Red" : "Blue";
        GameObject target = SearchTarget(searchCapacity, targetTag, gameObject);
        if (target != null)
        {
            // ターゲットが見える場合
            // 索敵範囲
            float searchRange = Calculator.Instance.CalculateSearchRange(searchCapacity);

            // ターゲットの位置を把握し自分が動いていなかったら方角を決める
            Vector2 yourPosition = target.transform.position;

            // レイキャストを使用して前方に障害物があるか判定
            Vector2 targetDirection = yourPosition - myPosition;
            Vector2 rayStartPosition = new Vector2(transform.position.x + targetDirection.normalized.x * MACHINE_OFFSET, transform.position.y + targetDirection.normalized.y * MACHINE_OFFSET);
            RaycastHit2D hit = Physics2D.Raycast(rayStartPosition, targetDirection, searchRange - MACHINE_OFFSET);
            Debug.DrawLine(rayStartPosition, hit.point, Color.red);

            if (hit.collider != null && hit.collider.CompareTag("Wall"))
            {
                // 障害物がある場合、ネットワーク沿いに移動
                Explore(myPosition, target);
            }
            else
            {
                // 障害物がない場合、ターゲットを追跡
                cpuDirection = yourPosition - myPosition;
                currentNode = null;
            }
        }
        else
        {
            // ターゲットが見えない場合動かない
            cpuDirection = Vector2.zero;
            currentNode = null;
        }

        return cpuDirection;
    }

    // バランス型CPU
    Vector2 CpuBalance()
    {
        // 動く
        Vector2 myPosition = gameObject.transform.position;
        string targetTag = transform.parent.tag == "Blue" ? "Red" : "Blue";
        GameObject target = SearchTarget(searchCapacity, targetTag, gameObject);
        if (target != null)
        {
            // ターゲットが見える場合
            // 索敵範囲
            float searchRange = Calculator.Instance.CalculateSearchRange(searchCapacity);

            // ターゲットの位置を把握し自分が動いていなかったら方角を決める
            Vector2 yourPosition = target.transform.position;

            // レイキャストを使用して前方に障害物があるか判定
            Vector2 targetDirection = yourPosition - myPosition;
            Vector2 rayStartPosition = new Vector2(transform.position.x + targetDirection.normalized.x * MACHINE_OFFSET, transform.position.y + targetDirection.normalized.y * MACHINE_OFFSET);
            RaycastHit2D hit = Physics2D.Raycast(rayStartPosition, targetDirection, searchRange - MACHINE_OFFSET);
            Debug.DrawLine(rayStartPosition, hit.point, Color.red);

            if (hit.collider != null && hit.collider.CompareTag("Wall"))
            {
                // 障害物がある場合、ネットワーク沿いに移動
                Explore(myPosition, target);
                Debug.Log("探索1");
            }
            else
            {
                // 障害物がない場合、以下行動パターン
                // 敵の弾を検知
                GameObject targetBallet = SearchBallet(searchCapacity, "Ballet", gameObject);
                if (targetBallet == null)
                {
                    if (cpuPhaseTime == 0f)
                    {
                        // 弾が近くになければたたかう
                        float random = Random.Range(0f, 1f);
                        if (random <= 0.1f)
                        {
                            // 様子を見る
                            Stop();
                            Debug.Log("様子を見る");
                        }
                        else if (random <= 0.2f)
                        {
                            // ランダムに動く
                            MoveRandom();
                            Debug.Log("ランダムに動く");
                        }
                        else if (random <= 0.5f)
                        {
                            // 近づく
                            Approach(yourPosition, myPosition);
                            Debug.Log("近づく");
                        }
                        else
                        {
                            // 攻撃する
                            Approach(yourPosition, myPosition);
                            isAttack = true;
                            Debug.Log("攻撃する");
                        }
                    }
                    else if (cpuPhaseTime < 0.1f && isAttack)
                    {
                        isAttack = false;
                        Attack(yourPosition, myPosition, target);

                    }
                    else if (cpuPhaseTime < 0.5f)
                    {
                        cpuPhaseTime += Time.deltaTime;
                    }
                    else
                    {
                        // 行動パターンリセット
                        cpuPhaseTime = 0f;
                        isDashing = false;
                        machine.OffDefence();
                        currentNode = null;
                        Debug.Log("リセット");
                    }
                }
                else
                {
                    if (cpuPhaseTime == 0f)
                    {
                        // 弾が近くにあれば回避or防御
                        float random = Random.Range(0f, 1f);
                        if (random <= 0.5f)
                        {
                            // 回避
                            isDashing = true;
                            Avert(targetBallet.transform.position, myPosition);
                            Debug.Log("回避");
                        }
                        else
                        {
                            // 防御
                            Defense();
                            Debug.Log("防御");
                        }
                    }
                    else if (cpuPhaseTime < 0.5f)
                    {
                        cpuPhaseTime += Time.deltaTime;
                    }
                    else
                    {
                        // 行動パターンリセット
                        cpuPhaseTime = 0f;
                        isDashing = false;
                        machine.OffDefence();
                        currentNode = null;
                        Debug.Log("リセット");
                    }
                }
            }
        }
        else
        {
            // ターゲットが見えない場合、ネットワーク沿いに移動
            string targetStation = transform.parent.tag == "Blue" ? "StationEnemy" : "Station";
            GameObject opponentStationObject = GameObject.Find(targetStation);
            Explore(myPosition, opponentStationObject);
            Debug.Log("探索2");
        }

        return cpuDirection;
    }

    // 味方追従CPU
    Vector2 CpuFollow()
    {
        Vector2 myPosition = gameObject.transform.position;
        string targetTag = transform.parent.tag == "Blue" ? "Red" : "Blue";
        GameObject target = SearchTarget(searchCapacity, targetTag, gameObject);
        if (target != null)
        {
            // ターゲットが見える場合、バランス行動
            CpuBalance();
        }
        else
        {
            // ターゲットが見えない場合、プレイヤーに追従
            string allyTag = transform.parent.tag == "Blue" ? "Blue" : "Red";
            GameObject ally = SearchAlly(searchCapacity, allyTag, gameObject);
            if (ally != null)
            {
                // プレイヤーが見える場合追従
                Vector2 yourPosition = ally.transform.position;
                if (Vector2.Distance(yourPosition, myPosition) >= 1f)
                {
                    Approach(yourPosition, myPosition);
                }
                else
                {
                    Stop();
                }
            }
            else{
                // プレイヤーが見えない場合、バランス行動
                CpuBalance();
            }
        }

        return cpuDirection;
    }

    // データ初期化
    public void InitializeData()
    {
        pilotData = transform.parent.GetComponent<Unit>().UnitData.pilotData;
        this.pilotName = pilotData.pilotName;
        this.shootability = pilotData.shootability;
        this.slashability = pilotData.slashability;
        this.acceleration = pilotData.acceleration;
        this.luck = pilotData.luck;
        this.searchCapacity = pilotData.searchCapacity;
        this.aiMode = pilotData.aiMode;
    }
}
