using UnityEngine;

[System.Serializable]
public class PilotModel
{
    // パイロットパラメータ
    private string pilotName;
    private int shootability;
    private int slashability;
    private int acceleration;
    private int luck;
    private int searchCapacity;
    private Enums.AIMode aiMode;
    private int level;
    private int totalExp;
    private int earnedExp;
    private int killCount;

    // メタパラメータ
    private Vector2 currentDirection;
    private Vector2 lastDirection;
    private float lastKeyPressTime;
    private float cpuPhaseTime; // CPUの行動フェーズ切替時間
    private bool isDoubleTap;
    private bool isDashing; // ダッシュ中かどうか
    private bool isAttack; // CPUの行動が攻撃
    private Node currentNode; // 現在のノード
    private Node nextNode; // 次のノード

    public string PilotName
    {
        get { return pilotName; }
        set { pilotName = value; }
    }
    public int Shootability
    {
        get { return shootability; }
        set { shootability = value; }
    }
    public int Slashability
    {
        get { return slashability; }
        set { slashability = value; }
    }
    public int Acceleration
    {
        get { return acceleration; }
        set { acceleration = value; }
    }
    public int Luck
    {
        get { return luck; }
        set { luck = value; }
    }
    public int SearchCapacity
    {
        get { return searchCapacity; }
        set { searchCapacity = value; }
    }
    public Enums.AIMode AiMode
    {
        get { return aiMode; }
        set { aiMode = value; }
    }
    public int Level
    {
        get { return level; }
        set { level = value; }
    }
    public int TotalExp
    {
        get { return totalExp; }
        set { totalExp = value; }
    }
    public int EarnedExp
    {
        get { return earnedExp; }
        set { earnedExp = value; }
    }
    public int KillCount
    {
        get { return killCount; }
        set { killCount = value; }
    }
    public Vector2 CurrentDirection
    {
        get { return currentDirection; }
        set { currentDirection = value; }
    }
    public Vector2 LastDirection
    {
        get { return lastDirection; }
        set { lastDirection = value; }
    }
    public float LastKeyPressTime
    {
        get { return lastKeyPressTime; }
        set { lastKeyPressTime = value; }
    }
    public float CpuPhaseTime
    {
        get { return cpuPhaseTime; }
        set { cpuPhaseTime = value; }
    }
    public bool IsDoubleTap
    {
        get { return isDoubleTap; }
        set { isDoubleTap = value; }
    }
    public bool IsDashing
    {
        get { return isDashing; }
        set { isDashing = value; }
    }
    public bool IsAttack
    {
        get { return isAttack; }
        set { isAttack = value; }
    }
    public Node CurrentNode
    {
        get { return currentNode; }
        set { currentNode = value; }
    }
    public Node NextNode
    {
        get { return nextNode; }
        set { nextNode = value; }
    }
}
