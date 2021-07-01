using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;




public class UDPManager : MonoBehaviour
{
    static UdpClient udp;
    Thread thread;

    public GameObject cube;
    //public CubeMove cubemove;
    private CubeMove cubemove;

    static readonly object lockObject = new object();
    string returnData = "Hiii";
    bool precessData = false;

    void Start()
    {
        cubemove = cube.GetComponent<CubeMove>();
        thread = new Thread(new ThreadStart(ThreadMethod));
        thread.Start();
    }

    void Update()
    {
        if (precessData)
        {
            /*lock object to make sure there data is 
             *not being accessed from multiple threads at thesame time*/
            lock (lockObject)
            {
                precessData = false;
                //cube.SendMessage("Move");
                // or
                cubemove.Move();

                //Process received data
                Debug.Log("Received: " + returnData);

                //Reset it for next read(OPTIONAL)
                returnData = "";
            }
        }
    }

    private void ThreadMethod()
    {
        udp = new UdpClient(6000);
        while (true)
        {
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            byte[] receiveBytes = udp.Receive(ref RemoteIpEndPoint);

            /*lock object to make sure there data is 
            *not being accessed from multiple threads at thesame time*/
            lock (lockObject)
            {
                returnData = Encoding.ASCII.GetString(receiveBytes);

                Debug.Log("Recieved from python"+returnData);
                if (returnData == "1\n")
                {
                    //Done, notify the Update function
                    precessData = true;
                }
            }
        }
    }


}
