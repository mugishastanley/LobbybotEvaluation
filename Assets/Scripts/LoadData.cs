using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class LoadData : MonoBehaviour
{
    [Header("Data")]
    private string RightControllerPos = @"c:\temp\RightContPos.txt";
    //private string RightControllerPos = @"c:\temp\HandPos.txt";
    private string handPos = @"c:\temp\HandPos.txt";
    private string RightControllerRot = @"c:\temp\RightContRot.txt";
    private string RightControllerVel = @"c:\temp\RightContVel.txt";
    private string RightControllerAcc = @"c:\temp\RightContAcc.txt";
    private string HeadRotation = @"c:\temp\HeadRot.txt";
    private string HeadRotationEuler = @"c:\temp\HeadRotEuler.txt";
    private string HeadPosition = @"c:\temp\HeadPos.txt";
    private int _linecounter;
    
    private List<Vector3> _handposition;
    private List<Vector3> _headpostion;
    private List<Quaternion> _headrotation;
    // Start is called before the first frame update
    public TextAsset textFile;     // drop your file here in inspector
    public Transform Hand;
    public Transform Head;
    public bool isDone  { get; set; }
    public bool startrecord{ get; set; }

    public Vector3 Handpos { get; set; }

    void Start()
    {
        //string text = textFile.text;  //this is the content as string
        _handposition= new List<Vector3>();
        _headpostion= new List<Vector3>();
        _headrotation= new List<Quaternion>();
        
        readTextFileVector3(RightControllerPos,_handposition);
        readTextFileVector3(HeadPosition,_headpostion);
        readTextFileVector4(HeadRotation,_headrotation);

        startrecord = false;
        isDone = false;
    }

    // Update is called once per frame
    
    
   /**
    private void FixedUpdate()
    {
        if (!isDone)
        {
            StartCoroutine(Delaymotion(Hand, _handposition, Head, _headpostion, _headrotation));
            //runImmediately(Hand, _handposition, Head, _headpostion, _headrotation);
            isDone = true;
        }
    }
    
   **/

    public void runload()
    {
        startrecord = true;
        if (!isDone)
        {
            //StartCoroutine(Delaymotion(Hand, _handposition, Head, _headpostion, _headrotation));
            //runImmediately(Hand, _handposition, Head, _headpostion, _headrotation);
            isDone = true;
        }
        
    }
    
    

    void readTextFileVector3(string file_path, List<Vector3> structure)
    {
        //Mthd 2
        StreamReader inp_stm = new StreamReader(file_path);
        while(!inp_stm.EndOfStream)
        {
            
            string inp_ln = inp_stm.ReadLine( );
            // Extract everything between brackets
            Regex regex = new Regex(@"\(.*?\)");
            MatchCollection matches = regex.Matches(inp_ln);
            //remove brackets
            var tr = matches[0].ToString();
            var result = tr.Trim('(', ')');
            var sStrings = result.Split(","[0]);
            float x = float.Parse(sStrings[0]);
            float y = float.Parse(sStrings[1]);
            float z = float.Parse(sStrings[2]);
            Vector3 pos = new Vector3(x, y, z);
            
            structure.Add(pos);
           // Debug.Log("Row :"+" x:"+x+" Y:"+y+" z:"+z);
           //Debug.Log("Pos is:"+pos);
        }
        inp_stm.Close( );  
        Debug.Log("positions loaded is:");
    }
    
    void readTextFileVector4(string file_path, List<Quaternion> structure)
    {
        //Mthd 2
        StreamReader inp_stm = new StreamReader(file_path);
        while(!inp_stm.EndOfStream)
        {
            
            string inp_ln = inp_stm.ReadLine( );
            // Extract everything between brackets
            Regex regex = new Regex(@"\(.*?\)");
            MatchCollection matches = regex.Matches(inp_ln);
            //remove brackets
            var tr = matches[0].ToString();
            var result = tr.Trim('(', ')');
            var sStrings = result.Split(","[0]);
            float x = float.Parse(sStrings[0]);
            float y = float.Parse(sStrings[1]);
            float z = float.Parse(sStrings[2]);
            float w = float.Parse(sStrings[3]);
            Quaternion rot = new Quaternion(x, y, z,w);
            structure.Add(rot);
            // Debug.Log("Row :"+" x:"+x+" Y:"+y+" z:"+z);
            //Debug.Log("Pos is:"+rot);
        }
        inp_stm.Close( );  
        Debug.Log("rotations loaded is:");
    }

   IEnumerator Delaymotion(Transform hand, List<Vector3> handpos, Transform head, List<Vector3> headpos, List<Quaternion> headrot)
   {
       var counter = 0;
       var size = handpos.Capacity-1;
       //Debug.Log("size is "+size);
       while (counter < size)
       {
          // Debug.Log("counter is "+counter);
           hand.position = handpos[counter];
           head.position = headpos[counter];
           head.rotation = headrot[counter];
           Handpos = hand.position;
           counter++;
           //tt.position = enumerator.Current;
           yield return new WaitForSecondsRealtime(0.022f);
           
       }
       Debug.Log("Motion finished");
   }


   void runImmediately(Transform hand, List<Vector3> handpos, Transform head, List<Vector3> headpos,
       List<Quaternion> headrot)
   {
       var counter = 0;
       var size = handpos.Capacity-1;
       Debug.Log("size is "+size);
       while (counter < size)
       {
           // Debug.Log("counter is "+counter);
           hand.position = handpos[counter];
           head.position = headpos[counter];
           head.rotation = headrot[counter];
           Handpos = hand.position;
           counter++;
           //tt.position = enumerator.Current;
           //yield return new WaitForSecondsRealtime(0.02f);
           
       }
       Debug.Log("Motion finished");
   }
}
