using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;


public class SendCollision : MonoBehaviour
{
    private static int localPort;

// private FallingSpheres _SphereFall;

    // prefs
    private string IP;  // define in init
    public int port;  // define in init

    // "connection" things
    IPEndPoint remoteEndPoint;
    UdpClient client;

    // gui
    string strMessage = "";

    //string player = ;
    //string player = "apple"

    //set some bounds provided by the robot;
    private const float _xMin = 0f;
    private const float _xMax = 0.8f;
    private const float _yMin = -0.8f;
    private const float _yMax = 0.8f;
    private const float _zMin = 0f;
    private const float _zMax = 1f;

    string rot = "";

    //some conversions
    private float _x, _y, _z, _xrot, _yrot, _zrot;

    // call it from shell (as program)
    private static void Main()
    {
        UDPSendImproved2 sendObj = new UDPSendImproved2();
        sendObj.init();

    }
    // start from unity3d
    public void Start()
    {
        init();
    }


    /*coordinates conversion by cyclic swap
     * x->y
     * y->z
     * z->x
     */
    static Vector3 convertcoords(ref float x,
                   ref float y, ref float z)
    {

        // Before overwriting b, store  
        // its value in temp. 
        float temp = y;

        // Now do required swapping  
        // starting with y. 
        y = x;
        x = z;
        z = temp;
        Vector3 coords = new Vector3(x, y, z);
        return coords;
    }

     // OnGUI
    void OnGUI()
    {
        //string pos = GameObject.Find("apple").transform.position.ToString();
        //convert  from euler angles to degrees
        //FallingSpheres sphere = FallingSpheres.transform.GetComponent<FallingSpheres>()        

        //Cap coordinates to bounds 
        float xPos = Mathf.Clamp(_x, _yMin, _yMax);
        float yPos = Mathf.Clamp(_y, _zMin, _zMax);
        float zPos = Mathf.Clamp(_z, _xMin, _xMax);


        string pos = convertcoords(ref xPos, ref yPos, ref zPos).ToString();

        Rect rectObj = new Rect(40, 380, 200, 400);
        /**
         * GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        GUI.Box(rectObj, "# UDPSend-Data" + port + " #\n"
                    + " \n"
                    + pos +" "+rot
                , style); 
        */

        //removed gui style

        GUI.Box(rectObj, "# UDPSend-Data" + port + " #\n"
                    + " \n"
                    + pos
                );

        // ------------------------
        // send it
        // ------------------------

        strMessage = GUI.TextField(new Rect(40, 420, 140, 20), pos);

        sendString(strMessage + "\n");

    }

    // init
    public void init()
    {
        print("UDPSend.init()");
        IP = "130.251.243.231";
        port = 11000;

        // ----------------------------
        // Send
        // ----------------------------
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        client = new UdpClient();

        // status
        print("Sending to " + IP + " : " + port);
        print("Testing: nc -lu " + IP + " : " + port);



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
                // Send the text to the remote client.
                if (text != "")
                {

                    //Encoding data with UTF8 encoding into binary format.
                    byte[] data = Encoding.UTF8.GetBytes(text);

                    // Send the text to the remote client.
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
            // Encoding data with UTF8 encoding into binary format.
            byte[] data = Encoding.UTF8.GetBytes(message);

            // Sned msg to remote client
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
}
