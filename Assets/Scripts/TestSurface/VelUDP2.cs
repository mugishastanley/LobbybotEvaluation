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
    private GameObject Visual;
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

    public Matrix4x4 sentdata{get;set; }

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
    Matrix4x4 Home;

    //Vector3 rotation = new Vector3(0, 90, 0);
    Vector3 previous = new Vector3(0.0f, 0.0f, 0.0f);
    float velfactor=0.25f;
    float wspace = 0f;
    private readonly float tol = 0.01f; 

    //Vector3 Interpoint = new Vector3(0, 180, 0);


    /**
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

    **/

    // start from unity3d
    public void Start()
    {

        init();
    }

    // OnGUI
    void OnGUI()
    {
        var position = FindObjectOfType<KdFindClosest>().Nearobpos;
        var rotation = FindObjectOfType<KdFindClosest>().getclosestobjectrotation();
        Visual.transform.position = FindObjectOfType<KdFindClosest>().Colorpose;
        Visual.transform.rotation = FindObjectOfType<KdFindClosest>().getclosestobjectrotation();

        //Matrix4x4 RobotToCalTracker = FindObjectOfType<TestTransforms>().RB2CT();

        Matrix4x4 RobotToCalTracker = RB2CT();
        Vector3 posbe4 = position;

        string velocity = VelscalerTest(posbe4).ToString("F4");
        Debug.Log("posbe4 " + posbe4.ToString("F3")+"Velocity "+velocity);
        //string velocity = Velscaler(posbe4).ToString("F4");

        //Matrix4x4 Matrixsent = RobotToCalTracker * Matrix4x4.TRS(Tosend.transform.position, Tosend.transform.rotation, new Vector3(1,1,1));
        //Matrix4x4 Matrixsent = RobotToCalTracker * Matrix4x4.TRS(Tosend.transform.position, Tosend.transform.rotation, new Vector3(1,1,1)) * Transform3(90);
        //Matrix4x4 Matrixsent = RobotToCalTracker * Matrix4x4.TRS(Tosend.transform.position, Tosend.transform.rotation, new Vector3(1,1,1)) * Transform4(63.44f);

        // Changes 23
        Matrix4x4 Matrixsent = RobotToCalTracker * Matrix4x4.TRS(position, rotation, new Vector3(1, 1, 1)) * FindObjectOfType<SelectFace>().ChangeSurface();
        sentdata = Matrixsent;

        Matrix4x4 Matrixsent2 = Unity2urmat(Matrixsent);
        string datasent = Matrixsent2.ToString("F4") + ' ' + velocity;
        Debug.Log($"To send pos{datasent}");


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

        sendString(strMessage + "\n");
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

   
    public Matrix4x4 RB2CT()
    {
        Matrix4x4 T4 = new Matrix4x4();
        ////base_T_cal
        T4[0, 0] = 0.7071068f; T4[0, 1] = 0.7071068f; T4[0, 2] = 0.0f; T4[0, 3] = 0.12728f;
        T4[1, 0] = 0.0f; T4[1, 1] = 0.0f; T4[1, 2] = 1.0f; T4[1, 3] = -0.02000f;
        T4[2, 0] = 0.7071068f; T4[2, 1] = -0.7071068f; T4[2, 2] = 0.0f; T4[2, 3] = 0.11314f;
        T4[3, 0] = 0.0f; T4[3, 1] = 0.0f; T4[3, 2] = 0.0f; T4[3, 3] = 1.0f;
        return T4;

    }



}
