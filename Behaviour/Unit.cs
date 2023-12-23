using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private GameObject machinePrefab; // マシン
    public GameObject MachinePrefab { get => machinePrefab; }
    [SerializeField] private GameObject pilotPrefab; // パイロット
    public GameObject PilotPrefab { get => pilotPrefab; }
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

    // マシンをセット
    public void SetMachinePrefab(GameObject machinePrefab)
    {
        this.machinePrefab = machinePrefab;
    }

    // パイロットをセット
    public void SetPilotPrefab(GameObject pilotPrefab)
    {
        this.pilotPrefab = pilotPrefab;
    }

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
        GameObject machineObject = Instantiate(machinePrefab, transform.position, Quaternion.identity, transform);
        machineObject.name = "Machine";
    }

    // パイロットの展開
    public void DeployPilot()
    {
        GameObject pilotObject = Instantiate(pilotPrefab, transform.position, Quaternion.identity, transform);
        pilotObject.name = "Pilot";
    }

    // 撃破数インクリメント
    public void IncrementKillCount()
    {
        this.killCount++;
        Debug.Log(gameObject.name + "：" + killCount);
    }
}
