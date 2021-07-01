using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

/*
 
    -----------------------
    UDP-Receive (send to)
    -----------------------
    // [url]http://msdn.microsoft.com/de-de/library/bb979228.aspx#ID0E3BAC[/url]
   
   
    // > receive
    // 127.0.0.1 : 8051
   
    // send
    // nc -u 127.0.0.1 8051
 
*/


public class UDPReceive : MonoBehaviour
{

    // receiving Thread
    Thread receiveThread;

    // udpclient object
    UdpClient client;

    public string IP = "10.186.15.58";// default local
    public int port; // define > init

    // infos
    public string lastReceivedUDPPacket = "";
    public string allReceivedUDPPackets = ""; // clean up this from time to time!


    // start from shell
    private static void Main()
    {
        UDPReceive receiveObj = new UDPReceive();
        receiveObj.init();

        string text = "";
        do
        {
            text = Console.ReadLine();
        }
        while (!text.Equals("exit"));
    }
    // start from unity3d
    public void Start()
    {

        init();
    }

    // OnGUI
    void OnGUI()
    {
        Rect rectObj = new Rect(40, 10, 200, 400);
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        GUI.Box(rectObj, "# UDPReceive\n127.0.0.1 " + port + " #\n"
                    + "shell> nc -u 127.0.0.1 : " + port + " \n"
                    + "\nLast Packet: \n" + lastReceivedUDPPacket
                    + "\n\nAll Messages: \n" + allReceivedUDPPackets
                , style);
    }

    // init
    private void init()
    {
        // Endpunkt definieren, von dem die Nachrichten gesendet werden.
        print("UDPSend.init()");

        // define port
        port = 11001;

        // status
        print("Sending to 127.0.0.1 : " + port);
        print("Test-Sending to this Port: nc -u 127.0.0.1  " + port + "");


        // ----------------------------
        // Abhören
        // ----------------------------
        // Lokalen Endpunkt definieren (wo Nachrichten empfangen werden).
        // Einen neuen Thread für den Empfang eingehender Nachrichten erstellen.
        receiveThread = new Thread(
            new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

    }

    // receive thread
    private void ReceiveData()
    {

        client = new UdpClient(port);
        while (true)
        {
            try
            {

                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);


                string text = Encoding.UTF8.GetString(data);
                //string text = Encoding.ASCII.GetString(data);


                print(">> " + text);
                print (" Poses "+ Data_to_vec(text));

                // latest UDPpacket
                // latest UDPpacket
                lastReceivedUDPPacket = text;

                // ....
                allReceivedUDPPackets = allReceivedUDPPackets + text;

            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    // getLatestUDPPacket
    // cleans up the rest
    public string getLatestUDPPacket()
    {
        allReceivedUDPPackets = "";
        return lastReceivedUDPPacket;
    }

    //put data into a vector3

    Vector3 Data_to_vec(string Data) {
        string NewData = Data.Replace("[", "").Replace("]", "");
        char[] charSeparators = new char[] {' '};
        string[] result;
        result = NewData.Split(charSeparators, StringSplitOptions.None);
        //print(result[0] + " . " + result[1] + ". " + result[2] + ". " + result[3]);
        Vector3 Pos = new Vector3(
        float.Parse(result[0]),
        float.Parse(result[2]),
        float.Parse(result[3]));
        // print("length of processed"+ words.Length);
        // print("processed"+x);
        return Pos;
    }

}