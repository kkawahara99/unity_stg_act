using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MonoCommon: MonoBehaviour
{
    const float BASE_EXP = 10f;
    const float EXP_RATIO = 1.1f;

    public static MonoCommon Instance { get; private set; }

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

    // 爆風生成
    public void GenerateExplosion(Vector2 position, float size)
    {
        GameObject explosionObject = Instantiate(MasterData.Instance.ExplosionPrefab, position, Quaternion.identity);
        explosionObject.transform.localScale = new Vector3(size, size, size);
    }

    // アイテム生成
    public void GenerateItem(List<ItemBean> dropItem, Transform generatorTransform)
    {
        ItemBean itemBean = null;
        int totalFrequency = 0;

        // 出現率の母数を取得
        foreach (ItemBean item in dropItem)
        {
            totalFrequency += item.Frequency;
        }

        // ランダム値生成
        int randomValue = UnityEngine.Random.Range(1, totalFrequency);

        // 出現率に基づきリストを選択
        foreach (ItemBean item in dropItem)
        {
            if (randomValue <= item.Frequency)
            {
                itemBean = item;
                break;
            }

            randomValue -= item.Frequency;
        }

        // ドロップアイテムなしの時はreturn
        if (itemBean == null || itemBean.ItemPrefab == null) return;

        Instantiate(itemBean.ItemPrefab, generatorTransform.position, Quaternion.identity);
    }

    // ミッション成功
    public void Succeeded()
    {
        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        // ミッション失敗時は成功にならない
        if (gameManager.IsFailed) return;

        gameManager.SetIsSucceed(true);
        StartCoroutine(SucceedCoroutine());
    }

    IEnumerator SucceedCoroutine()
    {
        RectTransform rectTransformL = GameObject.Find("SucceedL").GetComponent<RectTransform>();
        RectTransform rectTransformR = GameObject.Find("SucceedR").GetComponent<RectTransform>();

        // テキスト有効化
        rectTransformL.gameObject.GetComponent<Text>().enabled = true;
        rectTransformR.gameObject.GetComponent<Text>().enabled = true;

        // 移動方向を設定（例：右方向）
        Vector3 moveDirectionL = new Vector3(1f, 0f, 0f);
        Vector3 moveDirectionR = new Vector3(-1f, 0f, 0f);

        // 移動量を計算
        float moveAmount = 700f * Time.deltaTime;

        while (rectTransformL.position.x < 433f)
        {
            // テキストを移動
            rectTransformL.Translate(moveDirectionL * moveAmount);
            rectTransformR.Translate(moveDirectionR * moveAmount);
            yield return null;
        }
        float posisionX = GameObject.Find("Canvas").GetComponent<RectTransform>().position.x;
        rectTransformL.position = new Vector2(posisionX, rectTransformL.position.y);
        rectTransformR.position = new Vector2(posisionX, rectTransformR.position.y);

        SpriteRenderer foreground = GameObject.Find("Foreground").GetComponent<SpriteRenderer>();

        float currentAlpha = 0f;
        while (currentAlpha < 1f)
        {
            // 現在の透明度を取得
            currentAlpha = foreground.color.a;

            // 新しい透明度を計算
            float newAlpha = Mathf.MoveTowards(currentAlpha, 1f, 0.25f * Time.deltaTime);

            // 透明度を更新
            Color color = foreground.color;
            color.a = newAlpha;
            foreground.color = color;
            yield return null;
        }

        // 画面遷移
        if (DataManager.Instance.currentStageNo == 7)
        {
            SceneManager.LoadScene("TitleScene");
        }
        else
        {
            SceneManager.LoadScene("ResultScene");
        }
    }

    // ミッション失敗
    public void Failed()
    {
        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        // ミッション成功時は失敗にならない
        if (gameManager.IsSucceed) return;

        gameManager.SetIsFailed(true);
        StartCoroutine(FailedCoroutine());
    }

    IEnumerator FailedCoroutine()
    {
        RectTransform rectTransformL = GameObject.Find("FailedL").GetComponent<RectTransform>();
        RectTransform rectTransformR = GameObject.Find("FailedR").GetComponent<RectTransform>();

        // テキスト有効化
        rectTransformL.gameObject.GetComponent<Text>().enabled = true;
        rectTransformR.gameObject.GetComponent<Text>().enabled = true;

        // 移動方向を設定（例：右方向）
        Vector3 moveDirectionL = new Vector3(1f, 0f, 0f);
        Vector3 moveDirectionR = new Vector3(-1f, 0f, 0f);

        // 移動量を計算
        float moveAmount = 700f * Time.deltaTime;

        while (rectTransformL.position.x < 433f)
        {
            // テキストを移動
            rectTransformL.Translate(moveDirectionL * moveAmount);
            rectTransformR.Translate(moveDirectionR * moveAmount);
            yield return null;
        }
        float posisionX = GameObject.Find("Canvas").GetComponent<RectTransform>().position.x;
        rectTransformL.position = new Vector2(posisionX, rectTransformL.position.y);
        rectTransformR.position = new Vector2(posisionX, rectTransformR.position.y);

        SpriteRenderer foreground = GameObject.Find("Foreground").GetComponent<SpriteRenderer>();

        float currentAlpha = 0f;
        while (currentAlpha < 1f)
        {
            // 現在の透明度を取得
            currentAlpha = foreground.color.a;

            // 新しい透明度を計算
            float newAlpha = Mathf.MoveTowards(currentAlpha, 1f, 0.25f * Time.deltaTime);

            // 透明度を更新
            Color color = foreground.color;
            color.a = newAlpha;
            foreground.color = color;
            yield return null;
        }

        // 画面遷移
        SceneManager.LoadScene("TitleScene");
    }
}
