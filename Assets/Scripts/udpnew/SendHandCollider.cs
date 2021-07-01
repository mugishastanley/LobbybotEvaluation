using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

//using Renci.SshNet;

/*
    -----------------------
    UDP-Send: Only when collision occurs and object is collected. 
    -----------------------
*/

public class SendHandCollider : MonoBehaviour
{
    //[SerializeField]
    //private GameObject Tosend;
    private static int localPort;

    // prefs
    private string IP;  // define in init
    private int port;  // define in init

    // "connection" things
    IPEndPoint remoteEndPoint;
    UdpClient client;
    string pos;
    // gui
    string strMessage = "";

    // call it from shell (as program)
    private static void Main()
    {
        SendHandCollider sendObj = new SendHandCollider();
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

        /***
    void OnGUI()
    {
        

        Rect rectObj = new Rect(40, 380, 200, 400);
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        GUI.Box(rectObj, "# UDPSend-Data\n127.0.0.1 " + port + " #\n"
                    + "shell> nc -lu 127.0.0.1  " + port + " \n"
                    + pos
                , style);

        // ------------------------
        // send it
        // ------------------------

        strMessage = GUI.TextField(new Rect(40, 420, 140, 20), pos);

        // if (GUI.Button(new Rect(190, 420, 40, 20), "send"))
        // {
        sendString(strMessage + "\n");
        // print("Testing: nc -lu " + IP + " : " + pos);

        // }
    }
    **/

    // init
    public void init()
    {
        // Endpunkt definieren, von dem die Nachrichten gesendet werden.
        print("UDPSend.init()");

        // define

        //IP = "127.0.0.1";
        IP = "172.16.0.6";
        port = 11000;

        // ----------------------------
        // Send
        // ----------------------------
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        client = new UdpClient();
        // status
        // print("Sending to " + IP + " : " + port);
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

    public Vector3 Ros2Unity(Vector3 vector3)
    {
        return new Vector3(-vector3.y, vector3.z, vector3.x);
    }

    private float _speed = 0.15f;

    float offsetx = -0.0808f;
    float offsety = -0.1650f;
    float offsetz = 1.0585f;

    //public static int scorecount = 0;
    Vector3 collpos;
    Vector3 tosend;
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Apple")
        {
            collpos = transform.position;
            tosend = Ros2Unity(collpos);
            tosend.x += offsetx;
            tosend.y += offsety;
            tosend.z += offsetz;


            //tosend.x = collpos.y - 0.1548f;
            //tosend.y = collpos.z - 0.2733f;
            //tosend.z = collpos.x - 0.1053f;



            string pos = tosend.ToString("F4");
            Debug.Log("Collided with Hand at "+pos);
            sendString(pos);
         }
    }

  
}