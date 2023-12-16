using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Link> links;

    public Node OtherNode(Node node)
    {
        return links.Find(link => link.Contains(node)).GetOtherNode(this);
    }
}
