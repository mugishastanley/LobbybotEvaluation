using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    /*This script is used for testing purposes only
     It's intended to work as a stub*/

    public GameObject ToTest;
    public Transform tt;
    string pos;
    string pos2;
    string pos3;
    string pos4;
    Matrix4x4 m;
    Matrix4x4 n;
    TransformMatrix mt;
    // Start is called before the first frame update
    void Start()
    {
        pos = ToTest.GetComponent<Transform>().localPosition.ToString("F4") ;
        pos2 = ToTest.GetComponent<Transform>().position. ToString("F4");
        m = ToTest.transform.worldToLocalMatrix;
        Debug.Log("Pos 1: "+pos+ "Pos 2: "+pos2  + "m: " + m);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
