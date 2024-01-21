using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/Create MasterData")]
public class MasterData : ScriptableObject
{
    public GameObject UnitMaster; // ユニットマスタ
    public List<PrefabMapping> MachineMaster; // マシンマスタ
    public GameObject PilotMaster; // パイロットマスタ
    public List<PrefabMapping> MainWeaponMaster; // メイン武器マスタ
    public List<PrefabMapping> HandWeaponMaster; // サブ武器マスタ
    public List<PrefabMapping> ShieldMaster; // シールド武器マスタ

    public GameObject ExplosionPrefab; // 爆風プレハブ

    private static MasterData instance;

    public static MasterData Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<MasterData>("MasterData");
            }
            return instance;
        }
    }
}

[System.Serializable]
public class PrefabMapping
{
    public Enums.WeaponKey key;
    public GameObject prefab;
}