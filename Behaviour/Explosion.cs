using UnityEngine;

public class Explosion : MonoBehaviour
{
    const float EXPLOSION_TIME = 0.2f;

    void Start()
    {
        // 角度をランダムにする
        transform.rotation = Quaternion.Euler(0, 0, Random.value * 360);

        // 効果音
        SoundManager.Instance.PlaySE(SESoundData.SE.Explosion1);

        // 消す
        Destroy(gameObject, EXPLOSION_TIME);
    }

}
