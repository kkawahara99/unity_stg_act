using System.Collections.Generic;
using UnityEngine;

public class DijkstraAlgorithm : MonoBehaviour
{
    public List<Node> nodes;
    public List<Link> links;

    void Start()
    {
        // シーン上のNodeとLinkを取得
        GameObject[] nodeObjects = GameObject.FindGameObjectsWithTag("Node");
        foreach(GameObject nodeObject in nodeObjects)
        {
            nodes.Add(nodeObject.GetComponent<Node>());
        }
        GameObject[] linkObjects = GameObject.FindGameObjectsWithTag("Link");
        foreach(GameObject linkObject in linkObjects)
        {
            links.Add(linkObject.GetComponent<Link>());
        }
    }

    public Node FindShortestPath(Node startNode, Node endNode)
    {
        Dictionary<Node, float> distances = new Dictionary<Node, float>(); // ノードの距離
        Dictionary<Node, Node> previous = new Dictionary<Node, Node>(); // 前回ノード
        List<Node> unvisited = new List<Node>(); // 未訪問ノード

        // 距離、前回ノードの初期化
        foreach (Node node in nodes)
        {
            distances[node] = float.MaxValue;
            previous[node] = null;
            unvisited.Add(node);
        }

        // 始点ノードの距離は0
        distances[startNode] = 0;

        // 訪問済みノードを初期化

        while (unvisited.Count > 0)
        {
            // 現在の最短距離が最小となる未訪問ノードを取得して訪問済みにする
            Node currentNode = GetNodeWithShortestDistance(unvisited, distances);
            unvisited.Remove(currentNode);

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                float newDistance = distances[currentNode] + GetDistance(currentNode, neighbor);;

                if (newDistance < distances[neighbor])
                {
                    distances[neighbor] = newDistance;
                    previous[neighbor] = currentNode;
                }
            }
        }

        // 経路の構築
        List<Node> path = new List<Node>();
        Node current = endNode;
        while (current != null)
        {
            path.Insert(0, current);
            current = previous[current];
        }

        return path.Count > 1 ? path[1] : null; // 始点の次に進むべきNodeを返す
    }

    // 最短ノードを取得
    Node GetNodeWithShortestDistance(List<Node> nodes, Dictionary<Node, float> distances)
    {
        float shortestDistance = Mathf.Infinity;
        Node shortestNode = null;

        foreach (Node node in nodes)
        {
            if (distances[node] < shortestDistance)
            {
                shortestDistance = distances[node];
                shortestNode = node;
            }
        }

        return shortestNode;
    }

    private List<Node> GetNeighbors(Node node)
    {
        // 指定されたNodeの隣接するNodeを取得するロジックを実装
        // 例えば、nodeのリンク先を返すことができます
        return node.links.ConvertAll(link => link.GetOtherNode(node));
    }

    private float GetDistance(Node nodeA, Node nodeB)
    {
        // 2つのNode間の距離を取得するロジックを実装
        // 例えば、nodeA と nodeB の位置ベクトルの距離を計算することができます
        return Vector2.Distance(nodeA.transform.position, nodeB.transform.position);
    }
}