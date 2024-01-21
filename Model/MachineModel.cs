using UnityEngine;

[System.Serializable]
public class MachineModel
{
    /* マシンパラメータ */
    private string machineName; // マシン名
    private int hitPoint; // 耐久力（HP）
    private int propellantPoint; // 推進力（PP）
    private int atk; // 火力（Act）
    private int def; // 装甲（Def）
    private int spd; // 機動性（Speed）

    /* メタパラメータ */
    private GameObject mainWeaponPrefab; // メイン装備
    private GameObject handWeaponPrefab; // 近距離装備
    private GameObject shieldPrefab; // 盾装備

    private bool isRight; // 右向きかどうか
    private bool isBoosting = false; // ブースト中かどうか
    private bool isDown; // ダウン中かどうか
    private bool isAction; // アクション中かどうか
    private bool isDashing; // ダッシュ中かどうか
    private bool isDefence; // 防御中かどうか
    private bool isDead; // 死んでるかどうか
    private int currentHP; // 現在のHP
    private float currentPP; // 現在の推進力
    private Vector2 currentVelocity;  // 現在の速度
    private Shield shield; // シールド情報
    private PilotController pilot; // パイロット情報
    private Unit opponentUnit; // ダメージ食らわされた相手ユニット

    public string MachineName
    {
        get { return machineName; }
        set { machineName = value; }
    }
    public int HitPoint
    {
        get { return hitPoint; }
        set { hitPoint = value; }
    }
    public int PropellantPoint
    {
        get { return propellantPoint; }
        set { propellantPoint = value; }
    }
    public int Atk
    {
        get { return atk; }
        set { atk = value; }
    }
    public int Def
    {
        get { return def; }
        set { def = value; }
    }
    public int Spd
    {
        get { return spd; }
        set { spd = value; }
    }
    public GameObject MainWeaponPrefab
    {
        get { return mainWeaponPrefab; }
        set { mainWeaponPrefab = value; }
    }
    public GameObject HandWeaponPrefab
    {
        get { return handWeaponPrefab; }
        set { handWeaponPrefab = value; }
    }
    public GameObject ShieldPrefab
    {
        get { return shieldPrefab; }
        set { shieldPrefab = value; }
    }
    public bool IsRight
    {
        get { return isRight; }
        set { isRight = value; }
    }
    public bool IsBoosting
    {
        get { return isBoosting; }
        set { isBoosting = value; }
    }
    public bool IsDown
    {
        get { return isDown; }
        set { isDown = value; }
    }
    public bool IsAction
    {
        get { return isAction; }
        set { isAction = value; }
    }
    public bool IsDashing
    {
        get { return isDashing; }
        set { isDashing = value; }
    }
    public bool IsDefence
    {
        get { return isDefence; }
        set { isDefence = value; }
    }
    public bool IsDead
    {
        get { return isDead; }
        set { isDead = value; }
    }
    public int CurrentHP
    {
        get { return currentHP; }
        set { currentHP = value; }
    }
    public float CurrentPP
    {
        get { return currentPP; }
        set { currentPP = value; }
    }
    public Vector2 CurrentVelocity
    {
        get { return currentVelocity; }
        set { currentVelocity = value; }
    }
    public Shield Shield
    {
        get { return shield; }
        set { shield = value; }
    }
    public PilotController Pilot
    {
        get { return pilot; }
        set { pilot = value; }
    }
    public Unit OpponentUnit
    {
        get { return opponentUnit; }
        set { opponentUnit = value; }
    }

    // パラメータをセット
    public void SetParameter(string machineName, int hitPoint, int propellantPoint, int atk, int def, int spd)
    {
        this.machineName = machineName;
        this.hitPoint = hitPoint;
        this.propellantPoint = propellantPoint;
        this.atk = atk;
        this.def = def;
        this.spd = spd;
    }
}