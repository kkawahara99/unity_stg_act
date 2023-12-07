using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    private float exprosionTime = 0.2f;

    void Start()
    {
        transform.rotation = Quaternion.Euler(0, 0, Random.value * 360);
        Destroy(gameObject, exprosionTime);
    }

}
