using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class closestobject : MonoBehaviour
{

    public GameObject closest;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        closest.transform.position = FindObjectOfType<KdFindClosest>().getclosestobjectposition();
        //if (fc != null)
        //{
        //closest.transform.position = fc.getclosestobjectpose();
        //closest.transform.position = fc.getclosestobjectpose();
        //closest.transform.rotation = fc.getclosestobjectrot();
        Debug.Log("From update closet Found next " + closest.transform.position.ToString("F5"));
    }

}
