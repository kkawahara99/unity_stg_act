using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Elements
{
    public int redCount;    // 赤エレメント所持数
    public int blueCount;   // 青エレメント所持数
    public int greenCount;  // 緑エレメント所持数
    public int yellowCount; // 黄エレメント所持数

   
    public enum ElementType
    {
        Red,   // 赤エレメント
        Blue,  // 青エレメント
        Green, // 緑エレメント
        Yellow // 黄エレメント
    }
}
