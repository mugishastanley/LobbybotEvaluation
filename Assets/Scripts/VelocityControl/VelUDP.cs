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

public class VelUDP : MonoBehaviour
{
    [SerializeField]
    private GameObject RobotURDF;

    [SerializeField]
    private GameObject Tosend;

    [SerializeField]
    private GameObject Workspaceplane;
    [SerializeField]
    private GameObject UserHand;

    private static int localPort;
    private readonly float  PI = 3.1416f;
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
    Matrix4x4 EEWRTRobot;

    Matrix4x4 Home;
    //Vector3 rotation = new Vector3(112.2997f, -26.31f,111.978f); // newline mar 22
    float velfactor;
    float wspace = 0f;
    Vector3 Interpoint = new Vector3(0, 180, 0);



    // call it from shell (as program)
    private static void Main()
    {
        VelUDP sendObj = new VelUDP();
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
        Tosend.transform.position = FindObjectOfType<KdFindClosest>().getclosestobjectposition();
        //To be fixed later when mind is fresh
        //Tosend.transform.rotation = FindObjectOfType<KdFindClosest>().getclosestobjectrotation();

        Vector3 posbe4 = Tosend.transform.position;
        //string velocity = workspace(Tosend.transform.localPosition).ToString("F4");
        Vector3 pos = Unity2Ros(Tosend.transform.localPosition);
        //string velocity = workspace(pos).ToString("F4");
        string velocity = Velscaler(posbe4).ToString("F4");
        string velocity2 = Velscaler(pos).ToString("F4");
        //string velocity2 = Velscaler(posbe4).ToString("F4");
        //string velocity = Velscaler(pos).ToString("F4");
        //string pos = Unity2Ros(Tosend.transform.localPosition).ToString("F5");
        //string rot = Tosend.transform.localrotation.ToString("F5");

        string rot = Rot(Tosend.transform.localEulerAngles).ToString("F4");

        //string rot = Rot(rotation).ToString("F4");. //newline 22
        //string rot = Tosend.transform.rotation.ToString("F4");//newline 22


        string posstr = pos.ToString("F4");
        //Debug.Log("Recieved" + Tosend.transform.localPosition +"Rotation"+rot);

        string datasent = posstr + ',' + rot+ velocity;
        //string datasent = posstr + ',' + rotation + velocity; // new line 22
        
        //string datasent = posstr + ',' + velocity2 ;
        //Debug.Log($"To send pos{datasent}");
        //Debug.Log("To send pos" + pos + "Orient" + rot);
        //string datasent = (Calculate_Transform() * Test.transform.localToWorldMatrix).ToString("F8");
        //Testmat = Test.transform.localToWorldMatrix; //End effector WRT world
        //RobotWRTWorld = RobotURDF.transform.localToWorldMatrix;
        //EEWRTRobot = Testmat.inverse * RobotWRTWorld;
        //EEWRTRobot[0, 3] = EEWRTRobot[1, 3] ; //x=z
        //EEWRTRobot[1, 3] = EEWRTRobot[2, 3] * -1; //y= -z
        //EEWRTRobot[2, 3] = EEWRTRobot[0, 3] * -1; //z= -x

        //Testmat[1, 3] -= TableOffsety;
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

    // init
    public void init()
    {
        // Endpunkt definieren, von dem die Nachrichten gesendet werden.
        print("UDPSend.init()");

        // define

        //IP = "127.0.0.1";
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

    string getMatrix(Matrix4x4 p)
    {
        // get matrix from the Transform 
        //var p = Tosend.transform.localToWorldMatrix;
        // get position from the last column
        //var position = new Vector3(matrix[0, 3], matrix[1, 3], matrix[2, 3]);
        //Debug.Log("Transform position from matrix is: " + position);
        //Debug.Log("Mat " + p.ToString("F8"));

        /**
    this function returns a string representation of a matrix
    */
        string strrep = p[0, 0].ToString("F8") + " " +
                         p[0, 1].ToString("F8") + " " +
                         p[0, 2].ToString("F8") + " " +
                         p[0, 3].ToString("F8") + " " +
                         p[1, 0].ToString("F8") + " " +
                         p[1, 1].ToString("F8") + " " +
                         p[1, 2].ToString("F8") + " " +
                         p[1, 3].ToString("F8") + " " +
                         p[2, 0].ToString("F8") + " " +
                         p[2, 1].ToString("F8") + " " +
                         p[2, 2].ToString("F8") + " " +
                         p[2, 3].ToString("F8");

        //Debug.Log(strrep);
        return strrep;
        //return matrix.ToString("F8");
    }


    Matrix4x4 Calculate_Transform()
    {
        /*Alternatively we could get the transform matrix of the object attached to end effecotr of robot
         * but we didnot because it was noisy and therefore could not be relied on.
         * **/


        /**
        TrackedPointWRTVR =
         0.03396 - 0.09355 - 0.00977    0.28243
            - 0.09345 - 0.03474    0.00776 0.94222
            - 0.01066    0.00650 - 0.09922 - 0.09420
            0.00000 0.00000 0.00000 1.0000**/
        TrackedPointWRTVR[0, 0] = 0.03396f; TrackedPointWRTVR[0, 1] = -0.09355f; TrackedPointWRTVR[0, 2] = -0.00977f; TrackedPointWRTVR[0, 3] = 0.28243f; //- TableOffsetx;
        TrackedPointWRTVR[1, 0] = -0.09345f; TrackedPointWRTVR[1, 1] = -0.03474f; TrackedPointWRTVR[1, 2] = 0.00776f; TrackedPointWRTVR[1, 3] = 0.94222f; //- TableOffsety;
        TrackedPointWRTVR[2, 0] = -0.01066f; TrackedPointWRTVR[2, 1] = 0.00650f; TrackedPointWRTVR[2, 2] = -0.09922f; TrackedPointWRTVR[2, 3] = -0.09420f; //- TableOffsetz;
        TrackedPointWRTVR[3, 0] = 0f; TrackedPointWRTVR[3, 1] = 0f; TrackedPointWRTVR[3, 2] = 0f; TrackedPointWRTVR[3, 3] = 1f;

        //Data got from the robot 

        TrackedPointWRTRobot[0, 0] = -0.08960f; TrackedPointWRTRobot[0, 1] = -0.94254f; TrackedPointWRTRobot[0, 2] = 0.312184f; TrackedPointWRTRobot[0, 3] = 0.78926f;
        TrackedPointWRTRobot[1, 0] = -0.99594f; TrackedPointWRTRobot[1, 1] = 0.08186f; TrackedPointWRTRobot[1, 2] = -0.03754f; TrackedPointWRTRobot[1, 3] = -0.06808f;
        TrackedPointWRTRobot[2, 0] = 0.00903f; TrackedPointWRTRobot[2, 1] = -0.32390f; TrackedPointWRTRobot[2, 2] = -0.94605f; TrackedPointWRTRobot[2, 3] = 0.28854f;
        TrackedPointWRTRobot[3, 0] = 0.0f; TrackedPointWRTRobot[3, 1] = 0f; TrackedPointWRTRobot[3, 2] = 0f; TrackedPointWRTRobot[3, 3] = 1f;
        /**
         * 
         * **/
        VRWRTRobot = TrackedPointWRTRobot * TrackedPointWRTVR.inverse;

        //subtract the table offsets from the final transform
        //VRWRTRobot[0, 3] = VRWRTRobot[0, 3] - TableOffsetx;
        //VRWRTRobot[1, 3] = VRWRTRobot[0, 3] - TableOffsety;
        //VRWRTRobot[2, 3] = VRWRTRobot[0, 3] - TableOffsetz;

        //VRWRTRobot = TrackedPointWRTVR.inverse * TrackedPointWRTRobot;


        return VRWRTRobot;
    }

    Matrix4x4 HomePos()
    {
        Home[0, 0] = 0.98208f; Home[0, 1] = -0.18160f; Home[0, 2] = 0.05046f; Home[0, 3] = -0.03152f;
        Home[1, 0] = 0.12616f; Home[1, 1] = 0.43447f; Home[1, 2] = -0.89181f; Home[1, 3] = 0.69367f;
        Home[2, 0] = 0.14002f; Home[2, 1] = 0.88219f; Home[2, 2] = 0.44960f; Home[2, 3] = -0.51877f;
        Home[3, 0] = 0f; Home[3, 1] = 0f; Home[3, 2] = 0f; Home[3, 3] = 1.0f;


        return Home;
    }

    public Vector3 Ros2Unity(Vector3 vector3)
    {
        Vector3 pos = new Vector3(-vector3.y, vector3.z, vector3.x);
        return pos;
    }

    public Vector3 Unity2Rosold(Vector3 vector3)
    {
        Vector3 pos = new Vector3(-vector3.y, -1 * (vector3.x + 0.15f), vector3.z);
        return pos;
    }

    public Vector3 Unity2Ros(Vector3 vector3)
    {
        Vector3 pos = new Vector3(-vector3.y, -1 * (vector3.x + 0.15f), vector3.z);
        return pos;
    }


    public Vector3 Rot(Vector3 vector3)
    {
        /*Returns Vector3 rotation in radians*/
        Vector3 rot = new Vector3(vector3.x * PI / 180, vector3.y * PI / 180, vector3.z * PI / 180);
        return rot;

    }

    public float Velscalerold(Vector3 pos)
    {
        Vector3 pos2 = Workspaceplane.transform.position;
        float yplanedivider = pos2.y;
        //float velfactor;
        if (pos.y >= yplanedivider)
        { //we are in the ouside zone 
            velfactor = 0.15f;
        }
        else
            velfactor = 0.55f;
        Debug.Log("yplane coord"+ yplanedivider);
        Debug.Log("object y coord" + pos.y);
        return velfactor;
    }
    public float Velscaler(Vector3 pos)
    {
        //Vector3 pos2 = Workspaceplane.transform.position;
        //float yplanedivider = pos2.y;
        float velfactor;
        velfactor = 0.15f;
        //if (pos.y >= 0.55f)
       // { //we are in the ouside zone 
            velfactor = 0.15f;
      //  }
       // else
          //  velfactor = 0.55f;
        //Debug.Log("yplane coord" + yplanedivider);
        //Debug.Log("object y coord" + pos.y);
        return velfactor;
    }



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
        if (p.x >= xplanedivider)
        {
            wspace = 0.3f;
            if (((xl <= p.x) && (p.x <= xu)) && ((yl <= p.y) && (p.y <= yu)))
            {
                wspace = 0.2f;
            }
        }
        else wspace = 0.7f;
        return wspace;
    }

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
}