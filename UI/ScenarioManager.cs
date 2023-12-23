using System.Collections.Generic;
using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    [SerializeField] private List<Scenario> scenarios; // シナリオダイアログプレハブ

    public enum ScenarioID
    {
        Tutorial, // チュートリアル
        Yukino, // 雪乃戦記
    }

    void Start()
    {
        // 必要な他コンポーネント取得
        GameObject canvas = GameObject.Find("Canvas");

        // シナリオを取得
        ScenarioID scenarioID = DataManager.Instance.currentScenarioID;
        Scenario scenario = scenarios.Find(scenario => scenario.scenarioID == scenarioID);
        
        // 対象のステージNoを取得
        int currentStageNo = DataManager.Instance.currentStageNo;

        // 対象のステージNoのシナリオを生成する
        Instantiate(scenario.dialoguePrefabs[currentStageNo], canvas.transform);
    }
}

[System.Serializable]
public class Scenario
{
    public ScenarioManager.ScenarioID scenarioID;
    public List<GameObject> dialoguePrefabs;
}
