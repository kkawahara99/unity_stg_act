using UnityEngine;
using UnityEngine.UI;

public class ChargeUI : MonoBehaviour
{
    const float HIDE_UI_TIME = 3.0f; // ゲージを隠すまでの時間

    private Slider slider;
    private float deltaTime; // ゲージが変わらなかった時間

    void Start()
    {
        slider = gameObject.GetComponent<Slider>();
    }

    // ゲージの更新
    public void UpdateChargeUI(int currentValue, int MaxValue)
    {
        // 残りの割合
        float fillAmount = (float)currentValue / MaxValue;

        // stock の割合をゲージに反映
        ShowUI(true);
        slider.value = fillAmount;

        // ゲージを隠すまでの時間をリセット
        deltaTime = 0f;
    }

    // ゲージを隠すまでの時間を加算
    public void HideTime(float time)
    {
        deltaTime = Mathf.Min(deltaTime + time, HIDE_UI_TIME);
        if (deltaTime == HIDE_UI_TIME)
        {
            // 時間になったらUIを隠す
            ShowUI(false);
        }
    }

    // ゲージ表示・非表示
    void ShowUI(bool isShow)
    {
        slider.transform.Find("Background").gameObject.SetActive(isShow);
        slider.transform.Find("FillArea").gameObject.SetActive(isShow);
    }
}
