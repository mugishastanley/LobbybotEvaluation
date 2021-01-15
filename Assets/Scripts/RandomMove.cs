using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMove : MonoBehaviour
{
    void Start()
    {
        InvokeRepeating("SetRandomPos", 0, 1);
    }

    void SetRandomPos()
    {
        Vector3 temp = new Vector3(Random.Range(-10.6f, 10.6f), Random.Range(-10.6f, 10.6f), Random.Range(-10.6f, 10.6f));
        transform.position = temp;
    }

}
