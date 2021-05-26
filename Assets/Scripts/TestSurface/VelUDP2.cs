using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
//using Renci.SshNet;

/*
    -----------------------
    UDP-Send
    -----------------------
*/

public class VelUDP2 : MonoBehaviour
{
    [SerializeField]
    private GameObject RobotURDF;
    [SerializeField]
    private GameObject Tosend;
    [SerializeField]
    private GameObject Workspaceplane;
    [SerializeField]
    private Transform Torso;

    private Transform Tooltip;

    private static int localPort;
    private readonly float PI = 3.1416f;
    // prefs
    private string IP;  // define in init
    private int port;  // define in init

    // "connection" things
    IPEndPoint remoteEndPoint;
    UdpClient client;

    // gui
    string strMessage = "";

    Matrix4x4 TrackedPointWRTVR;
    Matrix4x4 TrackedPointWRTRobot;
    Matrix4x4 RobotWRTVR;
    Matrix4x4 VRWRTRobot;
    Matrix4x4 RobotWRTWorld;
    Matrix4x4 Testmat;
    Matrix4x4 T1;
    Matrix4x4 T2;
    Matrix4x4 T3;
    Matrix4x4 T4;
    Matrix4x4 T5;
    Matrix4x4 b1_T_b;
    Matrix4x4 tp1_T_tp;
    Matrix4x4 Home;

    //Vector3 rotation = new Vector3(0, 90, 0);
    Vector3 previous = new Vector3(0.0f, 0.0f, 0.0f);
    float velfactor=0.25f;
    float wspace = 0f;
    private readonly float tol = 0.01f; 

    //Vector3 Interpoint = new Vector3(0, 180, 0);



    // call it from shell (as program)
    private static void Main()
    {
        VelUDP2 sendObj = new VelUDP2();
        sendObj.init();

        // testing via console
        // sendObj.inputFromConsole();
        // as server sending endless
        sendObj.sendEndless(" endless infos \n");

    }
    // start from unity3d
    public void Start()
    {

        init();
    }

    // OnGUI
    void OnGUI()
    {
        //Location of closest object
        //Tosend.transform.position = FindObjectOfType<KdFindClosest>().getclosestobjectposition();
        Tosend.transform.position = FindObjectOfType<KdFindClosest>().Nearobpos;
        Tosend.transform.rotation = FindObjectOfType<KdFindClosest>().getclosestobjectrotation();


        Matrix4x4 RobotToCalTracker = FindObjectOfType<TestTransforms>().RB2CT();
        // var mat2 = FindObjectOfType<TestTransforms>().CaltoRobot();

        Vector3 posbe4 = Tosend.transform.position;
        //string velocity = Velscaler3(posbe4, Torso).ToString("F4");
        //Debug.Log("posbe4 " + posbe4.ToString("F4"));

        string velocity = VelscalerTest(posbe4).ToString("F4");
        Debug.Log("posbe4 " + posbe4.ToString("F3")+"Velocity "+velocity);
        //string velocity = Velscaler(posbe4).ToString("F4");

        //Matrix4x4 Matrixsent = RobotToCalTracker * Matrix4x4.TRS(Tosend.transform.position, Tosend.transform.rotation, new Vector3(1,1,1));
        //Matrix4x4 Matrixsent = RobotToCalTracker * Matrix4x4.TRS(Tosend.transform.position, Tosend.transform.rotation, new Vector3(1,1,1)) * Transform3(90);
        //Matrix4x4 Matrixsent = RobotToCalTracker * Matrix4x4.TRS(Tosend.transform.position, Tosend.transform.rotation, new Vector3(1,1,1)) * Transform4(63.44f);

        // Changes 23
        Matrix4x4 Matrixsent = RobotToCalTracker * Matrix4x4.TRS(Tosend.transform.position, Tosend.transform.rotation, new Vector3(1, 1, 1));
        //Before changing base
        // print("base" + Matrixsent);

        // Matrix4x4 Matrixsent_b1 = base1_T_base(90) * Matrixsent;
        //Matrix4x4 Matrixsent_b1_tcp1 = Matrixsent_b1 * TCP_T_TCP1(90);

        //after changing base
        //print("base1" + Matrixsent_b1);
        //print("b1_TCP1" + Matrixsent_b1_tcp1);

        //Matrix4x4 Matrixsent = RobotToCalTracker * base1_T_base(90) * Matrix4x4.TRS(Tosend.transform.position, Tosend.transform.rotation, new Vector3(1,1,1)) * TCP_T_TCP1(90);

        ////Before
        // print("Before" + Matrixsent);

        //Matrixsent = Matrixsent_b1;

        //After conversion.
        Matrix4x4 Matrixsent2 = Unity2urmat(Matrixsent);

        //print("After" + Matrixsent2);

        string datasent = Matrixsent2.ToString("F4") + ' ' + velocity;
        Debug.Log($"To send pos{datasent}");
        //Debug.Log("To send pos" + pos + "Orient" + rot);

        //string datasent = EEWRTRobot.ToString("F8");

        Rect rectObj = new Rect(40, 380, 200, 400);
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        GUI.Box(rectObj, "# UDPSend-Data\n127.0.0.1 " + port + " #\n"
                    + "shell> nc -lu 127.0.0.1  " + port + " \n"
                    + datasent
                , style);

        // ------------------------
        // send it
        // ------------------------

        strMessage = GUI.TextField(new Rect(40, 420, 140, 20), datasent);

        // if (GUI.Button(new Rect(190, 420, 40, 20), "send"))
        // {
        sendString(strMessage + "\n");

        // print("Testing: nc -lu " + IP + " : " + pos);
        // }
    }

