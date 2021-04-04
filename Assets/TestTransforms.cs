using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTransforms : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject CalTracker;
    public GameObject Robot;
    Matrix4x4 CT;
    Matrix4x4 CTlw;
    Matrix4x4 Rb;
    Matrix4x4 RbLW;
    Vector3 CTpos,Rblocalpos,Rbworldpos;
    Vector3 CTposlocal;
    
    Matrix4x4 invRb;
    public Matrix4x4 CaltoRobot() {
        CT = CalTracker.transform.localToWorldMatrix;
        
        Rb = Robot.transform.localToWorldMatrix;
        //var Rbw2l = Robot.transform.worldToLocalMatrix;
        Matrix4x4 invRb = new Matrix4x4();
        invRb[0, 0] = Rb[0, 0];
        invRb[0, 1] = Rb[1, 0];
        invRb[0, 2] = Rb[2, 0];

        invRb[1, 0] = Rb[0, 1];
        invRb[1, 1] = Rb[1, 1];
        invRb[1, 2] = Rb[2, 1];

        invRb[2, 0] = Rb[0, 2];
        invRb[2, 1] = Rb[1, 2];
        invRb[2, 2] = Rb[2, 2];

        invRb[0, 3] = -(invRb[0, 0] * Rb[0, 3] + invRb[0, 1] * Rb[1, 3] + invRb[0, 2] * Rb[2, 3]);
        invRb[1, 3] = -(invRb[1, 0] * Rb[0, 3] + invRb[1, 1] * Rb[1, 3] + invRb[1, 2] * Rb[2, 3]);
        invRb[2, 3] = -(invRb[2, 0] * Rb[0, 3] + invRb[2, 1] * Rb[1, 3] + invRb[2, 2] * Rb[2, 3]);

        invRb[3, 0] = Rb[3, 0];
        invRb[3, 1] = Rb[3, 1];
        invRb[3, 2] = Rb[3, 2];
        invRb[3, 3] = Rb[3, 3];

        Matrix4x4 invCT = new Matrix4x4();
        invCT[0, 0] = CT[0, 0];
        invCT[0, 1] = CT[1, 0];
        invCT[0, 2] = CT[2, 0];

        invCT[1, 0] = CT[0, 1];
        invCT[1, 1] = CT[1, 1];
        invCT[1, 2] = CT[2, 1];

        invCT[2, 0] = CT[0, 2];
        invCT[2, 1] = CT[1, 2];
        invCT[2, 2] = CT[2, 2];

        invCT[0, 3] = -(invCT[0, 0] * CT[0, 3] + invCT[0, 1] * CT[1, 3] + invCT[0, 2] * CT[2, 3]);
        invCT[1, 3] = -(invCT[1, 0] * CT[0, 3] + invCT[1, 1] * CT[1, 3] + invCT[1, 2] * CT[2, 3]);
        invCT[2, 3] = -(invCT[2, 0] * CT[0, 3] + invCT[2, 1] * CT[1, 3] + invCT[2, 2] * CT[2, 3]);

        invCT[3, 0] = CT[3, 0];
        invCT[3, 1] = CT[3, 1];
        invCT[3, 2] = CT[3, 2];
        invCT[3, 3] = CT[3, 3];
        



        Matrix4x4 RTC = invRb * CT;
        Matrix4x4 CTR = invCT * Rb;

        //print("CTR " + CTR);
        //print("RTC " + RTC);
        
        return CTR;


    }




    public Matrix4x4 RB2CT()
    {
        Matrix4x4 T4 = new Matrix4x4();
        
        //RotY90 RotX45 RotZ90 x=-0.17 y=-0.01 z=0.02  cal_T_base
        //T4[0, 0] = 0.7071068f;  T4[0, 1] = 0f;              T4[0, 2] = 0.7071068f;       T4[0, 3] = -0.17f;
        //T4[1, 0] = 0.7071068f;  T4[1, 1] = 0f;              T4[1, 2] = -0.7071068ff;    T4[1, 3] = -0.01f;
        //T4[2, 0] = 0f;          T4[2, 1] = 1f;              T4[2, 2] = 0;       T4[2, 3] = 0.02f;
        //T4[3, 0] = 0f;          T4[3, 1] = 0f;              T4[3, 2] = 0f;      T4[3, 3] = 1.0f;



        ////base_T_cal
        T4[0, 0] = 0.7071068f;  T4[0, 1] = 0.7071068f;     T4[0, 2] = 0.0f;       T4[0, 3] = 0.12728f;
        T4[1, 0] = 0.0f;        T4[1, 1] = 0.0f;           T4[1, 2] = 1.0f;       T4[1, 3] = -0.02000f;
        T4[2, 0] = 0.7071068f;  T4[2, 1] = -0.7071068f;     T4[2, 2] = 0.0f;      T4[2, 3] = 0.11314f;
        T4[3, 0] = 0.0f;        T4[3, 1] = 0.0f;           T4[3, 2] = 0.0f;       T4[3, 3] = 1.0f;


        return T4;
        
   
    }

}


