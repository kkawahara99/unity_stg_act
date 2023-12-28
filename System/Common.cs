using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Common : MonoBehaviour
{
    [SerializeField] GameObject explosionPrefab; // 爆風プレハブ

    public static Common Instance { get; private set; }

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

    // 色を変更
    public void SetColors(Color color, Transform transform)
    {
        SpriteRenderer[] spriteRenderers = transform.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            List<string> noColorObjects = new List<string> {"MainWeapon", "HandWeapon", "Shiled"};
            List<string> colorObjects = new List<string> {"Head2", "Booster"};
            string objectName = spriteRenderer.gameObject.name;
            if (noColorObjects.Contains(objectName))
            {
                // 装備は色塗りしない
                continue;
            }

            if (colorObjects.Contains(objectName))
            {
                // メガネ、ブースターのみ色塗りする（仮）
                spriteRenderer.color = color;
            }
        }
    }

    // 移動位置を制限
    public void RestrictMovePosition(Rigidbody2D rb, float maxX, float minX, float maxY, float minY)
    {
        // 新しい位置を計算
        Vector2 newPosition = rb.position + rb.velocity * Time.deltaTime;

        // X座標を制約
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);

        // Y座標を制約
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        // 新しい位置を適用
        rb.MovePosition(newPosition);
    }

    // 衝突時のベクトルを計算
    public Vector2 CalculateVelocity(ContactPoint2D contact, Vector2 currentVelocity, int spd)
    {
        Vector2 result;
        GameObject collidedObject = contact.collider.gameObject;

        if (collidedObject.name == "ToBe")
        {
            // ToBe: アイテムなどとの衝突では跳ね返らない
            result =  currentVelocity;
        }
        else
        {
            // 衝突対象のベクトル取得
            Vector2 collisionNormal = contact.normal;

            // 衝突対象との速度ベクトルの内積を計算
            float dotProduct = Vector2.Dot(currentVelocity, collisionNormal);

            if (dotProduct > 0f)
            {
                // 同じ方向ならベクトル加算
                result = currentVelocity + Calculator.Instance.calculateBounceVelocity(collisionNormal);
            }
            else
            {
                // 逆方向なら反射
                result = Calculator.Instance.calculateBounceVelocity(Vector2.Reflect(currentVelocity, collisionNormal));
            }
        }

        // 跳ね返りベクトルは一定値以上にならないようにする
        Vector2 maxVelocity = Calculator.Instance.calculateBounceVelocity(
            Calculator.Instance.calculateTargetVelocity(
                Vector2.one.normalized, spd, false
            )
        );
        result.x = Mathf.Clamp(result.x, -maxVelocity.x, maxVelocity.x);
        result.y = Mathf.Clamp(result.y, -maxVelocity.y, maxVelocity.y);

        return result;
    }

    // 接触時のイベント分岐
    public int DecideEvent(ContactPoint2D contact, int def, int deffenceLuck)
    {
        int damageValue = 0;

        // 衝突したオブジェクトから情報を取得
        Ballet collidedBallet = contact.collider.gameObject.GetComponent<Ballet>();
        Weapon collidedWeapon = contact.collider.gameObject.GetComponent<Weapon>();

        if (collidedBallet != null)
        {
            // 弾に被弾したとき
            int atc = collidedBallet.Power;
            int luck = collidedBallet.Pilot.Luck;

            // ダメージ処理
            damageValue = Calculator.Instance.CalculateDamage(atc, luck, def, deffenceLuck);
        }
        else if (collidedWeapon != null)
        {
            // 武器に被弾したとき
            int atc = collidedWeapon.Machine.Atc + collidedWeapon.Power;
            int luck = collidedWeapon.Pilot.Luck;

            // ダメージ処理
            damageValue = Calculator.Instance.CalculateDamage(atc, luck, def, deffenceLuck);
        }
        else
        {
            // その他と接触したとき
        }

        return damageValue;
    }

    // 被弾時爆風生成
    public void GenerateExplosionWhenHitted(ContactPoint2D contact)
    {
        GenerateExplosion(contact.point, 0.4f);
    }

    // 爆風生成
    public void GenerateExplosion(Vector2 position, float size)
    {
        GameObject explosionObject = Instantiate(explosionPrefab, position, Quaternion.identity);
        explosionObject.transform.localScale = new Vector3(size, size, size);
    }

    // ダウン中からの復帰
    public IEnumerator ComeBackFromDown(GameObject downObject, float comeBackTime, bool isDown)
    {
        bool isClearness = false;
        float downTime = 0f;
        float transparency;
        SpriteRenderer[] spriteRenderers = downObject.GetComponentsInChildren<SpriteRenderer>(false);

        // コライダーを一時的に無効化
        downObject.GetComponent<Collider2D>().enabled = false;

        while (isDown)
        {
            if (downTime > comeBackTime)
            {
                // ダウンタイムが復帰時間に達した場合、復帰
                isDown = false;
                if (!isClearness)
                {
                    // 透明になっていないときはそのまま処理終了
                    break;
                }
            }

            // 点滅
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                if (isClearness)
                {
                    // 透明のときは非透明化
                    transparency = 1f;
                }
                else
                {
                    // 非透明の時は透明化
                    transparency = 0f;
                }
                Color currentColor = spriteRenderer.color;
                currentColor.a = transparency;
                spriteRenderer.color = currentColor;
            }

            // 点滅フラグ反転
            isClearness = !isClearness;

            // ダウンタイムに加算
            downTime += Time.deltaTime;

            yield return null;
            yield return null;
        }

        // コライダーを有効化
        downObject.GetComponent<Collider2D>().enabled = true;
    }

    // 増やした後のHPを返す
    public int IncreaseHP(int maxHp, int currentHP, int recoveryValue)
    {
        // 現在HPを増やす
        // 最大HP以上にならないようにする
        return Mathf.Min(currentHP + recoveryValue, maxHp);
    }

    // 減らした後のHPを返す
    public int DecreaseHP(int currentHP, int damageValue)
    {
        // 現在HPを減らす
        // 0以下にならないようにする
        return Mathf.Max(currentHP - damageValue, 0);
    }

    // 減らした後のPPを返す
    public float DecreasePP(float currentPP)
    {
        // 現在HPを減らす
        // 0以下にならないようにする
        return Mathf.Max(currentPP - 10 * Time.deltaTime, 0f);
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

        Debug.Log(rectTransformL.position.x);
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
            SceneManager.LoadScene("StrategyScene");
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

    //　オブジェクトの再起的探索
    public Transform FindObjectRecursively(Transform parentTransform, string nameToFind)
    {
        Transform result = parentTransform.Find(nameToFind);

        if (result != null)
        {
            // ヒットすればそのtransformを返す
            return result;
        }

        // 再帰的に子オブジェクトを探索
        for (int i = 0; i < parentTransform.childCount; i++)
        {
            Transform child = parentTransform.GetChild(i);
            result = FindObjectRecursively(child, nameToFind);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    // オブジェクト名を指定してGameObjectを検索するメソッド
    public GameObject FindObjectByName(List<GameObject> objectList, string name)
    {
        foreach (GameObject obj in objectList)
        {
            if (obj.name == name)
            {
                return obj;
            }
        }
        // 見つからなかった場合はnullを返します
        return null;
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
        int randomValue = Random.Range(1, totalFrequency);

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
}
