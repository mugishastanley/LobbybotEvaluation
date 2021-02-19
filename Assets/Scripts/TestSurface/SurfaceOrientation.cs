using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceOrientation : MonoBehaviour
{

    public Transform testsurface;
    public float RotY;
    private Matrix4x4 T1;
    private Matrix4x4 T2;
    private Matrix4x4 T3;
    private Matrix4x4 T4;


    /*T=TransZ[15]  for the top surface
T=TransZ[2.87] * RotY[63.44°] * TransZ[7.75] for the other 5 surfaces
    RotY[63.44°] = 1.107 rads
* **/
    // Start is called before the first frame update
    void Start()
    {
        //case 1
        //testsurface.transform.position.z=0.15;
        
        //case2
        //testsurface.transform.position
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    Matrix4x4 Transformation1(float Theta)
    {
        T1[0, 0] = 1; T1[0, 1] = 0; T1[0, 2] = 0; T1[0, 3] = 0f;
        T1[1, 0] = 0; T1[1, 1] = 1.0f; T1[1, 2] = 0.0f; T1[1, 3] = 0;
        T1[2, 0] = 0f; T1[2, 1] = 0f; T1[2, 2] = 1; T1[2, 3] = 0.15f;
        T1[3, 0] = 0f; T1[3, 1] = 0f; T1[3, 2] = 0f; T1[3, 3] = 1.0f;
        return T1;
    }


    Matrix4x4 Transformation2(float Theta)
    {
        T2[0, 0] = Mathf.Cos (Theta); T2[0, 1] = 0 ; T2[0, 2] = 0.0775f * Mathf.Sin(Theta); T2[0, 3] = 0f;
        T2[1, 0] = 0; T2[1, 1] = 1.0f; T2[1, 2] = 0.0f; T2[1, 3] = 0;
        T2[2, 0] = -0.0281f*Mathf.Sin(Theta); T2[2, 1] = 0f; T2[2, 2] = 0.00217775f*Mathf.Cos(Theta); T2[2, 3] = 0f;
        T2[3, 0] = 0f; T2[3, 1] = 0f; T2[3, 2] = 0f; T2[3, 3] = 1.0f;
        return T2;
    }

    Matrix4x4 Transformation3(float Theta)
    {/*Inverse of T1*/
        T3[0, 0] = 1; T3[0, 1] = 0; T3[0, 2] = 0; T3[0, 3] = 0f;
        T3[1, 0] = 0; T3[1, 1] = 1.0f; T3[1, 2] = 0.0f; T3[1, 3] = 0;
        T3[2, 0] = 0f; T3[2, 1] = 0f; T3[2, 2] = 1; T3[2, 3] = -0.15f;
        T3[3, 0] = 0f; T3[3, 1] = 0f; T3[3, 2] = 0f; T3[3, 3] = 1.0f;
        return T3;
    }


    Matrix4x4 Transformation4(float Theta)
    {/*Inverse of T2*/
        T4[0, 0] = Mathf.Cos(Theta); T4[0, 1] = 0; T4[0, 2] = -Mathf.Sin(Theta); T4[0, 3] = 0.0285f* Mathf.Sin(Theta);
        T4[1, 0] = 0; T4[1, 1] = 1.0f; T4[1, 2] = 0.0f; T4[1, 3] = 0;
        T4[2, 0] =  Mathf.Sin(Theta); T4[2, 1] = 0f; T4[2, 2] = Mathf.Cos(Theta); T4[2, 3] = -0.0285f * Mathf.Cos(Theta) - 0.0775f;
        T4[3, 0] = 0f; T4[3, 1] = 0f; T4[3, 2] = 0f; T4[3, 3] = 1.0f;
        return T4;

    }

  






}
