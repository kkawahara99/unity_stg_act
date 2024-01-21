using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Command : MonoBehaviour
{
    [SerializeField] private string distination; // 遷移先
    [SerializeField] private ScenarioManager.ScenarioID scenarioID; // シナリオID
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
        if (isInit)
        {
            // 初めからのとき
            // 現在のステージNoを初期化
            DataManager.Instance.currentStageNo = 0;
            // 現在のシナリオを初期化
            DataManager.Instance.currentScenarioID = scenarioID;

            // 初期データ登録
            StationData data = DataManager.Instance.stationData;
            data.hitPoint = 50;
            data.atk= 0;
            data.def = 10;
            data.luck = 0;
            // MachineData machineData = new MachineData();
            // machineData.machineName = "Gimo";
            // machineData.hitPoint = 20;
            // machineData.propellantPoint = 20;
            // machineData.atk= 10;
            // machineData.def = 10;
            // machineData.spd = 10;
            PilotData pilotData = new PilotData();
            pilotData.pilotName = "アナタ";
            pilotData.shootability = 10;
            pilotData.slashability = 10;
            pilotData.acceleration = 10;
            pilotData.luck = 10;
            pilotData.searchCapacity = 10;
            pilotData.aiMode = Enums.AIMode.Balance;
            pilotData.level = 1;
            UnitData unitData = new UnitData();
            unitData.isCpu = false;
            unitData.isManual = false; // オプションに依存予定
            unitData.color = new Color(0.5f, 0.5f, 0.75f, 1f);
            unitData.machineKey = "GimoX";
            unitData.mainWeaponKey = "BeamGun";
            unitData.handWeaponKey = "BeamKnife";
            unitData.shieldKey = "Shield";
            // unitData.machineData = machineData;
            unitData.pilotData = pilotData;
            data.unitDatas = new List<UnitData>();
            data.unitDatas.Add(unitData);
            // DataManager.Instance.stationData = data;
            // GameObject stationObject = Instantiate(DataManager.Instance.stationObject, Vector2.zero, Quaternion.identity);
            // stationObject.GetComponent<Station>().UnitObjects.Add();
            // DataManager.Instance.stationObject = stationObject;
        }
        if (DataManager.Instance.currentStageNo == 6)
        {
            // ToDo: 味方機を増やす
            StationData data = DataManager.Instance.stationData;
            // MachineData machineData = new MachineData();
            // machineData.machineName = "Gimo";
            // machineData.hitPoint = 10;
            // machineData.propellantPoint = 10;
            // machineData.atk= 10;
            // machineData.def = 10;
            // machineData.spd = 10;
            PilotData pilotData = new PilotData();
            pilotData.pilotName = "Ally1";
            pilotData.shootability = 10;
            pilotData.slashability = 10;
            pilotData.acceleration = 10;
            pilotData.luck = 10;
            pilotData.searchCapacity = 10;
            pilotData.aiMode = Enums.AIMode.Follow;
            UnitData unitData = new UnitData();
            unitData.isCpu = true;
            unitData.isManual = false; // オプションに依存予定
            unitData.color = new Color(0.5f, 0.5f, 0.75f, 1f);
            unitData.machineKey = "Gimo";
            unitData.mainWeaponKey = "BeamGun";
            unitData.handWeaponKey = "BeamKnife";
            unitData.shieldKey = "Shield";
            // unitData.machineData = machineData;
            unitData.pilotData = pilotData;
            data.unitDatas.Add(unitData);
        }
        if (DataManager.Instance.currentStageNo == 7)
        {
            // ToDo: 味方機を増やす
            StationData data = DataManager.Instance.stationData;
            // MachineData machineData = new MachineData();
            // machineData.machineName = "Gimo";
            // machineData.hitPoint = 10;
            // machineData.propellantPoint = 10;
            // machineData.atk= 10;
            // machineData.def = 10;
            // machineData.spd = 10;
            PilotData pilotData = new PilotData();
            pilotData.pilotName = "Ally2";
            pilotData.shootability = 10;
            pilotData.slashability = 10;
            pilotData.acceleration = 10;
            pilotData.luck = 10;
            pilotData.searchCapacity = 10;
            pilotData.aiMode = Enums.AIMode.Follow;
            UnitData unitData = new UnitData();
            unitData.isCpu = true;
            unitData.isManual = false; // オプションに依存予定
            unitData.color = new Color(0.5f, 0.5f, 0.75f, 1f);
            unitData.machineKey = "Gimo";
            unitData.mainWeaponKey = "BeamGun";
            unitData.handWeaponKey = "BeamKnife";
            unitData.shieldKey = "Shield";
            // unitData.machineData = machineData;
            unitData.pilotData = pilotData;
            data.unitDatas.Add(unitData);
        }

        SceneManager.LoadScene(distination);
    }

    // メニュー切り替え
    void SwitchMenu(GameObject menuObject)
    {
        // 現メニュー
        Menu currentMenu = menuObject.GetComponent<Menu>();

        // 新メニュー生成
        List<GameObject> menuPrefabs = currentMenu.MenuPrefabs;
        GameObject menuPrefab = Common.FindObjectByName(menuPrefabs, distination);
        Instantiate(menuPrefab, menuObject.transform.parent);

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
