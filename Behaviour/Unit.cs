using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private int machineNo; // マシン
    public int MachineNo { get => machineNo; }
    // [SerializeField] private GameObject pilotPrefab; // パイロット
    // public GameObject PilotPrefab { get => pilotPrefab; }
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
        GameObject machinePrefab = DataManager.Instance.MachineMaster[machineNo];
        GameObject machineObject = Instantiate(machinePrefab, transform.position, Quaternion.identity, transform);
        machineObject.name = "Machine";
        machineObject.GetComponent<Machine>().InitializeData();
    }

    // パイロットの展開
    public void DeployPilot()
    {
        GameObject pilotPrefab = DataManager.Instance.PilotMaster;
        GameObject pilotObject = Instantiate(pilotPrefab, transform.position, Quaternion.identity, transform);
        pilotObject.name = "Pilot";
        pilotObject.GetComponent<Pilot>().InitializeData();
    }

    // 撃破数インクリメント
    public void IncrementKillCount()
    {
        this.killCount++;
        Debug.Log(gameObject.name + "：" + killCount);
    }

    // データ初期化
    public void InitializeData()
    {
        if (gameObject.tag == "Blue")
        {
            unitData = GameObject.Find("Station").GetComponent<Station>().StationData.unitDatas[unitNo];
        }
        else
        {
            unitData = GameObject.Find("StationEnemy").GetComponent<Station>().StationData.unitDatas[unitNo];
        }
        this.isCpu = unitData.isCpu;
        this.isManual = unitData.isManual;
        this.machineNo = unitData.machineNo;
    }
}
