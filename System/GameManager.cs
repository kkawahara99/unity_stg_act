using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject[] stageMapPrefabs; // ステージマッププレハブ

    private bool isPaused = false;
    public bool IsPaused { get => isPaused; }
    private Controller controller; // コントローラ
    private CameraController cameraController; // カメラ

    void Awake()
    {
        // 必要な他コンポーネント取得
        controller = GameObject.Find("EventSystem").GetComponent<Controller>();
        cameraController = GameObject.Find("Main Camera").GetComponent<CameraController>();

        // 対象のステージNoを取得
        int currentStageNo = DataManager.Instance.currentStageNo;

        // 対象のステージNoのマップを生成する
        GameObject mapObject = Instantiate(stageMapPrefabs[currentStageNo], Vector2.zero, Quaternion.identity);
        mapObject.name = "MapManager";
    }

    void Update()
    {
        OnStart();
    }

    // スタートボタン検知
    public void OnStart()
    {
        if (controller.StartPhase == InputActionPhase.Started)
        {
            controller.SetStartPhase(InputActionPhase.Performed);
            Pause();
        }
    }

    // ゲームをポーズする
    void Pause()
    {
        // シーン内のすべてのRigidbody2Dを取得
        Rigidbody2D[] allRigidbodies = FindObjectsOfType<Rigidbody2D>();

        // ボーズ状態をスイッチ
        isPaused = !isPaused;

        foreach (Rigidbody2D rb in allRigidbodies)
        {
            // ポーズ中はisKinematicをtrue
            // ポーズ解除時はisKinematicをfalseにする
            rb.isKinematic = isPaused;

        }

        // ゲーム速度停止
        Color color;
        if (isPaused)
        {
            // ポーズ中はゲーム速度停止
            Time.timeScale = 0f;
            // 前景色半黒
            color = new Color(0f, 0f, 0f, 0.5f);
        }
        else
        {
            // ポーズ解除時はゲーム速度戻す
            Time.timeScale = 1f;
            // 前景色クリア
            color = new Color(0f, 0f, 0f, 0f);
        }

        // 画面前景色を変更
        cameraController.SetForeground(color);
    }
}
