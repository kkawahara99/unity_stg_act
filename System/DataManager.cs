using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    public ScenarioManager.ScenarioID currentScenarioID; // 現在のシナリオ
    public int currentStageNo; // 現在のステージNo
    public int coinCount; // 所持コイン数
    public Elements elements; // 所持エレメント
    public StationData stationData; // ステーションデータ（味方データ）

    public GameObject UnitMaster; // ユニットマスタ
    public List<GameObject> MachineMaster; // マシンマスタ
    public GameObject PilotMaster; // パイロットマスタ

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

[System.Serializable]
public class StationData
{
    public int hitPoint;
    public int atc;
    public int def;
    public int luck;
    public List<UnitData> unitDatas;
}

[System.Serializable]
public class UnitData
{
    public bool isCpu;
    public bool isManual;
    public int machineNo;
    public MachineData machineData;
    public PilotData pilotData;
}

[System.Serializable]
public class MachineData
{
    public string machineName;
    public int hitPoint;
    public int propellantPoint;
    public int atc;
    public int def;
    public int spd;
}

[System.Serializable]
public class PilotData
{
    public string pilotName;
    public int shootability;
    public int slashability;
    public int acceleration;
    public int luck;
    public int searchCapacity;
    public Pilot.AIMode aiMode;
}