    private static Matrix4x4 Unity2urmat(Matrix4x4 Matrixsent)
    {
        Matrix4x4 Matrixsent2 = new Matrix4x4();
        //rotx
        Matrixsent2[0, 0] = Matrixsent[0, 0];
        Matrixsent2[1, 0] = Matrixsent[2, 0];
        Matrixsent2[2, 0] = Matrixsent[1, 0];

        //Rot y - swap with previous Z
        Matrixsent2[0, 1] = Matrixsent[0, 2];
        Matrixsent2[1, 1] = Matrixsent[2, 2];
        Matrixsent2[2, 1] = Matrixsent[1, 2];

        //translation
        Matrixsent2[0, 3] = Matrixsent[0, 3];
        Matrixsent2[1, 3] = Matrixsent[2, 3];
        Matrixsent2[2, 3] = Matrixsent[1, 3];

        //rot Z - swap with previous Y
        Matrixsent2[0, 2] = Matrixsent[0, 1];
        Matrixsent2[1, 2] = Matrixsent[2, 1];
        Matrixsent2[2, 2] = Matrixsent[1, 1];

        Matrixsent2[3, 3] = 1.0f;
        return Matrixsent2;
    }

    // init
    public void init()
    {
        // Endpunkt definieren, von dem die Nachrichten gesendet werden.
        print("UDPSend.init()");

        // define
               
        IP = "192.168.0.101";
        port = 21000;

        // ----------------------------
        // Send
        // ----------------------------
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        client = new UdpClient();
        // mat_to_vec();
        // status
        print("Sending to " + IP + " : " + port);
        //print("Testing: nc -lu " + IP + " : " + pos);


    }

    // inputFromConsole
    private void inputFromConsole()
    {
        try
        {
            string text;
            do
            {
                text = Console.ReadLine();

                // Den Text zum Remote-Client senden.
                if (text != "")
                {

                    // Daten mit der UTF8-Kodierung in das Binärformat kodieren.
                    byte[] data = Encoding.UTF8.GetBytes(text);

                    // Den Text zum Remote-Client senden.
                    client.Send(data, data.Length, remoteEndPoint);
                }
            } while (text != "");
        }
        catch (Exception err)
        {
            print(err.ToString());
        }

    }

