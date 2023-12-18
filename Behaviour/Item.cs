using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private Genre genre; // アイテムのジャンル
    [SerializeField] private bool isFull; // 回復アイテムがFullかどうか
    [SerializeField] private Elements.ElementType elementType; // エレメントの種類

    private float lifeTime; // アイテムの生存時間
    const float SOON_LIFE_TIME = 5f; // アイテムが点滅し始める時間
    const float MAX_LIFE_TIME = 7f; // アイテムが消滅する時間
    private bool isClearness; // 透明フラグ
    private bool isFlashing; // 点滅中かどうか
    
    public enum Genre
    {
        Recovery, // 回復
        Coin,     // コイン
        Element   // エレメント
    }

    void Update()
    {
        // アイテムの時間を加算
        lifeTime += Time.deltaTime;

        if (lifeTime > SOON_LIFE_TIME && !isFlashing)
        {
            // 点滅を始める
            Flashing();
        }
        else if (lifeTime > MAX_LIFE_TIME)
        {
            // 消滅
            DestroyItem();
        }
    }

    // プレイヤーが取得したとき
    void OnTriggerEnter2D(Collider2D collision)
    {
        Transform playerTransform = collision.transform.parent;
        bool isPlayer = !playerTransform.GetComponent<Unit>().IsCpu;
        if (isPlayer)
        {
            // プレイヤーのみ取得できる
            switch (genre)
            {
                case Genre.Recovery:
                    ItemRecovery(playerTransform);
                    break;
                case Genre.Coin:
                    ItemCoin();
                    break;
                case Genre.Element:
                    ItemElement();
                    break;
            }
        }
    }

    // 回復
    void ItemRecovery(Transform playerTransform)
    {
        // 対象マシンを取得
        Machine machine = playerTransform.Find("Machine").GetComponent<Machine>();

        int recoveryValue; // 回復量

        if (isFull)
        {
            // 効果音
            SoundManager.Instance.PlaySE(SESoundData.SE.Recover2);

            // 全回復
            recoveryValue = machine.HitPoint;
        }
        else
        {
            // 効果音
            SoundManager.Instance.PlaySE(SESoundData.SE.Recover1);

            // 半分回復
            recoveryValue = machine.HitPoint / 2;
        }

        // 回復させる
        machine.RecoverHP(recoveryValue);

        // アイテムを消す
        DestroyItem();
    }

    // コイン
    void ItemCoin()
    {
        // 効果音
        SoundManager.Instance.PlaySE(SESoundData.SE.Coin1);

        // 所持金を増やす
        DataManager.Instance.coinCount += 1;

        // アイテムを消す
        DestroyItem();
    }

    // エレメント 
    void ItemElement()
    {
        // 効果音
        SoundManager.Instance.PlaySE(SESoundData.SE.Energy1);

        // エレメントを増やす
        switch (elementType)
        {
            case Elements.ElementType.Red:
                DataManager.Instance.elements.redCount += 1;
                break;
            case Elements.ElementType.Blue:
                DataManager.Instance.elements.blueCount += 1;
                break;
            case Elements.ElementType.Green:
                DataManager.Instance.elements.greenCount += 1;
                break;
            case Elements.ElementType.Yellow:
                DataManager.Instance.elements.yellowCount += 1;
                break;
        }

        // アイテムを消す
        DestroyItem();
    }

    // アイテムを消す
    void DestroyItem()
    {
        Destroy(gameObject);
    }

    // 点滅
    public IEnumerator Flashing()
    {
        isFlashing = true;
        float transparency;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        while (isFlashing)
        {
            // 点滅
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

            // 点滅フラグ反転
            isClearness = !isClearness;

            yield return null;
            yield return null;
        }
    }
}
