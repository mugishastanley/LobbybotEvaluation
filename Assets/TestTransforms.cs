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
        var Rbw2l = Robot.transform.worldToLocalMatrix;
        invRb[0, 0] = Rb[0, 0];
        invRb[0, 1] = Rb[1, 0];
        invRb[0, 2] = Rb[2, 0];
        invRb[0, 3] = -Rb[0, 3];

        invRb[1, 0] = Rb[0, 1];
        invRb[1, 1] = Rb[1, 1];
        invRb[1, 2] = Rb[2, 1];
        invRb[1, 3] = -Rb[1, 3];

        invRb[2, 0] = Rb[0, 2];
        invRb[2, 1] = Rb[1, 2];
        invRb[2, 2] = Rb[2, 2];
        invRb[2, 3] = -Rb[2, 3];

        invRb[3, 0] = Rb[3, 0];
        invRb[3, 1] = Rb[3, 1];
        invRb[3, 2] = Rb[3, 2];
        invRb[3, 3] = Rb[3, 3];


        

        Matrix4x4 RTC = invRb * CT;


        //print("Rb  :" + Rb);
        //print("Rbw  :" + Rb.inverse);
        //print("Rbw2l  :" + Rbw2l);
        //print("invRb Matrix" + invRb);


        var calTrackerMat = Matrix4x4.TRS(CalTracker.transform.localPosition, CalTracker.transform.rotation, new Vector3(1, 1, 1));
        print("calTrackerMat " + calTrackerMat);
        print("Rb  :" + Rb);

        //print("CT  :" + CT);

        //print("TR Matrix" + RTC.ToString("F3"));   

        return  invRb * CT ;


    }




    public Matrix4x4 RB2CT()
    {
        Matrix4x4 T4 = new Matrix4x4();
        //rotation in z -45, translation in x -15cm
        T4[0, 0] = 0.7071068f; T4[0, 1] = 0.7071068f; T4[0, 2] = 0;    T4[0, 3] = -0.00f;
        T4[1, 0] = -0.7071068f; T4[1, 1] = 0.7071068f;  T4[1, 2] = 0.0f; T4[1, 3] = -0.00f;
        T4[2, 0] = 0f;          T4[2, 1] = 0f;          T4[2, 2] = 1;    T4[2, 3] = -0.00f;
        T4[3, 0] = 0f;         T4[3, 1] = 0f;          T4[3, 2] = 0f;   T4[3, 3] = 1.0f;
        return T4;
        
   
    }
}


