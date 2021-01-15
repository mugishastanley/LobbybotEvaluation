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

public class UDPSendImproved2 : MonoBehaviour
{
    [SerializeField]
    private GameObject Test;

    //[SerializeField]

    private GameObject Tosend;
    private static int localPort;
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

    // call it from shell (as program)
    private static void Main()
    {
        UDPSendImproved2 sendObj = new UDPSendImproved2();
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
        //string pos = Tosend.transform.position.ToString("F5");
        //string rot = Tosend.transform.rotation.ToString("F5");
        //string rot = Tosend.transform.localEulerAngles.ToString("F5");
        //string datasent = pos + ',' + rot;
        string datasent = (Calculate_Transform() * Test.transform.localToWorldMatrix).ToString("F8"); 

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

    //stub to test transform matrix
    Matrix4x4 mat_to_vec()
    {
        Matrix4x4 m= new Matrix4x4();
        m[0, 0] = -0.93928045f;
        m[0, 1] = 0.33122116f;
        m[0, 2] = 0.08969279f;
        m[0, 3] = -0.77373964f;
        m[1, 0] = 0.08146696f;
        m[1, 1] = -0.03866611f;
        m[1, 2] = 0.99592572f;
        m[1, 3] = 0.06621116f;
        m[2, 0] = 0.33333975f;
        m[2, 1] = 0.94276059f;
        m[2, 2] = 0.00933475f;
        m[2, 3] = 0.32289779f;
        m[3, 0] = 0f;
        m[3, 1] = 0f;
        m[3, 2] = 0f;
        m[3, 3] = 1f;
        Test.transform.rotation = Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
        Test.transform.position = m.GetColumn(3);
        return m;
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
        TrackedPointWRTVR[0, 0] = 0.03396f; TrackedPointWRTVR[0, 1] = -0.09355f; TrackedPointWRTVR[0, 2] = -0.00977f; TrackedPointWRTVR[0, 3] = 0.28243f;
        TrackedPointWRTVR[1, 0] = -0.09345f; TrackedPointWRTVR[1, 1] = -0.03474f; TrackedPointWRTVR[1, 2] = 0.00776f; TrackedPointWRTVR[1, 3] = 0.94222f;
        TrackedPointWRTVR[2, 0] = -0.01066f; TrackedPointWRTVR[2, 1] = 0.00650f; TrackedPointWRTVR[2, 2] = -0.09922f; TrackedPointWRTVR[2, 3] = -0.09420f;
        TrackedPointWRTVR[3, 0] = 0f; TrackedPointWRTVR[3, 1] = 0f; TrackedPointWRTVR[3, 2] = 0f; TrackedPointWRTVR[3, 3] = 1f;


        TrackedPointWRTRobot[0, 0] = -0.08960f; TrackedPointWRTRobot[0, 1] = -0.94254f; TrackedPointWRTRobot[0, 2] = 0.312184f; TrackedPointWRTRobot[0, 3] = 0.78926f;
        TrackedPointWRTRobot[1, 0] = -0.99594f; TrackedPointWRTRobot[1, 1] = 0.08186f; TrackedPointWRTRobot[1, 2] = -0.03754f; TrackedPointWRTRobot[1, 3] = -0.06808f;
        TrackedPointWRTRobot[2, 0] = 0.00903f; TrackedPointWRTRobot[2, 1] = -0.32390f; TrackedPointWRTRobot[2, 2] = -0.94605f; TrackedPointWRTRobot[2, 3] = 0.28854f;
        TrackedPointWRTRobot[3, 0] = 0.0f; TrackedPointWRTRobot[3, 1] = 0f; TrackedPointWRTRobot[3, 2] = 0f; TrackedPointWRTRobot[3, 3] = 1f;
        /**
         * 
         * **/
        VRWRTRobot = TrackedPointWRTRobot * TrackedPointWRTVR.inverse;
        //VRWRTRobot = TrackedPointWRTVR.inverse * TrackedPointWRTRobot;


        return VRWRTRobot;
    }
}