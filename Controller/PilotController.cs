using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PilotController : MonoBehaviour
{
    // パイロットパラメータ
    [SerializeField] private string pilotName;
    [SerializeField] private int shootability;
    [SerializeField] private int slashability;
    [SerializeField] private int acceleration;
    [SerializeField] private int luck;
    [SerializeField] private int searchCapacity;
    [SerializeField] private Enums.AIMode aiMode;

    private MachineController machine; // Machineスクリプト
    private Controller controller; // コントローラ
    private GameManager gameManager; // ゲーム管理
    private DijkstraAlgorithm dijkstra; // 最短経路探索アルゴリズム
    private Unit unit;
    public Unit Unit
    {
        get { return unit; }
    }
    private PilotData pilotData;
    private PilotModel model;
    public PilotModel Model
    {
        get { return model; }
    }

    const float MACHINE_OFFSET = 0.39f; // Ray射出オフセット
    const float MIN_DISTANCE = 0.3f; // ノードへの到達みなし距離
 
    void Start()
    {
        // 必要な他コンポーネント取得
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        controller = GameObject.Find("EventSystem").GetComponent<Controller>();
        dijkstra = GameObject.Find("MapManager").GetComponent<DijkstraAlgorithm>();
        machine = transform.parent.Find(MachineConst.MACHINE).GetComponent<MachineController>();
        unit = transform.parent.GetComponent<Unit>();
        if (model == null) 
        {
            model = new PilotModel() {
                PilotName = this.pilotName,
                Shootability = this.shootability,
                Slashability = this.slashability,
                Acceleration = this.acceleration,
                Luck = this.luck,
                SearchCapacity = this.searchCapacity,
                AiMode = this.aiMode
            };
        }

        // マシンにパイロット情報を渡す
        machine.SetPilot(this);
    }

    void Update()
    {
        // ポーズ中はプレイヤー操作を受け付けない
        if (gameManager.IsPaused) return;

        if (unit.IsCpu)
        {
            // CPUが操作する
            CpuInput();
        }
        else
        {
            // プレイヤー操作を受け付ける
            OnMove();
            OnShoot();
            OnSlash();
            OnShield();
        }

        // マシンを動かす
        MoveMachine();
    }

    // CPU操作
    void CpuInput()
    {
        // AIモードに応じた挙動を行う
        switch (model.AiMode)
        {
            case Enums.AIMode.Simple:
                CpuSimple();
                break;
            case Enums.AIMode.Freedom:
                CpuFreedom();
                break;
            case Enums.AIMode.Tracking:
                CpuTracking();
                break;
            case Enums.AIMode.Balance:
                CpuBalance();
                break;
            case Enums.AIMode.Follow:
                CpuFollow();
                break;
            default:
                // どれにも該当しない場合は動かない
                model.CurrentDirection = Vector2.zero;
                break;
        }
   }

    // 移動方向取得
    public void OnMove()
    {
        model.CurrentDirection = controller._direction;
        if (controller.MovePhase == InputActionPhase.Started)
        {
            controller.SetMovePhase(InputActionPhase.Performed);

            // ダブルタップかどうか判定
            model.IsDoubleTap = PilotLogic.IsDoubleTap(
                Util.GetAngleByVector2(model.CurrentDirection, model.LastDirection),
                Util.GetDiffByFloat(Time.time, model.LastKeyPressTime)
            );

            //　前回押下時の方向と時間を更新
            model.LastDirection = model.CurrentDirection;
            model.LastKeyPressTime = Time.time;
        }
    }

    // 射撃命令
    public void OnShoot()
    {
        if (controller.ShootPhase == InputActionPhase.Started)
        {
            controller.SetShootPhase(InputActionPhase.Performed);

            // 相手タグを取得
            string targetTag = Util.GetOpponentTag(transform.parent.tag);

            //　ターゲットを設定
            GameObject target = SearchTarget(targetTag, gameObject);

            // 射撃
            StartCoroutine(machine.Shoot(target, model.CurrentDirection));
        }
    }

    // 斬撃命令
    public void OnSlash()
    {
        if (controller.SlashPhase == InputActionPhase.Started)
        {
            controller.SetSlashPhase(InputActionPhase.Performed);

            // 斬撃
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

    // 機体の移動
    void MoveMachine()
    {
        // 移動方向がある場合のみ速度を更新
        if (model.CurrentDirection.magnitude > 0)
        {
            // 旋回させる
            machine.Turn(model.CurrentDirection.x);

            // ブーストさせる
            machine.BoostBehaviour(true);

            if ((model.IsDoubleTap || model.IsDashing) && machine.Model.CurrentPP > 0)
            {
                // ダッシュ状態へ
                model.IsDashing = true;
            }
            else
            {
                // ダッシュ解除
                model.IsDashing = false;
                model.IsDoubleTap = false;
            }
        }
        else
        {
            // ブースト解除
            machine.BoostBehaviour(false);

            // ダッシュ解除
            model.IsDashing = false;
        }

        // マシンの移動速度をセット
        machine.SetSpeed(model.CurrentDirection.normalized, model.IsDashing);
    }

    // 索敵
    public GameObject SearchTarget(string searchTag, GameObject searchObject)
    {
        // 索敵範囲
        float searchRange = Calculator.CalcSearchRange(model.SearchCapacity);

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
    public GameObject SearchBallet(string searchTag, GameObject searchObject)
    {
        // 索敵範囲
        float searchRange = Calculator.CalcSearchRange(model.SearchCapacity / 2);

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
            bool isEnemy = transform.parent.tag == TagConst.BLUE ? target.GetComponent<Ballet>().IsEnemy : !target.GetComponent<Ballet>().IsEnemy;
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
    public GameObject SearchAlly(string searchTag, GameObject searchObject)
    {
        // 索敵範囲
        float searchRange = Calculator.CalcSearchRange(model.SearchCapacity);

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
        model.CurrentDirection = Vector2.zero;
    }
    // 近づく
    void Approach(Vector2 yourPosition, Vector2 myPosition)
    {
        model.CurrentDirection = yourPosition - myPosition;
    }
    // 離れる
    void Leave(Vector2 yourPosition, Vector2 myPosition)
    {
        model.CurrentDirection = myPosition - yourPosition;
    }
    // 避ける
    void Avert(Vector2 yourPosition, Vector2 myPosition)
    {
        // 弾に対して垂直方向に移動
        model.CurrentDirection = Vector2.Perpendicular(myPosition - yourPosition) * Util.GetRandomSign();
    }
    // ランダムに動く
    void MoveRandom()
    {
        model.CurrentDirection = Common.GetRandomVector2();
    }
    // 攻撃する
    void Attack(Vector2 yourPosition, Vector2 myPosition, GameObject target)
    {
        if (Common.IsSlashRange(yourPosition, myPosition))
        {
            // 近距離の場合斬撃
            StartCoroutine(machine.Slash());
        }
        else
        {
            // 離れている時射撃
            StartCoroutine(machine.Shoot(target, model.CurrentDirection));
        }
    }
    // 防御する
    void Defense()
    {
        StartCoroutine(machine.Defence());
    }
    // 探索する
    void Explore(Vector2 myPosition, GameObject target)
    {
        // 障害物がある場合、ネットワーク沿いに移動
        GameObject myNearestNodeObject = SearchTarget("Node", gameObject);
        Node myNearestNode = myNearestNodeObject.GetComponent<Node>();
        Node targetNearestNode = SearchTarget("Node", target).GetComponent<Node>();
        Vector2 myNearestNodePosition = myNearestNodeObject.transform.position;

        if (model.CurrentNode == null)
        {
            // ネットワーク上にいない場合は一番近いノードに向かう
            model.NextNode = myNearestNode;
        }

        if (Vector2.Distance(myNearestNodePosition, myPosition) < MIN_DISTANCE && model.CurrentNode != myNearestNode)
        {
            // 一番近いノードに到達したら次のノードに向かう
            // 現在のノードを更新する
            model.CurrentNode = myNearestNode;
            model.NextNode = dijkstra.FindShortestPath(model.CurrentNode, targetNearestNode);
        }
        
        model.CurrentDirection = (Vector2)model.NextNode.transform.position - myPosition;
    }

    // 経過時間を加算
    void AddDeltaTime()
    {
        model.CpuPhaseTime += Time.deltaTime;
    }

    // 単純なCPU
    void CpuSimple()
    {
        // 相手と自分の距離を求める
        Vector2 myPosition = gameObject.transform.position;
        string targetTag = Util.GetOpponentTag(transform.parent.tag);
        GameObject target = SearchTarget(targetTag, gameObject);
        if (target != null)
        {
            Vector2 yourPosition = target.transform.position;
            if (model.CpuPhaseTime < 0.5f)
            {
                // 少し止まる
                Stop();
            }
            else if (model.CpuPhaseTime < 1.5f)
            {
                // 少し動く
                Approach(yourPosition, myPosition);
            }
            else if (model.CpuPhaseTime < 2.0f)
            {
                // 少し止まる
                Stop();
            }
            else if (model.CpuPhaseTime < 2.5f)
            {
                // 少し動く
                Approach(yourPosition, myPosition);
            }
            else
            {
                // 攻撃
                Attack(yourPosition, myPosition, target);

                // 行動パターンをリセット
                model.CpuPhaseTime = 0;
            }
            AddDeltaTime();
        }
    }

    // 自由なCPU
    void CpuFreedom()
    {
        // 動く
        Vector2 myPosition = gameObject.transform.position;
        string targetTag = Util.GetOpponentTag(transform.parent.tag);
        GameObject target = SearchTarget(targetTag, gameObject);
        if (target != null)
        {
            // ターゲットが見える場合
            Vector2 yourPosition = target.transform.position;
            if (model.CpuPhaseTime == 0f)
            {
                // 少し動く
                Approach(yourPosition, myPosition);
            }
            else if (model.CpuPhaseTime < 0.1f)
            {
                // 何もしない？
            }
            else
            {
                // 攻撃
                Attack(yourPosition, myPosition, target);
                model.CpuPhaseTime = 0f;
            }
            AddDeltaTime();
        }
        else
        {
            // ターゲットが見えない場合ランダムに動く
            if (model.CpuPhaseTime == 0f)
            {
                // 向きを変更
                MoveRandom();
            }
            else if (model.CpuPhaseTime < 0.5f)
            {
                // 何もしない？
            }
            else
            {
                model.CpuPhaseTime = 0;
            }
            AddDeltaTime();
        }
    }

    // 追跡するCPU
    void CpuTracking()
    {
        // 動く
        Vector2 myPosition = gameObject.transform.position;
        string targetTag = Util.GetOpponentTag(transform.parent.tag);
        GameObject target = SearchTarget(targetTag, gameObject);
        if (target != null)
        {
            // ターゲットが見える場合
            // 索敵範囲
            float searchRange = Calculator.CalcSearchRange(model.SearchCapacity);

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
                model.CurrentDirection = yourPosition - myPosition;
                model.CurrentNode = null;
            }
        }
        else
        {
            // ターゲットが見えない場合動かない
            model.CurrentDirection = Vector2.zero;
            model.CurrentNode = null;
        }
    }

    // バランス型CPU
    void CpuBalance()
    {
        // 動く
        Vector2 myPosition = gameObject.transform.position;
        string targetTag = Util.GetOpponentTag(transform.parent.tag);
        GameObject target = SearchTarget(targetTag, gameObject);
        if (target != null)
        {
            // ターゲットが見える場合
            // 索敵範囲
            float searchRange = Calculator.CalcSearchRange(model.SearchCapacity);

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
                GameObject targetBallet = SearchBallet("Ballet", gameObject);
                if (targetBallet == null)
                {
                    if (model.CpuPhaseTime == 0f)
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
                            model.IsAttack = true;
                            Debug.Log("攻撃する");
                        }
                        AddDeltaTime();
                    }
                    else if (model.CpuPhaseTime < 0.1f && model.IsAttack)
                    {
                        model.IsAttack = false;
                        Attack(yourPosition, myPosition, target);
                        AddDeltaTime();
                    }
                    else if (model.CpuPhaseTime < 0.5f)
                    {
                        // 変化なし
                        AddDeltaTime();
                    }
                    else
                    {
                        // 行動パターンリセット
                        model.CpuPhaseTime = 0f;
                        model.IsDashing = false;
                        machine.OffDefence();
                        model.CurrentNode = null;
                        Debug.Log("リセット");
                    }
                }
                else
                {
                    if (model.CpuPhaseTime == 0f)
                    {
                        // 弾が近くにあれば回避or防御
                        float random = Random.Range(0f, 1f);
                        if (random <= 0.5f)
                        {
                            // 回避
                            model.IsDashing = true;
                            Avert(targetBallet.transform.position, myPosition);
                            Debug.Log("回避");
                        }
                        else
                        {
                            // 防御
                            Defense();
                            Debug.Log("防御");
                        }
                        AddDeltaTime();
                    }
                    else if (model.CpuPhaseTime < 0.5f)
                    {
                        // 変化なし
                        AddDeltaTime();
                    }
                    else
                    {
                        // 行動パターンリセット
                        model.CpuPhaseTime = 0f;
                        model.IsDashing = false;
                        machine.OffDefence();
                        model.CurrentNode = null;
                        Debug.Log("リセット");
                    }
                }
            }
        }
        else
        {
            // ターゲットが見えない場合、ネットワーク沿いに移動
            string targetStation = PilotLogic.GetStationNameByTag(transform.parent.tag);
            GameObject opponentStationObject = GameObject.Find(targetStation);
            Explore(myPosition, opponentStationObject);
            Debug.Log("探索2");
        }
    }

    // 味方追従CPU
    void CpuFollow()
    {
        Vector2 myPosition = gameObject.transform.position;
        string targetTag = Util.GetOpponentTag(transform.parent.tag);
        GameObject target = SearchTarget(targetTag, gameObject);
        if (target != null)
        {
            // ターゲットが見える場合、バランス行動
            CpuBalance();
        }
        else
        {
            // ターゲットが見えない場合、プレイヤーに追従
            GameObject ally = SearchAlly(transform.parent.tag, gameObject);
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
    }

    // データ初期化
    public void InitializeData()
    {
        pilotData = transform.parent.GetComponent<Unit>().UnitData.pilotData;
        model = new PilotModel() {
            PilotName = pilotData.pilotName,
            Shootability = pilotData.shootability,
            Slashability = pilotData.slashability,
            Acceleration = pilotData.acceleration,
            Luck = pilotData.luck,
            SearchCapacity = pilotData.searchCapacity,
            AiMode = pilotData.aiMode
        };
    }
}
