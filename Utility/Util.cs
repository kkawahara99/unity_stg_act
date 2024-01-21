using UnityEngine;

public class Util
{
    // 2つのVector2のなす角を返す
    public static float GetAngleByVector2(Vector2 fromVector, Vector2 toVector)
    {
        // Vector2.Angleメソッドを使用して2つのベクトルのなす角を計算
        float angle = Vector2.Angle(fromVector, toVector);

        // 2つのベクトルの外積を使用して角度の符号を調整
        float crossProduct = Vector3.Cross(fromVector, toVector).z;

        // 外積が負の場合は角度を反転
        if (crossProduct < 0)
        {
            angle = 360 - angle;
        }

        return angle;
    }

    // 2つのfloatの和を返す
    public static float GetSumByFloat(float a, float b)
    {
        return a + b;
    }

    // 2つのfloatの差を返す
    public static float GetDiffByFloat(float a, float b)
    {
        return Mathf.Abs(a - b);
    }

    // -1か1を返す
    public static int GetRandomSign()
    {
        return Random.Range(0, 2) * 2 - 1;
    }

    // 相手側のタグを返す
    public static string GetOpponentTag(string tag)
    {
        return tag == TagConst.BLUE ? TagConst.RED : TagConst.BLUE;
    }
}
