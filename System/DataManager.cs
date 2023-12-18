using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    public Station station;
    public int currentStageNo; // 現在のステージNo
    public int coinCount; // 所持コイン数
    public Elements elements; // 所持エレメント

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