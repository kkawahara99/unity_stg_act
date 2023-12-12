using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    [SerializeField] private GameObject[] dialoguePrefabs; // シナリオダイアログプレハブ

    void Start()
    {
        // 必要な他コンポーネント取得
        GameObject canvas = GameObject.Find("Canvas");

        // 対象のステージNoを取得
        int currentStageNo = DataManager.Instance.currentStageNo;

        // 対象のステージNoのシナリオを生成する
        Instantiate(dialoguePrefabs[currentStageNo], canvas.transform);
    }
}
