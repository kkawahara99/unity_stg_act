using UnityEngine;
using UnityEngine.SceneManagement;

public class Command : MonoBehaviour
{
    [SerializeField] private string distinationScene; // 遷移先のシーン
    [SerializeField] private EventType eventType; // イベントタイプ
    [SerializeField] private bool isNextStage; // ステージ進めるか

    public enum EventType
    {
        Transition,  // 別シーンへ遷移
        SmallScreen, // 小画面を表示
        Quit,        // ゲームをやめる
        Other,       // その他（ToDo）
    }

    // コマンドのイベント
    public void CommandEvent()
    {
        switch (eventType)
        {
            case EventType.Transition:
                TransitionAnotherScene();
                break;
            case EventType.SmallScreen:
                break;
            case EventType.Quit:
                Quit();
                break;
            case EventType.Other:
                break;
        }
    }

    // 画面遷移
    void TransitionAnotherScene()
    {
        // 「次のステージへ」のときは現在のステージNoをインクリメント
        if (isNextStage) DataManager.Instance.currentStageNo += 1;

        SceneManager.LoadScene(distinationScene);
    }

    // ゲームをやめる
    void Quit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
