using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Command : MonoBehaviour
{
    [SerializeField] private string distination; // 遷移先
    [SerializeField] private EventType eventType; // イベントタイプ
    [SerializeField] private bool isNextStage; // ステージ進めるか
    [SerializeField] private bool isInit; // ゲーム初期化するか（タイトルではじめから）

    public enum EventType
    {
        Transition,  // 別シーンへ遷移
        SwitchMenu, // 画面切り替え
        Quit,        // ゲームをやめる
        Other,       // その他（ToDo）
    }

    // コマンドのイベント
    public void CommandEvent(GameObject menuObject)
    {
        switch (eventType)
        {
            case EventType.Transition:
                TransitionAnotherScene();
                break;
            case EventType.SwitchMenu:
                SwitchMenu(menuObject);
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

        //　
        if (isInit) DataManager.Instance.currentStageNo = 0;

        SceneManager.LoadScene(distination);
    }

    // メニュー切り替え
    void SwitchMenu(GameObject menuObject)
    {
        // 現メニュー
        Menu currentMenu = menuObject.GetComponent<Menu>();

        // 新メニュー生成
        List<GameObject> menuPrefabs = currentMenu.MenuPrefabs;
        Debug.Log(menuPrefabs);
        GameObject menuPrefab = Common.Instance.FindObjectByName(menuPrefabs, distination);
        Debug.Log(menuPrefab);
        Instantiate(menuPrefab, menuObject.transform.parent);
        Debug.Log("新メニュー");

        // 現メニュー非活性
        Destroy(menuObject);
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