    // sendData
    private void sendString(string message)
    {
        try
        {
            //if (message != "")
            //{
            // Daten mit der UTF8-Kodierung in das Binärformat kodieren.
            byte[] data = Encoding.UTF8.GetBytes(message);

            // Den message zum Remote-Client senden.
            client.Send(data, data.Length, remoteEndPoint);
            //}
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }

    // endless test
    private void sendEndless(string testStr)
    {
        do
        {
            sendString(testStr);
        }
        while (true);

    }

   Matrix4x4 HomePos()
    {
        Home[0, 0] = 0.98208f; Home[0, 1] = -0.18160f; Home[0, 2] = 0.05046f; Home[0, 3] = -0.03152f;
        Home[1, 0] = 0.12616f; Home[1, 1] = 0.43447f; Home[1, 2] = -0.89181f; Home[1, 3] = 0.69367f;
        Home[2, 0] = 0.14002f; Home[2, 1] = 0.88219f; Home[2, 2] = 0.44960f; Home[2, 3] = -0.51877f;
        Home[3, 0] = 0f; Home[3, 1] = 0f; Home[3, 2] = 0f; Home[3, 3] = 1.0f;


        return Home;
    }

   
    public float Velscaler(Vector3 point)
    {    
        Vector3 plane = Workspaceplane.transform.localPosition;

        Debug.Log("Plane position " + plane.ToString("F3"));
        if (previous != point)
        {
            if ((point.x < plane.x))
            {
                Debug.Log("we are inside the car");
               // if (previous.x <= plane.x)//previous point outside
                {
               //     velfactor = 0.6f;
                }
                // else
                // velfactor = 0.25f;
                velfactor = 0.25f;
                Debug.Log("point.x < plane.x " + velfactor.ToString("F3"));
                Debug.Log("point.x < plane.x ==plane.x " + plane.x.ToString("F3"));
                Debug.Log("point.x < plane.x== point.x " + point.x.ToString("F3"));
            }
            else
            {
                if ( Mathf.Approximately(previous.x, plane.x))//previous point inside or on plane
                {
                    Debug.Log("We are outside");
                    velfactor = 0.6f;
                    Debug.Log("previous.x >= plane.x " + velfactor.ToString("F3"));
                }
                else
                {
                    velfactor = 0.25f;
                    Debug.Log("else previous.x >= plane.x" + velfactor.ToString("F3"));
                }
                    

            }
            previous = point;
        }
        return velfactor;
    }

    public float Velscalernew(Vector3 point)
    {
        Vector3 plane = Workspaceplane.transform.localPosition;

        Debug.Log("Plane position " + plane.ToString("F3"));
        if (previous != point)
        {
            if ((point.x > plane.x) || (Mathf.Abs(point.x-plane.x)<tol))
            { //we are inside the car
                if ((previous.x > plane.x) || (Mathf.Abs(previous.x - plane.x) < tol))//previous point inside
                {
                    velfactor = 0.4f;
                    Debug.Log("previous.x >= plane.x " + velfactor.ToString("F3"));
                }
                else
                {
                    velfactor = 0.25f;
                    Debug.Log("else previous.x >= plane.x" + velfactor.ToString("F3"));
                }
            }
            else
            {
                velfactor = 0.25f;
                Debug.Log("point.x < plane.x " + velfactor.ToString("F3"));
                Debug.Log("point.x < plane.x == plane.x " + plane.x.ToString("F3"));
                Debug.Log("point.x < plane.x == point.x " + point.x.ToString("F3"));


            }
            previous = point;
        }
        return velfactor;
    }

    public float VelscalerTest(Vector3 point)
    {
        Vector3 plane = Workspaceplane.transform.localPosition;

        Debug.Log("Plane position " + plane.ToString("F3"));
        if (previous != point)
        {
            if ((point.x > plane.x) || (Mathf.Abs(point.x - plane.x) < tol))
            { //we are inside the car
                if ((previous.x > plane.x) || (Mathf.Abs(previous.x - plane.x) < tol))//previous point outside or on plane
                {
                    velfactor = 0.6f;
                    Debug.Log("previous was outside or on plane vel next is outside or on plane" + velfactor.ToString("F3") + "prev pos" + previous.x);
                }
                else
                {
                        velfactor = 0.25f;
                    //    Debug.Log("else previous.x >= plane.x" + velfactor.ToString("F3"));
                    Debug.Log("previous was inside plane  next is on plane vel " + velfactor.ToString("F3") +"prev pos"+previous.x);
                }
            }
            else
            {
                velfactor = 0.25f;
                //Debug.Log("point.x < plane.x " + velfactor.ToString("F3"));
                //Debug.Log("point.x < plane.x == plane.x " + plane.x.ToString("F3"));
                //Debug.Log("point.x < plane.x == point.x " + point.x.ToString("F3"));
                Debug.Log("previous inside plane next is also inside plane vel " + velfactor.ToString("F3") + "prev pos" + previous.x);


            }
            previous = point;
        }
        return velfactor;
    }





    public float Velscaler3(Vector3 point, Transform torso) // with 3 worksapces
    {
        Vector3 plane = Workspaceplane.transform.localPosition;
        float xcenter = torso.transform.position.x;
        float zcenter = torso.transform.position.z;
        float xoffset = 0.1f;
        float zoffset = 0.2f;
        float xu = xcenter + xoffset;
        float xl = xcenter - xoffset;
        float zu = zcenter + zoffset;
        float zl = zcenter - zoffset;

        if (previous != point)
        {
            if (point.x <= plane.x)
            { //we are inside the car
              
                if (((xl <= point.x) && (point.x <= xu)) && ((zl <= point.z) && (point.z <= zu)))//point inside WH space, hand approx as retangle.
                {
                    velfactor = 0.1f;
                }
                else
                    velfactor = 0.25f;
            }
            else
            {
                if (previous.x <= plane.x)//previous point outside
                {
                    velfactor = 0.25f;
                }
                else
                    velfactor = 0.6f;
            }
            previous = point;
        }
        return velfactor;
    }
    /**
    public float workspace(Vector3 p)
    {

        //workspace computation variables
        float xcenter = UserHand.transform.position.x;
        float ycenter = UserHand.transform.position.y;
        float xplanedivider = Workspaceplane.transform.position.x;
        float xoffset = 1.0f;
        float yoffset = 0.7f;
        float xu = xcenter + xoffset;
        float xl = xcenter - xoffset;
        float yu = ycenter + yoffset;
        float yl = ycenter - yoffset;
        float space = 0.0f;
        //

        /** In put: Vector 3 point, user hand, Inside car/outside car separating place plane pn
         * Output: Vector3 point and a workspace value
         **/
        /**
        if (p.x >= xplanedivider)
        {
            //wspace = 0.3f;
            if (((xl <= p.x) && (p.x <= xu)) && ((yl <= p.y) && (p.y <= yu)))//point inside space
            {
                wspace = 0.2f;
            }
        }
        else wspace = 0.7f;
        return wspace;
    }
        **/

    //intermediate point algorithm.
    public void workspaceintermediate(Vector3 p, out float wspace, out Vector3 Interpoint)
    {
        Interpoint = new Vector3(0, 0, 0);
        float xplanedivider = Workspaceplane.transform.position.x;
        if (p.x >= xplanedivider)
        {
            wspace = 0.6f;
        }
        else wspace = 0.8f;
    }

    //Matrix Transforms

    //ROBOT REERENCE FRAME//
    // Transform from TCP to prop top surface , In ROBOT Cordinate System.
    // Translation in z by 0.116 m
    //    T1 = [ 1  0   0   0
    //           0  1   0   0
    //           0  0   1   0.116
    //           0  0   0   1     ]

    // Transform from prop top surface To TCP, In ROBOT Cordinate System.
    // Translation in z by -0.116 m
    //    T1' = [ 1  0   0   0
    //           0  1   0   0
    //           0  0   1   -0.116
    //           0  0   0   1     ]

    // Transform from TCP to prop side surface , In ROBOT Cordinate System.
    // Trans Z (0.0287) Rot x (-63.44) Trans Z(0.0775)
    //    T2 = [ 1  0               0               0
    //           0  cos(theta)     -sin(theta)     -0.0775*sin(theta)   
    //           0  sin(theta)      cos(theta)      0.0775cos(theta) + 0.0287
    //           0  0               0               1                            ]

    // Transform from prop side surface to TCP , In ROBOT Cordinate System.
    // Trans Z(-0.0775) Rot x (63.44) Trans Z (-0.0287)  
    //    T2' = [   1   0               0               0
    //              0   cos(theta)      sin(theta)     -0.0287*sin(theta)   
    //              0  -sin(theta)      cos(theta)     -0.0775 - 0.0287*cos(theta)
    //              0   0               0               1                            ]




    //CAPSULE REFERENCE FRAME //
    // Transform from tcp to prop top surface , In Capsule Reference. (X Y Z) is (Z Y X) in Robot System
    // Translation in x by 0.116 m
    //    T1 = [ 1  0   0   0.116
    //           0  1   0   0
    //           0  0   1   0
    //           0  0   0   1     ]

    // Transform from prop top surface To TCP, In Capsule Reference. (X Y Z) is (Z Y X) in Robot System.
    // Translation in x by -0.116 m
    //    T1' = [   1  0   0   -0.116
    //              0  1   0    0
    //              0  0   1    0
    //              0  0   0    1     ]

    // Transform from TCP to prop side surface ,In Capsule Reference. (X Y Z) is (Z Y X) in Robot System.
    // Trans X (0.0287) Rot Z (-63.44) Trans X (0.0775)
    //    T2 = [ cos(theta)   - sin(theta)      0     0.0775cos(theta) + 0.0287
    //           sin(theta)     cos(theta)      0     0.0775*sin(theta)   
    //           0              0               1     0
    //           0              0               0     1                            ]

    // Transform from prop side surface to TCP , In Capsule Reference. (X Y Z) is (Z Y X) in Robot System.
    // Trans X(-0.0775) Rot Z (63.44) Trans X (-0.0287)  
    //    T2' = [    cos(theta)     sin(theta)      0      -0.0775 - 0.0287*cos(theta)
    //              -sin(theta)     cos(theta)      0       0.0287*sin(theta)   
    //                  0           0               1       0
    //                  0           0               0            1                ]

    //UNITY REFERENCE FRAME//
    // Transform from tcp to prop top surface , In Unity Reference. (X Y Z) is ( X Z Y) in Robot System.
    // Translation in Y by 0.116 m
    //    T1 = [ 1  0   0   0
    //           0  1   0   0.116
    //           0  0   1   0
    //           0  0   0   1     ]

    // Transform from prop top surface To TCP, In Unity Reference. (X Y Z) is ( X Z Y) in Robot System.
    // Translation in Y by -0.116 m
    //    T1' = [   1  0   0    0
    //              0  1   0    -0.116
    //              0  0   1    0
    //              0  0   0    1     ]

    // Transform from TCP to prop side surface ,In Unity Reference. (X Y Z) is ( X Z Y) in Robot System.
    // Trans Y (0.0287) Rot X (63.44) Trans Y (0.0775)
    //    T2 = [ 1  0            0             0 
    //           0  cos(theta)  -sin(theta)    0.0775*cos(theta) + 0.0287
    //           0  sin(theta)   cos(theta)    0.0775sin(theta)
    //           0  0            0             1                                 ]


    // Transform from prop side surface to TCP , In Unity Reference. (X Y Z) is ( X Z Y) in Robot System.
    // Trans Y(-0.0775) Rot X (-63.44) Trans Y (-0.0287)  
    //    T2' = [    1      0               0               0
    //               0      cos(theta)      sin(theta)      -0.0287*cos(theta) - 0.0775   
    //               0      -sin(theta)     cos(theta)      0.0287*sin(theta)
    //               0      0               0               1                ]


    // Transform from TCP to prop side surface ,In Unity Reference. (X Y Z) is ( X Z Y) in Robot System.
    // Trans Y (0.0287)Rot Y(72.n) Rot X (63.44) Trans Y (0.0775)
    //    T2 = [ cos(theta1)    sin(theta1)*sin(theta2)   cos(theta2)*sin(theta1)      0.0775*sin(theta1)*sin(theta2) 
    //           0              cos(theta2)              -sin(theta2)                    0.0775*cos(theta2) + 0.0287
    //          -sin(theta1)    cos(theta1)*sin(theta2)   cos(theta2)*cos(theta1)       0.0775sin(theta2)*cos(theta1)
    //           0              0                         0                             1                                 ]


    // Transform from prop side surface to TCP , In Unity Reference. (X Y Z) is ( X Z Y) in Robot System.
    // Trans Y(-0.0775) Rot Y(-72.n) Rot X (-63.44) Trans Y (-0.0287)
    // the input has to a positive angle.(sign has beeen taken into cosideration)
    //    T2' = [   cos(theta1)                   0                   -sin(theta1)                   0
    //              sin(theta1)*sin(theta2)       cos(theta2)          cos(theta1)*sin(theta2)       -0.0287*cos(theta2) - 0.0775 
    //              cos(theta2)*sin(theta1)      -sin(theta2)          cos(theta2)*cos(theta1)       0.0287*sin(theta2)
    //              0                             0                    0                             1                                 ]

    Matrix4x4 Transform3(float theta)
    {/*Inverse of T1*/
        T3[0, 0] = 1.0f; T3[0, 1] = 0.0f;     T3[0, 2] = 0.0f;       T3[0, 3] = 0.0f;
        T3[1, 0] = 0.0f; T3[1, 1] = 1.0f;     T3[1, 2] = 0.0f;       T3[1, 3] = -0.116f;
        T3[2, 0] = 0.0f; T3[2, 1] = 0.0f;     T3[2, 2] = 1.0f;       T3[2, 3] = 0.0f;
        T3[3, 0] = 0.0f; T3[3, 1] = 0.0f;     T3[3, 2] = 0.0f;       T3[3, 3] = 1.0f;
        return T3;
    }

    Matrix4x4 base1_T_base(float theta)
    {
        b1_T_b[0, 0] = -1.0f;    b1_T_b[0, 1] = 0.0f;     b1_T_b[0, 2] = 0.0f;     b1_T_b[0, 3] = 0.0f;
        b1_T_b[1, 0] = 0.0f;     b1_T_b[1, 1] = 1.0f;     b1_T_b[1, 2] = 0.0f;     b1_T_b[1, 3] = 0.0f;
        b1_T_b[2, 0] = 0.0f;     b1_T_b[2, 1] = 0.0f;     b1_T_b[2, 2] = -1.0f;    b1_T_b[2, 3] = 0.0f;
        b1_T_b[3, 0] = 0.0f;     b1_T_b[3, 1] = 0.0f;     b1_T_b[3, 2] = 0.0f;     b1_T_b[3, 3] = 1.0f;
        return  b1_T_b;
    }
    Matrix4x4 TCP_T_TCP1(float theta)
    {
        tp1_T_tp[0, 0] = 1.0f;      tp1_T_tp[0, 1] = 0.0f;      tp1_T_tp[0, 2] = 0.0f;      tp1_T_tp[0, 3] = 0.0f;
        tp1_T_tp[1, 0] = 0.0f;      tp1_T_tp[1, 1] = 0.0f;      tp1_T_tp[1, 2] = 1.0f;      tp1_T_tp[1, 3] = 0.0f;
        tp1_T_tp[2, 0] = 0.0f;      tp1_T_tp[2, 1] = -1.0f;     tp1_T_tp[2, 2] = 0.0f;      tp1_T_tp[2, 3] = 0.0f;
        tp1_T_tp[3, 0] = 0.0f;      tp1_T_tp[3, 1] = 0.0f;      tp1_T_tp[3, 2] = 0.0f;      tp1_T_tp[3, 3] = 1.0f;
        return tp1_T_tp;
    }
  

    Matrix4x4 Transform4(float Theta)
    {/*Inverse of T2*/ //to reach the side surfaces of the prop
        Theta = Theta * Mathf.PI / 180;
        T4[0, 0] = 1.0f;    T4[0, 1] =  0.0f;                   T4[0, 2] = 0.0f;                      T4[0, 3] = 0.0f;
        T4[1, 0] = 0.0f;    T4[1, 1] = Mathf.Cos(Theta);        T4[1, 2] = Mathf.Sin(Theta);          T4[1, 3] = -0.0285f * Mathf.Cos(Theta) - 0.0775f;
        T4[2, 0] = 0.0f;    T4[2, 1] = -Mathf.Sin(Theta);       T4[2, 2] = Mathf.Cos(Theta);          T4[2, 3] = 0.0285f * Mathf.Sin(Theta);
        T4[3, 0] = 0.0f;    T4[3, 1] = 0.0f;                    T4[3, 2] = 0.0f;                      T4[3, 3] = 1.0f;

           return T4;

    }


    Matrix4x4 Transform45(float Theta1, float Theta2)
    {/*Inverse of T2*/ //to reach the side surfaces of the prop
        Theta1 = Theta1 * Mathf.PI / 180;
        Theta2 = Theta2 * Mathf.PI / 180;

        T5[0, 0] = Mathf.Cos(Theta1);                            T5[0, 1] = 0.0f;                 T5[0, 2] = -Mathf.Sin(Theta1);                      T5[0, 3] = 0.0f;
        T5[1, 0] = Mathf.Sin(Theta1) * Mathf.Sin(Theta2);        T5[1, 1] = Mathf.Cos(Theta2);    T5[1, 2] = Mathf.Cos(Theta1)*Mathf.Sin(Theta2);     T5[1, 3] = -0.0285f * Mathf.Cos(Theta2) - 0.0775f;
        T5[2, 0] = Mathf.Cos(Theta2) * Mathf.Sin(Theta1);        T5[2, 1] = -Mathf.Sin(Theta2);   T5[2, 2] = Mathf.Cos(Theta1)*Mathf.Cos(Theta2);     T5[2, 3] = 0.0285f * Mathf.Sin(Theta2);
        T5[3, 0] = 0.0f;                                         T5[3, 1] = 0.0f;                 T5[3, 2] = 0.0f;                                    T5[3, 3] = 1.0f;

        return T5;                    


    }

    /*Matrix utils*/

    public Quaternion GetRotation(Matrix4x4 matrix4X4)
    {
        float qw = Mathf.Sqrt(1f + matrix4X4.m00 + matrix4X4.m11 + matrix4X4.m22) / 2;
        float w = 4 * qw;
        float qx = (matrix4X4.m21 - matrix4X4.m12) / w;
        float qy = (matrix4X4.m02 - matrix4X4.m20) / w;
        float qz = (matrix4X4.m10 - matrix4X4.m01) / w;
        return new Quaternion(qx, qy, qz, qw);
    }

    public Vector3 GetPostion(Matrix4x4 matrix4X4)
    {
        var x = matrix4X4.m03;
        var y = matrix4X4.m13;
        var z = matrix4X4.m23;
        return new Vector3(x, y, z);
    }
    public Matrix4x4 FromTRS(Vector3 pos, Vector3 rot) {// returns a transform matrix from rotation translatio and scale.
        Vector3 scale = new Vector3(1, 1, 1);
        Quaternion rotation = Quaternion.Euler(rot.x, rot.y, rot.z);
        Matrix4x4 m = Matrix4x4.TRS(pos, rotation, scale);
        return m;
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
        T4[0, 0] = 0.7071068f; T4[0, 1] = 0.7071068f; T4[0, 2] = 0.0f; T4[0, 3] = 0.12728f;
        T4[1, 0] = 0.0f; T4[1, 1] = 0.0f; T4[1, 2] = 1.0f; T4[1, 3] = -0.02000f;
        T4[2, 0] = 0.7071068f; T4[2, 1] = -0.7071068f; T4[2, 2] = 0.0f; T4[2, 3] = 0.11314f;
        T4[3, 0] = 0.0f; T4[3, 1] = 0.0f; T4[3, 2] = 0.0f; T4[3, 3] = 1.0f;
        return T4;

    }



}
