using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Link : MonoBehaviour
{
    public List<Node> nodes = new List<Node>();
    public float distance;

    const float LINE_WIDTH = 0.1f; // リンクの太さなどを指定

    public bool Contains(Node node)
    {
        return nodes.Contains(node);
    }

    public Node GetOtherNode(Node node)
    {
        return nodes[0] == node ? nodes[1] : nodes[0];
    }
    void Start()
    {
        Vector2 startPosition = nodes[0].transform.position;
        Vector2 endPosition = nodes[1].transform.position;

        // リンクの向き（角度）を計算
        Vector2 direction = endPosition - startPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // リンクの長さを計算
        distance = direction.magnitude;

        // リンクの位置、向き、長さを設定
        transform.position = startPosition + 0.5f * direction;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        transform.localScale = new Vector2(distance, LINE_WIDTH);

        // ノードにリンク情報追加
        nodes[0].links.Add(this);
        nodes[1].links.Add(this);
    }
}
