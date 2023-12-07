using System.Collections;
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

    void Start()
    {
    }

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
}
