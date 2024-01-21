using System;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private Enums.WeaponKey machineKey; // マシンキー
    public Enums.WeaponKey MachineKey { get => machineKey; }
    [SerializeField] private Enums.WeaponKey mainWeaponKey; // メイン武器キー
    public Enums.WeaponKey MainWeaponKey { get => mainWeaponKey; }
    [SerializeField] private Enums.WeaponKey handWeaponKey; // サブ武器キー
    public Enums.WeaponKey HandWeaponKey { get => handWeaponKey; }
    [SerializeField] private Enums.WeaponKey shieldKey; // シールドキー
    public Enums.WeaponKey ShieldKey { get => shieldKey; }
    [SerializeField] private bool isCpu = false; // CPUかどうか
    public bool IsCpu { get => isCpu; }
    [SerializeField] private bool isManual = false; // マニュアル操作かどうか
    public bool IsManual { get => isManual; }
    [SerializeField] private bool isRight = true; // 右向きかどうか
    public bool IsRight { get => isRight; }
    [SerializeField] private Color color; // カラー
    public Color Color { get => color; }
    [SerializeField] private List<ItemBean> dropItem = new List<ItemBean>(); // ドロップアイテム
    public List<ItemBean> DropItem { get => dropItem; }
    private int killCount; // 撃破数
    public int KillCount { get => killCount; }
    public int unitNo;

    private UnitData unitData;
    public UnitData UnitData { get => unitData; }

    // カラーをセット
    public void SetColor(Color color)
    {
        this.color = color;
    }

    // 位置をセット
    public void SetPosition(Vector2 position)
    {
        transform.position = position;
    }

    // マシンの展開
    public void DeployMachine()
    {
        GameObject machinePrefab = Common.GetPrefabMapping(machineKey, MasterData.Instance.MachineMaster);
        GameObject machineObject = Instantiate(machinePrefab, transform.position, Quaternion.identity, transform);
        machineObject.name = "Machine";
        machineObject.GetComponent<MachineController>().InitializeData();
    }

    // パイロットの展開
    public void DeployPilot()
    {
        GameObject pilotPrefab = MasterData.Instance.PilotMaster;
        GameObject pilotObject = Instantiate(pilotPrefab, transform.position, Quaternion.identity, transform);
        pilotObject.name = "Pilot";
        pilotObject.GetComponent<PilotController>().InitializeData();
    }

    // 撃破数インクリメント
    public void IncrementKillCount()
    {
        if (unitData == null) return;
        unitData.pilotData.killCount++;
    }

    // 獲得経験値増加
    public void IncreaseEarnedExp(int grantExp)
    {
        if (unitData == null) return;
        unitData.pilotData.earnedExp += grantExp;
    }

    // データ初期化
    public void InitializeData()
    {
        bool isAlly = gameObject.tag == TagConst.BLUE;
        if (isAlly)
        {
            unitData = GameObject.Find("Station").GetComponent<Station>().StationData.unitDatas[unitNo];
        }
        else
        {
            unitData = GameObject.Find("StationEnemy").GetComponent<Station>().StationData.unitDatas[unitNo];
        }
        this.isRight = isAlly ? true : false;
        this.isCpu = unitData.isCpu;
        this.isManual = unitData.isManual;
        this.color = unitData.color;
        this.machineKey = unitData.machineKey;
        this.mainWeaponKey = unitData.mainWeaponKey;
        this.handWeaponKey = unitData.handWeaponKey;
        this.shieldKey = unitData.shieldKey;
    }
}
