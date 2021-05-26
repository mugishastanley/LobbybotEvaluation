using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testprojection : MonoBehaviour
{
    public Transform capsule;
    //public Transform plane;
    [SerializeField]
    public Plane plane2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 targetPos = Vector3.ProjectOnPlane(capsule.position, plane.up);

        Vector3 tpos = plane2.ClosestPointOnPlane(capsule.position);
        Debug.Log("Intersection at" + tpos.ToString("F3"));
        Debug.DrawLine(tpos,capsule.position,Color.red);

    }
}
