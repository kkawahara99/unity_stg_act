using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    public ScenarioManager.ScenarioID currentScenarioID; // 現在のシナリオ
    public int currentStageNo; // 現在のステージNo
    public int coinCount; // 所持コイン数
    public int currentCoinCount; // 所持コイン数（ステージ内）
    public Elements elements; // 所持エレメント
    public Elements currentElements; // 所持エレメント（ステージ内）
    public StationData stationData; // ステーションデータ（味方データ）


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

// [CreateAssetMenu(menuName = "MyScriptable/Create MasterData")]
// public class MasterData : ScriptableObject
// {
//     public GameObject UnitMaster; // ユニットマスタ
//     public List<PrefabMapping> MachineMaster; // マシンマスタ
//     public GameObject PilotMaster; // パイロットマスタ
//     public List<PrefabMapping> MainWeaponMaster; // メイン武器マスタ
//     public List<PrefabMapping> HandWeaponMaster; // サブ武器マスタ
//     public List<PrefabMapping> ShieldMaster; // シールド武器マスタ
// }

[System.Serializable]
public class StationData
{
    public string stationName;
    public int hitPoint;
    public int atk;
    public int def;
    public int luck;
    public List<UnitData> unitDatas;
}

[System.Serializable]
public class UnitData
{
    public bool isCpu;
    public bool isManual;
    public Color color;
    public Enums.WeaponKey machineKey;
    public Enums.WeaponKey mainWeaponKey;
    public Enums.WeaponKey handWeaponKey;
    public Enums.WeaponKey shieldKey;
    // public MachineData machineData;
    public PilotData pilotData;
}

// [System.Serializable]
// public class MachineData
// {
//     public string machineName;
//     public int hitPoint;
//     public int propellantPoint;
//     public int atk;
//     public int def;
//     public int spd;
// }

[System.Serializable]
public class PilotData
{
    public string pilotName;
    public int shootability;
    public int slashability;
    public int acceleration;
    public int luck;
    public int searchCapacity;
    public Enums.AIMode aiMode;
    public int level;
    public int totalExp;
    public int earnedExp;
    public int killCount;
}