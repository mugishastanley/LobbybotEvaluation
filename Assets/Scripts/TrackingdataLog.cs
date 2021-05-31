using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using System.IO;


public class TrackingdataLog : MonoBehaviour
{
    [Header("Data")]
    string serializedData;
    private string RightControllerPos = @"c:\temp\RightContPos.txt";
    private string handPos = @"c:\temp\HandPos.txt";
    private string RightControllerRot = @"c:\temp\RightContRot.txt";
    private string RightControllerVel = @"c:\temp\RightContVel.txt";
    private string RightControllerAcc = @"c:\temp\RightContAcc.txt";
    private string HeadRotation = @"c:\temp\HeadRot.txt";
    private string HeadRotationEuler = @"c:\temp\HeadRotEuler.txt";
    private string HeadPosition = @"c:\temp\HeadPos.txt";
    private string EETransform = @"c:\temp\EETransform.txt";
    private string Sentdata = @"c:\temp\SentDataTransform.txt";

    public GameObject RobotEndEffector;
    public GameObject Hand;

   

    public void Start()
    {
        inDevices();
    }

    // Once you complete this module, we'll keep your Update function active
    // to drive the map display
    void Update()
    {
        //trackerinfo();
        matrixsent();
        //trackingdata();
    }

    private void matrixsent() {
        //Matrix4x4 sent = FindObjectOfType<VelUDP2>().sentdata;
        using (StreamWriter sw = File.AppendText(Sentdata))
        {
            sw.WriteLine(System.DateTime.Now + "," + Time.time + ", "+ FindObjectOfType<VelUDP2>().sentdata);//time in seconds since start of game
                                                                                                 //sw.WriteLine("Extra line");                      
        }
    }


    private void trackerinfo()
    {
        /*Gets information of devices available */
        List<XRNodeState> nodeStates = new List<XRNodeState>();
        UnityEngine.XR.InputTracking.GetNodeStates(nodeStates);

        //get the state of each node
        foreach (XRNodeState nodestate in nodeStates)
        {
            Vector3 right_vel, right_pos, head_pos, right_acc;
            Quaternion head_rot, right_rot;
            Matrix4x4 Tmat;

            if (nodestate.nodeType == XRNode.RightHand)
            {
                //get pos of the right hand
                if (nodestate.TryGetPosition(out right_pos))
                {
                    //Debug.Log("Right cont Pos is" + right_pos.ToString("F4"));
                        using (StreamWriter sw = File.AppendText(RightControllerPos))
                    {
                        sw.WriteLine(System.DateTime.Now + "," + Time.time + "," + right_pos.ToString("F4"));//time in seconds since start of game
                        //sw.WriteLine("Extra line");                      
                    }
                }
                //get velocity of the right hand
                if (nodestate.TryGetVelocity(out right_vel))
                {
                    using (StreamWriter sw = File.AppendText(RightControllerVel))
                    {
                        sw.WriteLine(System.DateTime.Now + "," + Time.time + "," + right_vel.ToString("F4"));
                        //sw.WriteLine("Extra line");                      
                    }
                }
                //get acc
                if (nodestate.TryGetAcceleration(out right_acc))
                {
                    //Debug.Log("Right cont ACC is" + right_pos.ToString("F4"));
                    using (StreamWriter sw = File.AppendText(RightControllerAcc))
                    {
                        sw.WriteLine(System.DateTime.Now + "," + Time.time + "," + right_acc.ToString("F4"));//time in seconds since start of game
                        //sw.WriteLine("Extra line");                      
                    }


                }

                if (nodestate.TryGetRotation(out right_rot))
                {
                    using (StreamWriter sw = File.AppendText(RightControllerRot))
                    {
                        sw.WriteLine(System.DateTime.Now + "," + Time.time + "," + right_rot.ToString("F4"));
                        //sw.WriteLine("Extra line");                      
                    }
                }
            }
            //get rotation and pos of the head
            if (nodestate.nodeType == XRNode.Head)
            {
                if (nodestate.TryGetRotation(out head_rot))
                {
                    using (StreamWriter sw3 = File.AppendText(HeadRotation))
                    {
                        sw3.WriteLine(System.DateTime.Now + "," + Time.time + "," + head_rot.ToString("F4"));
                        //sw.WriteLine("Extra line");                      
                    }
                }

                if (nodestate.TryGetRotation(out Quaternion headroteuler))
                {
                    using (StreamWriter sw3 = File.AppendText(HeadRotationEuler))
                    {
                        sw3.WriteLine(System.DateTime.Now + "," + Time.time + "," + headroteuler.eulerAngles.ToString("F4"));
                        //sw.WriteLine("Extra line");                      
                    }
                }

                if (nodestate.TryGetPosition(out head_pos))
                {
                    using (StreamWriter sw3 = File.AppendText(HeadPosition))
                    {
                        sw3.WriteLine(System.DateTime.Now + "," + Time.time + "," + head_pos.ToString("F4"));
                        //sw.WriteLine("Extra line");                      
                    }
                }

                

            }

            //get transform matrix of End Effector
            //Tmat = RobotEndEffector.transform.localToWorldMatrix;
            {
                using (StreamWriter sw3 = File.AppendText(EETransform))
                {
                    sw3.WriteLine(System.DateTime.Now + "," + Time.time + "," + RobotEndEffector.transform.position.ToString("F4"));
                   // Debug.Log("Robot EE" + Tmat.ToString());
                }
                using (StreamWriter sw3 = File.AppendText(handPos))
                {
                    sw3.WriteLine(System.DateTime.Now + "," + Time.time + "," + Hand.transform.position.ToString("F4"));
                    // Debug.Log("Robot EE" + Tmat.ToString());
                }
            }
        }

        void Statsinfo()
        {
            int framect = 1;
            XRStats.TryGetFramePresentCount(out framect);

        }
        //Debug.Log("Delta time duration " + Time.deltaTime);


    }

    private void trackingdata()
    {
        //to be called every update
        //works fine as at 3/6/2020 6:02
        /** same functionality as tracker info but less reliable 
         * because it will still get data when tracked is not ready or off **/
       Vector3 pos = InputTracking.GetLocalPosition(XRNode.RightHand);
       Quaternion rot = InputTracking.GetLocalRotation(XRNode.RightHand);
       Vector3 headrot = InputTracking.GetLocalRotation(XRNode.Head).eulerAngles;
       Debug.Log("From trackingdata rght Position" + pos);
    }

    void inDevices()
    {
        //works fine as at 3/6/2020 6:02
        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(inputDevices);
        foreach (var device in inputDevices)
        {
            Debug.Log(string.Format("Device found with name '{0}' and role '{1}'",
                      device.name, device.role.ToString()));

        }
    }
}