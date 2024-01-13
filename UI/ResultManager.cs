using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    [SerializeField] GameObject resultItemPrefab; // アイテム結果パネル
    [SerializeField] GameObject resultExpPrefab; // EXP結果パネル
    [SerializeField] GameObject pilotSlotPrefab; // Pilotスロットプレハブ

    private Controller controller; // コントローラ
    private bool isSubmit; // 決定ボタン押下フラグ
    private bool isDoneItem; // アイテム結果画面完了
    private bool isDoneExp; // EXP結果画面完了
    private GameObject resultObject; // 結果画面のオブジェクト

    const float FADE_DURATION = 0.25f;
    const float WAIT_TIME = 1f;
    const float COUNTING_WAIT_TIME = 0.01f;

    void Start()
    {
        // 必要な他コンポーネント取得
        controller = GameObject.Find("EventSystem").GetComponent<Controller>();

        // タイトルを表示
        SetTitle("ITEM RESULT");

        // アイテム表示数を更新
        SetResult(true, resultItemPrefab);
        UpdateItemAmount();

        // アイテム結果を集計
        StartCoroutine(ResultItemCoroutine());
    }

    void Update()
    {
        OnShoot();
    }

    // 決定ボタン
    public void OnShoot()
    {
        // ボタン押下開始時以外はreturn
        if (controller.ShootPhase != InputActionPhase.Started) return;

        controller.SetShootPhase(InputActionPhase.Performed);

        // 効果音（鳴らさない）
        SoundManager.Instance.PlaySE(SESoundData.SE.Submit);

        // 次の結果へ
        if (isDoneItem)
        {
            isDoneItem = false;

            // 結果画面消す
            SetResult(false, null);

            // タイトルを表示
            SetTitle("EXP RESULT");

            // EXP結果表示
            SetResult(true, resultExpPrefab);

            // EXP結果を集計
            StartCoroutine(ResultExpCoroutine());
        }
        if (isDoneExp)
        {
            // 画面遷移
            SceneManager.LoadScene("StrategyScene");
        }
    }

    void SetResult(bool isSetting, GameObject obj)
    {
        if (isSetting)
        {
            // 表示する
            resultObject = Instantiate(obj, transform);
        }
        else
        {
            // 表示消す
            Destroy(resultObject);
        }
    }

    void SetTitle(string text)
    {
        GameObject.Find("Title").GetComponent<Text>().text = text;
    }

    // アイテム表示数を更新
    void UpdateItemAmount()
    {
        foreach (Transform child in resultObject.transform)
        {
            // 子オブジェクト名からアイテム所持数、獲得数を取得
            int amount;
            int addAmount;
            switch (child.name)
            {
                case "Coin":
                    amount = DataManager.Instance.coinCount;
                    addAmount = DataManager.Instance.currentCoinCount;
                    break;
                case "RedElement":
                    amount = DataManager.Instance.elements.redCount;
                    addAmount = DataManager.Instance.currentElements.redCount;
                    break;
                case "BlueElement":
                    amount = DataManager.Instance.elements.blueCount;
                    addAmount = DataManager.Instance.currentElements.blueCount;
                    break;
                case "GreenElement":
                    amount = DataManager.Instance.elements.greenCount;
                    addAmount = DataManager.Instance.currentElements.greenCount;
                    break;
                case "YellowElement":
                    amount = DataManager.Instance.elements.yellowCount;
                    addAmount = DataManager.Instance.currentElements.yellowCount;
                    break;
                default:
                    amount = 0;
                    addAmount = 0;
                    break;
            }

            // 表示数を更新
            Text tAmount = child.Find("CurrentAmount").GetComponent<Text>();
            tAmount.text = amount.ToString();
            Text tAddAmount = child.Find("GetAmount").GetComponent<Text>();
            tAddAmount.text = addAmount.ToString();

            // 獲得アイテム数が0になったら非表示
            if (addAmount == 0)
            {
                child.Find("GetAmount").gameObject.SetActive(false);
            }
        }
    }

    // アイテム獲得数を所持数に徐々に加算
    bool AddAmount()
    {
        List<bool> isZeros = new List<bool>();

        foreach (Transform child in resultObject.transform)
        {
            // 子オブジェクト名からアイテム所持数加算、獲得数減算
            switch (child.name)
            {
                case "Coin":
                    // 獲得数が0になったらフラグ更新
                    if (DataManager.Instance.currentCoinCount == 0)
                    {
                        isZeros.Add(true);
                        continue;
                    }
                    DataManager.Instance.currentCoinCount--;
                    if (DataManager.Instance.coinCount < 9999)
                        DataManager.Instance.coinCount++;
                    break;
                case "RedElement":
                    if (DataManager.Instance.currentElements.redCount == 0)
                    {
                        isZeros.Add(true);
                        continue;
                    }
                    DataManager.Instance.currentElements.redCount--;
                    if (DataManager.Instance.elements.redCount < 9999)
                        DataManager.Instance.elements.redCount++;
                    break;
                case "BlueElement":
                    if (DataManager.Instance.currentElements.blueCount == 0)
                    {
                        isZeros.Add(true);
                        continue;
                    }
                    DataManager.Instance.currentElements.blueCount--;
                    if (DataManager.Instance.elements.blueCount < 9999)
                        DataManager.Instance.elements.blueCount++;
                    break;
                case "GreenElement":
                    if (DataManager.Instance.currentElements.greenCount == 0)
                    {
                        isZeros.Add(true);
                        continue;
                    }
                    DataManager.Instance.currentElements.greenCount--;
                    if (DataManager.Instance.elements.greenCount < 9999)
                        DataManager.Instance.elements.greenCount++;
                    break;
                case "YellowElement":
                    if (DataManager.Instance.currentElements.yellowCount == 0)
                    {
                        isZeros.Add(true);
                        continue;
                    }
                    DataManager.Instance.currentElements.yellowCount--;
                    if (DataManager.Instance.elements.yellowCount < 9999)
                        DataManager.Instance.elements.yellowCount++;
                    break;
                default:
                    break;
            }
            isZeros.Add(false);
        }

        // 一つでも獲得数1以上のアイテムがあったらfalseを返す
        foreach (bool isZero in isZeros)
        {
            if (!isZero) return false;
        }
        return true;
    }

    void UpdateExp(GameObject pilotSlot, UnitData unitData)
    {
        pilotSlot.transform.Find("Name").GetComponent<Text>().text = unitData.pilotData.pilotName;
        pilotSlot.transform.Find("Lv").GetChild(0).GetComponent<Text>().text = unitData.pilotData.level.ToString();
        pilotSlot.transform.Find("Plus").GetChild(0).GetComponent<Text>().text = unitData.pilotData.earnedExp.ToString();
        Slider slider = pilotSlot.transform.Find("ExpBar").GetComponent<Slider>();
        
        int nextExp = Common.Instance.GetNextExp(unitData.pilotData.level);
        int currentExpToNext = unitData.pilotData.totalExp - Common.Instance.GetExpCurrentLevel(unitData.pilotData.level);
        float fillAmount = (float)currentExpToNext / nextExp;
        slider.value = fillAmount;

        // 獲得経験値が0になったら非表示
        if (unitData.pilotData.earnedExp == 0)
        {
            pilotSlot.transform.Find("Plus").gameObject.SetActive(false);
        }
    }

    // EXP獲得数を徐々に加算
    bool AddExp(List<UnitData> unitDatas)
    {
        List<bool> isZeros = new List<bool>();

        foreach (UnitData unitData in unitDatas)
        {
            // 子オブジェクト名から合計EXP加算、獲得EXP減算
            // 獲得数が0になったらフラグ更新
            if (unitData.pilotData.earnedExp == 0)
            {
                isZeros.Add(true);
                continue;
            }
            unitData.pilotData.earnedExp--;
            unitData.pilotData.totalExp++;
            int nextLevel = unitData.pilotData.level + 1;
            if (unitData.pilotData.totalExp >= Common.Instance.GetExpCurrentLevel(nextLevel))
            {
                // 経験値が次のレベルに達していたらレベルアップ
                if (unitData.pilotData.level < 99)
                {
                    unitData.pilotData.level++;
                    // 効果音
                    SoundManager.Instance.PlaySE(SESoundData.SE.Levelup1);
                }
                    
            }

            isZeros.Add(false);
        }

        // 一つでも獲得EXP1以上があったらfalseを返す
        foreach (bool isZero in isZeros)
        {
            if (!isZero) return false;
        }
        return true;
    }

    IEnumerator ResultItemCoroutine()
    {
        // 各アイテムの獲得数を順次表示していく
        foreach (Transform child in resultObject.transform)
        {
            float startTime = Time.time;
            Color amountColor = child.Find("GetAmount").GetComponent<Text>().color;

            while (Time.time < startTime + FADE_DURATION)
            {
                float t = (Time.time - startTime) / FADE_DURATION;
                Color newColor = new Color(amountColor.r, amountColor.g, amountColor.b, Mathf.Lerp(0f, 1f, t));
                child.Find("GetAmount").GetComponent<Text>().color = newColor;
                child.Find("GetAmount").Find("Plus").GetComponent<Text>().color = newColor;
                yield return null;  // 1フレーム待つ
            }
        }

        // すこし待つ
        yield return new WaitForSeconds(WAIT_TIME);

        // 各アイテムの獲得数を所持数に追加していく（所持数は1ずつ増えていき、獲得数は減っていく演出）
        bool isFinish = false;
        while (!isFinish)
        {
            isFinish = AddAmount();

            // 表示更新
            UpdateItemAmount();

            // 効果音
            SoundManager.Instance.PlaySE(SESoundData.SE.Point1);

            // ちょっと待つ
            yield return new WaitForSeconds(COUNTING_WAIT_TIME);
        }

        // コルーチン終了フラグオン
        isDoneItem = true;
    }

    IEnumerator ResultExpCoroutine()
    {
        List<GameObject> pilotSlots = new List<GameObject>();
        // 編成ユニット数分スロットを表示する
        List<UnitData> unitDatas = DataManager.Instance.stationData.unitDatas;
        foreach (UnitData unitData in unitDatas)
        {
            GameObject pilotSlot = Instantiate(pilotSlotPrefab, resultObject.transform);
            UpdateExp(pilotSlot, unitData);

            pilotSlots.Add(pilotSlot);
        }

        // 獲得EXPを順次表示していく
        foreach (Transform child in resultObject.transform)
        {
            float startTime = Time.time;
            Color amountColor = child.Find("Plus").GetChild(0).GetComponent<Text>().color;

            while (Time.time < startTime + FADE_DURATION)
            {
                float t = (Time.time - startTime) / FADE_DURATION;
                Color newColor = new Color(amountColor.r, amountColor.g, amountColor.b, Mathf.Lerp(0f, 1f, t));
                child.Find("Plus").GetChild(0).GetComponent<Text>().color = newColor;
                child.Find("Plus").GetComponent<Text>().color = newColor;
                yield return null;  // 1フレーム待つ
            }
        }

        // すこし待つ
        yield return new WaitForSeconds(WAIT_TIME);

        // EXP徐々に増やしていく
        bool isFinish = false;
        while (!isFinish)
        {
            isFinish = AddExp(unitDatas);

            // 表示更新
            for (int i = 0; i < pilotSlots.Count; i++)
            {
                UpdateExp(pilotSlots[i], unitDatas[i]);
            }

            // 効果音
            SoundManager.Instance.PlaySE(SESoundData.SE.Point1);

            // ちょっと待つ
            yield return new WaitForSeconds(COUNTING_WAIT_TIME);
        }

        // コルーチン終了フラグオン
        isDoneExp = true;
    }
}
