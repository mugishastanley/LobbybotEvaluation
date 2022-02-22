using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine.Serialization;
using Valve.VR;

public class KdFindClosest : MonoBehaviour
{
    public Button initialiseButton;
    public GameObject whitePrefab;
    public GameObject blackPrefab;
    public GameObject plane;
    public bool projectiononPlane;
    public Transform calTracker;
    public Camera cam;
    //public GameObject qq;

    //public GameObject Safepoint;
    private float _velinside = 0.25f;
    private float _veloutside =0.6f;
    private string _algoresults = @"c:\temp\Results.csv";

    
    [SerializeField]
    private GameObject[] points;
    [SerializeField]
    private GameObject[] safepoints;
    [FormerlySerializedAs("_isnearestfound")] [SerializeField]
    //private GameObject _safepointparent;
    private Vector3 _gazevector;
    private bool isnearestfound = false;


    //private Vector3 Nearobpos;
    //private Vector3 _nearoblocalpose;
    //private Quaternion Nearobrot;
    private float _d1, _d2, _d3, _d4;
    private float _timeThroughplane, _timeInside;
    public string Idtoros { get; set; }
    
    
    private const float _threshold=0.2f;
    private const float _tol = 0.3f;
    private const float _lambda = 0.2f; 

    Collider _objCollider;
    Plane[] _planes;
    Renderer[] _renderers;
    GameObject _pts;
    Vector3 _prevPos;

    protected KdTree<SpawnedPoint> PointsInCar = new KdTree<SpawnedPoint>();
    protected KdTree<SpawnedPoint> Hands = new KdTree<SpawnedPoint>();
    protected List<SpawnedPoint> Safepoints = new List<SpawnedPoint>();
    protected List<SpawnedPoint> PointsInCar2 = new List<SpawnedPoint>();

    private SpawnedPoint _first;
    private SpawnedPoint _second;
    private SpawnedPoint _nearestObj;
    private float _handtime;
    //private Dictionary<int,Vector3> points_in_space;


    public Vector3 Nearobpos { get; private set; }
    public Quaternion Nearobrot { get; private set; }
    public Vector3 Startpose  { get; private set; }
    public Quaternion Startrot  { get; private set; }

    
    
    public Vector3 Colorpose { get; set; }
    private Vector3 oldvel;


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
    private List<float> _time;
    // Start is called before the first frame update
    //public TextAsset textFile;     // drop your file here in inspector
    private Transform Hand;
    private Transform Head;
    private bool isDone;
    private int _stop;

    private SpawnedPoint  _first_2;
    private SpawnedPoint _first_3;
    private SpawnedPoint  _first_4;
    private SpawnedPoint  _first_43;
    private SpawnedPoint _first_52;
    private SpawnedPoint  _first_5;
    private SpawnedPoint  _first_6;

    public Vector3 Handpos { get; set; }


    private void Awake()
    {
        init_from_sim();
        Init();
    }

    void Start()
    {
        //runImmediately(Hand, _handposition, _headpostion, _headrotation,_time);
    }

    void init_from_sim()
    {
        isDone = true;
        //string text = textFile.text;  //this is the content as string
        _handposition= new List<Vector3>();
        _headpostion= new List<Vector3>();
        _headrotation= new List<Quaternion>();
        _time = new List<float>();
        _handtime = 0.0f;
        _gazevector = new Vector3(0, 0, 0);
       
        
        readhandTextFileVector3(RightControllerPos,_handposition, _time);
        readTextFileVector3(HeadPosition,_headpostion);
        readTextFileVector4(HeadRotation,_headrotation);
        
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
      //  Debug.Log("positions loaded:");
    }
    
    
    void readhandTextFileVector3(string file_path, List<Vector3> structure, List<float> time)
    {
        //Mthd 2
        StreamReader inp_stm = new StreamReader(file_path);
        while(!inp_stm.EndOfStream)
        {
            string inp_ln = inp_stm.ReadLine( );
            
            // Extract everything between comas
            Regex regex2 = new Regex(@"\,.*?\,");
            MatchCollection tt = regex2.Matches(inp_ln);
            //remove comas
            var tr0 = tt[0].ToString();
            var res = tr0.Trim(',', ',');
            //Debug.Log("time is "+ res);
            time.Add(float.Parse(res));

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
            //Debug.Log("Row :"+" x:"+x+" Y:"+y+" z:"+z);
            //Debug.Log("Time is "+tr0+" Pos is:"+pos);
        }
        inp_stm.Close( );  
        //  Debug.Log("positions loaded:");
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
        //("rotations loaded is:");
    }
    
    

    public void Init()
    {
        PointsInCar.UpdatePositions();
        oldvel = new Vector3();
        //_pointtoxyz = new Dictionary<int, float>();
        
        //cam = Camera.main;
        
        //initialise the safe points in a list
        /***
        for (int i=0; i< _safepointparent.transform.childCount;i++)
        {
            GameObject point = (Instantiate(BlackPrefab, _safepointparent.transform.GetChild(i).position, 
                _safepointparent.transform.GetChild(i).rotation, CalTracker.transform));
            Safepoints.Add((point).GetComponent<SpawnedPoint>());
        }
        ***/
        
        //initialise the points of interest  
        for (int i = 0; i < points.Length; i++)
        {
            var num = i + 1;
            GameObject point = (Instantiate(blackPrefab, points[i].transform.position, 
                points[i].transform.rotation,calTracker.transform));
            point.GetComponent<SpawnedPoint>().Id = num.ToString();
            PointsInCar.Add((point).GetComponent<SpawnedPoint>());
            Debug.Log("original Point  :"+i + points[i].transform.position.ToString("F4") + " localpos " +
                      points[i].transform.localPosition.ToString("F4"));
            //Debug.Log("Spawned Point at :"+i+point.transform.position.ToString("F4")+" localpos "+point.transform.localPosition.ToString("F4"));
            //Debug.Log("points " +i +"position"+ points[i].transform.position.ToString("F4"));
            //pointtoxyz.Add(points[i].transform.position, i+1);
            //_pointtoxyz.Add(i+1, point.GetComponent<SpawnedPoint>().Id);
            //writes points to results
            //write_result2(i+1,  point.GetComponent<SpawnedPoint>().Id);
            //points_in_space.Add(i+1,points[i].transform.position);

            
        }
        
        for (int i = 0; i < safepoints.Length; i++)
        {
            
            var num=i+20;
            //initialise the points of interest  
            GameObject point = (Instantiate(blackPrefab, safepoints[i].transform.position, 
                safepoints[i].transform.rotation,calTracker.transform));
            Safepoints.Add((point).GetComponent<SpawnedPoint>());
            
            //Debug.Log("safe points "+i +"position"+ safepoints[i].transform.position.ToString("F4"));
            /*Mechanically changing safe points**/
            //point.GetComponent<SpawnedPoint>().Id = "SP"+num;
            point.GetComponent<SpawnedPoint>().Id = num.ToString();
            //pointtoxyz.Add(safepoints[i].transform.position,20+i);
           // _pointtoxyz.Add( 20 + i,  point.GetComponent<SpawnedPoint>().Id);
            //writes safe points to results
           // write_result2(20+i,  point.GetComponent<SpawnedPoint>().Id);

        }

        
 
        
        
        /*
        //initialise the 
        for (int i = 0; i < points.Length; i++)
        {
            //initialise the points of interest  
            GameObject point = (Instantiate(BlackPrefab, points[i].transform.position, 
                points[i].transform.rotation, 
                CalTracker.transform));
            PointsInCar2.Add((point).GetComponent<SpawnedPoint>());
        }
    **/
        
        //initialise the number of hands
       // for (int i = 0; i < CountWhite; i++)
        {
            //Hands.Add(Instantiate(WhitePrefab).GetComponent<SpawnedPoint>());
            Hands.Add((whitePrefab).GetComponent<SpawnedPoint>());
        }
        

        //First point in the car should be the home position.
        //First = PointsInCar[0];
         _first = Safepoints[0];
        _second = PointsInCar[0];
        _first_2 = Safepoints[0];
        _first_3 = Safepoints[0];
        _first_4 = Safepoints[0];
        _first_43 = Safepoints[0];
        _first_5 = Safepoints[0];
        _first_52 = Safepoints[0];
        _first_6 = Safepoints[0];
        _nearestObj = PointsInCar[0];
        
        {
            Startpose =PointsInCar[points.Length-1].transform.localPosition;
            Startrot = PointsInCar[points.Length-1].transform.localRotation;
        }

        //runImmediately(Hand, _handposition, _headpostion, _headrotation);
        
    }
    
    public void ClickedButton()
    {
        isDone = false;
    }
    
    void runImmediately(Transform hand, List<Vector3> handpos, List<Vector3> headpos,
        List<Quaternion> headrot, List<float> time)
    {
        
        var counter = 0;
        var size = handpos.Capacity;
        while (counter < size)
        {
            // Debug.Log("counter is "+counter);
            
            hand.transform.position = handpos[counter];
            cam.transform.position = headpos[counter];
            cam.transform.rotation = headrot[counter];
            _handtime = time[counter];
            KdWithoutHead1();
            KdWithoutHead_threshold();
            WithHead3();
            WithHead_threshold4();
            //WithHead_threshold42();
            //WithHead_Homepose5();
            //WithHead_Homepose52();
            //WithHead_Handthreshold_Homepose6();
            //WithHead_Handthreshold_Homepose62();
            //TestHandvel_distfromray2(hand);
            //TestHandvel_distfromray3(hand);
            counter++;
        }
        Debug.Log("MyScript.Start " + GetInstanceID(), this);
    }
    
    IEnumerator RunDelay(GameObject hand, List<Vector3> handpos, List<Vector3> headpos,
        List<Quaternion> headrot, List<float> time)
    {
        
        var counter = 0;
        var size = handpos.Capacity;
        while (counter < size)
        {
            Debug.Log("counter is "+counter+ "Hand pos"+hand.transform.position);
            
            hand.transform.position = handpos[counter];
            cam.transform.position = headpos[counter];
            cam.transform.rotation = headrot[counter];
            _handtime = time[counter];
            //KdWithoutHead1();
            //KdWithoutHead_threshold();
            //WithHead3();
            //WithHead_threshold4();
            //WithHead_threshold42();
            //WithHead_Homepose5();
            //WithHead_Homepose52();
            //WithHead_Handthreshold_Homepose6();
            //WithHead_Handthreshold_Homepose62();
            //TestHandvel_distfromray2(hand);
            //TestHandvel_distfromray3(hand);
            counter++;
            yield return new WaitForSecondsRealtime(0.022f);
        }
        //Debug.Log("MyScript.Start " + GetInstanceID(), this);
    }


    // Update is called once per frame

    private void FixedUpdate()
    {
        if (!isDone)
        {
            //StartCoroutine(Delaymotion(Hand, _handposition, Head, _headpostion, _headrotation));
            //write_result(_handtime,  Nearobpos);
            
            //StartCoroutine runDelay(Hand, _handposition, _headpostion, _headrotation,);
            StartCoroutine(RunDelay(whitePrefab, _handposition, _headpostion, _headrotation,_time));
            isDone = true;
            //return;
        }
        //Debug.Log("Is done is"+isDone);
       
   //if (_stop <1)
       {
           //runImmediately(Hand, _handposition, _headpostion, _headrotation);
           
       }

      // Debug.Log("Time is "+ Time.realtimeSinceStartup);

        //if ((FindObjectOfType<LoadData>().startrecord))
        {
            //Debug.Log("Recording is "+FindObjectOfType<LoadData>().startrecord);
            //KdWithoutHead1();
            //KdWithoutHead_threshold();
            //WithHead3();
            //WithHead_threshold4();
            //WithHead_threshold42();
            //WithHead_Homepose5();
            //WithHead_Homepose52();
            //WithHead_Handthreshold_Homepose6();
            //WithHead_Handthreshold_Homepose62();
            //TestHandvel_distfromray2();
            //TestHandvel_distfromray3();
        }
        
        //KdWithoutHead();
        //KdWithoutHead_threshold();
        //WithHead2();
        //WithHead_threshold();
        //WithHead_Homepose2();
        //WithHead_Handthreshold_Homepose23();
        //TestHandvel_distfromray2();
        //TestHandvel_distfromray3();
        //Strategy(5);
    }
    
    
    

    void Strategy(int st)
    {
        switch (st)
        {
            case 1 :
                KdWithoutHead1();
                break;
            case 2 :
                KdWithoutHead_threshold();
                break;
            case 3 :
                WithHead3();
                break;
            case 4 :
                WithHead_threshold4();
                break;
            case 5 :
                WithHead_Homepose5();
                break;
            case  6:
                WithHead_Handthreshold_Homepose6();
                break;
            case  7:
                //TestHandvel_distfromray2();
                break;
            case  8:
                //TestHandvel_distfromray3();
                break;
            default:
                KdWithoutHead1();
                break;
        }
    }
    

    /**
    private int PosTopoint(Vector3 point)
    {
        //converts vector 3 to point.
        int pt=0;
        if (pointtoxyz.TryGetValue(point, out pt))
        {
            Debug.Log("found point "+ pt);
        }
        else
        {
            Debug.Log("Point not found" + pt + ":"+ point.ToString("F4"));
        }
        
        return pt;
    }
    **/


    private void write_result2(float time,string i,int stid)
    {
        //var pos=PosTopoint(point);
        using (StreamWriter sw = File.AppendText(@"c:\temp\Results"+stid+".csv"))
        {
            sw.WriteLine(time +","+i );
        }
    }
 
    private void KdWithoutHead1()
    {
        foreach (var hand in Hands)
        {
            SpawnedPoint nearestObj = PointsInCar.FindClosest(hand.transform.position);
            //Debug.Log("Object found at "+nearestObj.transform.position);
            Debug.DrawLine(hand.transform.position, nearestObj.transform.position, Color.red);
            write_result2(_handtime,  nearestObj.Id,1);
            
            int face = 0; 
            if (nearestObj.Id == "15")
            {
                face=FindObjectOfType<LineRenderSettings>().Facenum+3;
            }
            var objno = Int16.Parse(nearestObj.Id)+ face;
            Idtoros = objno.ToString();
            
            //Idtoros = nearestObj.Id;
            Nearobpos = nearestObj.transform.localPosition;
            Colorpose = nearestObj.transform.position;
            Nearobrot = nearestObj.transform.localRotation;
        }
    }

    private void KdWithoutHead_threshold()
    {
        
        //var prev = _first;
        //var nearestObj = _prev;
        foreach (var hand in Hands)
        {
            //estObj = _first;
            var nearestObj = PointsInCar.FindClosest(hand.transform.position);
            if (!(Vector3.Distance(nearestObj.transform.position, hand.transform.position) < _threshold))
            {
                nearestObj = _first_2;
            }
            Debug.DrawLine(hand.transform.position, nearestObj.transform.position, Color.red);
            Idtoros = nearestObj.Id;
            Nearobpos = nearestObj.transform.localPosition;
            Colorpose = nearestObj.transform.position;
            Nearobrot = nearestObj.transform.localRotation;
            write_result2(_handtime, nearestObj.Id,2);
            _first_2 = nearestObj;
            //_prev = nearestObj;
        }
    }

    private void KdWithoutHead_threshold2()
    {
        var nearestObj = _first;
        foreach (var hand in Hands)
        {
            //var nearestObj = _first;
            nearestObj = PointsInCar.FindClosest(hand.transform.position);
            if (Vector3.Distance(nearestObj.transform.position, hand.transform.position) < _threshold)
            {
                Debug.DrawLine(hand.transform.position, nearestObj.transform.position, Color.red);
                Nearobpos = nearestObj.transform.localPosition;
                Nearobrot = nearestObj.transform.localRotation;
            }
            write_result2(_handtime, nearestObj.Id,2);
        }
    }

    private void WithHead3()
    {
        //var _second = _first;
        foreach (var hand in Hands)
        {
            SpawnedPoint nearestObj = PointsInCar.FindClosest(hand.transform.position);
            nearestObj.tag = "nearestpoint";
            _first_3.tag = "nearestpoint";
            _pts = GameObject.FindGameObjectWithTag("nearestpoint");
            _renderers = _pts.GetComponents<Renderer>();
            isnearestfound = true;
            
            //nearestObj = best2(nearestObj, second, Testraycast());
            
            List<SpawnedPoint> pts2 = new List<SpawnedPoint>();
            pts2.Add(nearestObj);
            pts2.Add(_first_3);
  
            nearestObj = Use_Angles(pts2);
            
            Debug.DrawLine(hand.transform.position, nearestObj.transform.position, Color.red);
            Idtoros = nearestObj.Id;
            //nearestObj = best(nearestObj, second, Testraycast());
            write_result2(_handtime, nearestObj.Id,3);
            Nearobpos = nearestObj.transform.localPosition;
            Colorpose = nearestObj.transform.position;
            Nearobrot = nearestObj.transform.localRotation;
            if (_first_3 != nearestObj)
                _first_3 = nearestObj;
            //  Debug.Log("Nearest is at " + nearestObj.transform.position);
        }
    }
    
    
    
    private void WithHead_threshold4()
    {
        //var first = _first_4;
        var point = _first;
        foreach (var hand in Hands)
        {
            /*
             * select obj by hand
             * Check for visibility
             * return best based on visibility
             * check for threshold
             */
            SpawnedPoint nearestObj = PointsInCar.FindClosest(hand.transform.position);
            nearestObj.tag = "nearestpoint";
            _first_4.tag = "nearestpoint";
            _pts = GameObject.FindGameObjectWithTag("nearestpoint");
            _renderers = _pts.GetComponents<Renderer>();
            List<SpawnedPoint> pts2 = new List<SpawnedPoint>();
            
            //if(IsVisible(nearestObj.GetComponent<Renderer>()))
                pts2.Add(nearestObj);
            //if(IsVisible(_first_4.GetComponent<Renderer>()))
                pts2.Add(_first_4);
            point = Use_Angles(pts2);
            
            //var   point = best(nearestObj, First, Testraycast()); //used first and nearest obj because could not get the second from tree, 
            //nearestObj = best2(nearestObj, First, Testraycast());
            if ((Vector3.Distance(point.transform.position, hand.transform.position) < _threshold)&& IsVisible(point.GetComponent<Renderer>()))
            {
                //smallestf = dist;
                nearestObj = point;
            }
            else
            {
                nearestObj = _first_4;
            }
            Debug.DrawLine(hand.transform.position, nearestObj.transform.position, Color.red);
            Idtoros = nearestObj.Id;
            write_result2(_handtime, nearestObj.Id,4);
            
            //nearobpostion = nearestObj.transform.localPosition;
            Nearobpos = nearestObj.transform.localPosition;
            Colorpose = nearestObj.transform.position;
            Nearobrot = nearestObj.transform.localRotation;
            
            if (_first_4 != nearestObj)
                _first_4 = nearestObj;
        }
    }
    
    private void WithHead_threshold42()
    {
        //var point = _first_43;
        foreach (var hand in Hands)
        {
            /*
             * select object by visibility
             * select nearest by hand
             * check for threshold
             */
            //SpawnedPoint nearestObj = PointsInCar.FindClosest(hand.transform.position);
            //nearestObj.tag = "nearestpoint";
            //first.tag = "nearestpoint";
            //_pts = GameObject.FindGameObjectWithTag("nearestpoint");
            //_renderers = _pts.GetComponents<Renderer>();
            
            List<SpawnedPoint> pts2 = new List<SpawnedPoint>();
            pts2.Add(Safepoints[0]); 
            foreach (var pt in PointsInCar)
            {
                if (IsVisible(pt.GetComponent<Renderer>()))
                {
                    pts2.Add(pt);
                    Debug.Log("found point visible");
                } //  Debug.Log("found point");
            }
            
            var point = Use_Angles(pts2);

            if (!((Vector3.Distance(point.transform.position, hand.transform.position) < _threshold)))
            {
                point = _first_43;
            }
            
            Debug.DrawLine(hand.transform.position, point.transform.position, Color.red);
            Idtoros = point.Id;
            write_result2(_handtime, point.Id,42);
            
            //nearobpostion = nearestObj.transform.localPosition;
            Nearobpos = point.transform.localPosition;
            Colorpose = point.transform.position;
            Nearobrot = point.transform.localRotation;
            _first_43 = point;
        }
    }

    private void WithHead_Homepose5()
    {
        /*
         * select object by hand
         * 
         * This function selects the safe point based on association with a selected point,
         * does not use head gaze.
         */
       // var first = _first_52;
        foreach (var hand in Hands)
        {
            SpawnedPoint nearestObj = PointsInCar.FindClosest(hand.transform.position);
            nearestObj.tag = "nearestpoint";
            _first_52.tag = "nearestpoint";
            _pts = GameObject.FindGameObjectWithTag("nearestpoint");
            _renderers = _pts.GetComponents<Renderer>();
            //nearestObj 
            List<SpawnedPoint> pts2 = new List<SpawnedPoint>();
            pts2.Add(nearestObj);
            pts2.Add(_first_52);
            var point = Use_Angles(pts2);
            if ((Vector3.Distance(point.transform.position, hand.transform.position) < _threshold)&& IsVisible(point.GetComponent<Renderer>()) )
            {
                //smallestf = dist;
                nearestObj = point;
                //_trackhome = false;
            }
            else
            {
                //_trackhome = true;
                nearestObj = Associate_Safepointtopoint(point);
            }
            Debug.DrawLine(hand.transform.position, nearestObj.transform.position, Color.red);
            write_result2(_handtime, nearestObj.Id,5);
            Idtoros = nearestObj.Id;
            //nearobpostion = nearestObj.transform.localPosition;
            Nearobpos = nearestObj.transform.localPosition;
            Colorpose = nearestObj.transform.position;
            Nearobrot = nearestObj.transform.localRotation;
            if (_first_52 != nearestObj)
                _first_52 = nearestObj;
        }
    }


    private void WithHead_Homepose52()
    {
        /*not the best
         * This function selects desired points based on head gaze first,
         * then hand later based on a threshold.
         * Then associates home pose to point.
         * does not use kd tree
         * does not use head gaze to select safe point
         */
        //var first = _first_5;
        foreach (var hand in Hands)
        {
            //SpawnedPoint nearestObj = PointsInCar.FindClosest(hand.transform.position);
            var point = _first_5;
            List<SpawnedPoint> pts2 = new List<SpawnedPoint>();
            pts2.Add(PointsInCar[0]);
            foreach (var pt in PointsInCar)
            {
                if (IsVisible(pt.GetComponent<Renderer>()))
                {
                    pts2.Add(pt);
                }
            }

            //List<SpawnedPoint> pts = new List<SpawnedPoint>(PointsInCar);
            point = Use_Angles(pts2);
            if (!(Vector3.Distance(point.transform.position, hand.transform.position) < _threshold))
            {
                point = Associate_Safepointtopoint(point);
            }
            Debug.DrawLine(hand.transform.position, point.transform.position, Color.red);
            write_result2(_handtime, point.Id,52);
            Idtoros = point.Id;
            //write_result(_handtime, nearestObj.transform.position);
            //nearobpostion = nearestObj.transform.localPosition;
            Nearobpos = point.transform.localPosition;
            Colorpose = point.transform.position;
            Nearobrot = point.transform.localRotation;
            
            if (_first_5 != point)
                _first_5 = point;
        }
    }
    
    
    private void WithHead_Handthreshold_Homepose62()
    {
        /*
         * This function selects candidate points in head view first,
         * then distance from hand.
         * If point-hand dist > threshold,
         * Then selects safe point based on association with a given point
         * and not the head gaze.
         * 
         */
        //var first = _first;
        foreach (var hand in Hands)
        {
            List <SpawnedPoint> inview = new List<SpawnedPoint>();
            inview.Add(Safepoints[0]);
            foreach (var pt in PointsInCar)
            {
                if (IsVisible(pt.GetComponent<Renderer>()))
                {
                    inview.Add(pt);
                }
            }
            //var point = 
            SpawnedPoint nearestObj = Use_Angles(inview,_lambda);//select best point in view
            //nearestObj.tag = "nearestpoint";
            // first.tag = "nearestpoint";
            //_pts = GameObject.FindGameObjectWithTag("nearestpoint");
            //_renderers = _pts.GetComponents<Renderer>();
            //nearestObj 
            if (!(Vector3.Distance(nearestObj.transform.position, hand.transform.position) < _threshold))
            {
                //_trackhome = true;
                nearestObj = Associate_Safepointtopoint(nearestObj);
            }

            var position = nearestObj.transform.localPosition;
            Debug.DrawLine(hand.transform.position, position, Color.red);
            write_result2(_handtime, nearestObj.Id,62);
            Idtoros = nearestObj.Id;
            Nearobpos = position;
            Colorpose = nearestObj.transform.position;
            Nearobrot = nearestObj.transform.localRotation;
            
           // if (first != nearestObj)
            //    first = nearestObj;
        }
    }



    private void WithHead_Handthreshold_Homepose6()
    {
        var first = _first_6;
        foreach (var hand in Hands)
        {
            /*
             * Fisrt selects by hand nearest obj,
             * then by head visible
             * Move to safe point old.
             */
            //Debug.Log("Hand pos:"+hand.transform.position);
            SpawnedPoint nearestObj = PointsInCar.FindClosest(hand.transform.position); 
            nearestObj.tag = "nearestpoint";
            first.tag = "nearestpoint";
            _pts = GameObject.FindGameObjectWithTag("nearestpoint");
            _renderers = _pts.GetComponents<Renderer>();
            //nearestObj 
            
            List<SpawnedPoint> pts2 = new List<SpawnedPoint>();
            pts2.Add(PointsInCar[0]);
            if(IsVisible(nearestObj.GetComponent<Renderer>()))
                pts2.Add(nearestObj);
            if(IsVisible(_second.GetComponent<Renderer>()))
                pts2.Add(_second);
            var point = Use_Angles(pts2,_lambda);
            
            // var   point = best(nearestObj, First, Testraycast()); //used first and nearest obj because could not get the second from tree, 
            //nearestObj = best2(nearestObj, First, Testraycast());
            
            if (Vector3.Distance(point.transform.position, hand.transform.position) < _threshold  )
            {
                //smallestf = dist;
                nearestObj = point;
                //_trackhome = false;
            }
            else
            {
              //_trackhome = true;
              nearestObj = MovetoSafePoint();
            }

            var position = nearestObj.transform.position;
            Debug.DrawLine(hand.transform.position, position, Color.red);
            //write_result2(_handtime, position);
            write_result2(_handtime, nearestObj.Id,6);
            Idtoros = nearestObj.Id;
            
            //Debug.Log("Found point at :"+ position.ToString("F5")+"Time"+_handtime);
            //nearobpostion = nearestObj.transform.localPosition;
             //Nearobpos = nearestObj.transform.localPosition;
             
             Nearobpos = nearestObj.transform.localPosition;
             Colorpose = nearestObj.transform.position;
             Nearobrot = nearestObj.transform.localRotation;
             if (_first_6 != nearestObj)
                 _first_6 = nearestObj;
        }
    }
    
    
    
    
    
    private void SafepointsAlgo (Vector3 raycasthit)
    {
        //<summary>
        //This method determines a safe point by head rotation by selecting between the best in two frames
        //</summary>
        //input:
        //      list of safe points , head ray cast
        //output:
        //      Best point
        {
            var nearestDist = float.MaxValue;
            //GameObject nearestObj = null;

            foreach (var safepoint in Safepoints)
            {
                var d = _distance(safepoint.transform.position, raycasthit);
                if ( (d < nearestDist ) && IsVisible(safepoint.GetComponent<Renderer>()))
                {
                    nearestDist = d;
                    _nearestObj = safepoint;
                }
            }
            Debug.DrawLine(raycasthit, _nearestObj.transform.position, Color.red);
            //write_result(_handtime, _nearestObj.transform.position);
            //Debug.Log("Nearest distance:" + nearestDist);
        }
    }
    
     private void TestHandvel_direct()
    {
        var newPos = Hands[0].transform.position; // each frame track the new position
        var objVelocity = (newPos - _prevPos); // Time.fixedDeltaTime;  // velocity = dist/time
        Vector3 currentDirection = (newPos - _prevPos).normalized;
        _prevPos = newPos; // update position for next frame calculation
        var rayOrigin = newPos;
        var raydirection = objVelocity;
        var rayLength = 5.0f;
        RaycastHit hitcollider = new RaycastHit();

        if (Physics.Raycast(newPos, raydirection, out hitcollider, 20f))
        {
            //nearestObj = Distfromray(hitcollider, NewPos, ObjVelocity);
            _nearestObj = Directray(hitcollider, newPos, objVelocity);
            Debug.DrawLine(newPos,_nearestObj.transform.position,Color.green);
            
            //using (StreamWriter sw = File.AppendText(_algoresults))
            //{
                //sw.WriteLine(_handtime + "," + nearestObj.transform.position.ToString("F4"));
            //}
        }
        //Debug.DrawLine(NewPos,nearestObj.transform.position,Color.green);
        Nearobpos = _nearestObj.transform.localPosition;
        //nearobrot = nearestObj.transform.rotation;
       // Colorpose = nearestObj.transform.localPosition;
        //return nearestObj;
    }
    
    
    void TestHandvel_distfromray()
    {
        var newPos = Hands[0].transform.position; // each frame track the new position
        var objVelocity = (newPos - _prevPos); // Time.fixedDeltaTime;  // velocity = dist/time
        Vector3 currentDirection = (newPos - _prevPos).normalized;
        _prevPos = newPos; // update position for next frame calculation
        var rayOrigin = newPos;
        var raydirection = objVelocity;
        var rayLength = 5.0f;
        RaycastHit hitcollider = new RaycastHit();

        if (Physics.Raycast(newPos, raydirection, out hitcollider, 20f))
        {
            _nearestObj = Distfromray(hitcollider, newPos, objVelocity);
        }
        //write_result(_handtime, _nearestObj.transform.position);
        Debug.DrawLine(newPos,_nearestObj.transform.position,Color.green);
        Nearobpos = _nearestObj.transform.localPosition;
        //nearobrot = nearestObj.transform.rotation;
        // Colorpose = nearestObj.transform.localPosition;
        //return nearestObj;
    }
    
    
    private void TestHandvel_distfromray2(Transform hand)
    {
        
        var newPos = Hands[0].transform.position; // each frame track the new position
        var newvel = (newPos - _prevPos); // Time.fixedDeltaTime;  // velocity = dist/time
        var objVelocity = ((oldvel + newvel) / 2);
        Vector3 currentDirection = (newPos - _prevPos).normalized;
        _prevPos = newPos; // update position for next frame calculation
        var rayOrigin = newPos;
        var raydirection = objVelocity.normalized;
        var rayLength = 5.0f;
        RaycastHit hitcollider = new RaycastHit();
        oldvel = newvel;
        List<SpawnedPoint> pts2 = new List<SpawnedPoint>();
        foreach (var pt in PointsInCar)
        {
            pts2.Add(pt);
            
        }
        _nearestObj = Use_AnglesHandvel(pts2,newPos, raydirection);
        Nearobpos = _nearestObj.transform.localPosition;
        Nearobrot = _nearestObj.transform.rotation;
 
        write_result2(_handtime, _nearestObj.Id,7);
        Debug.DrawLine(newPos,_nearestObj.transform.position,Color.red);
    }
    
    
    private void TestHandvel_distfromray3(Transform hand)
    {
        var first = _first;
        //This function introduces a thresh hold on velocity magnitude
        var best = _nearestObj;
        //var newPos = Hands[0].transform.position; // each frame track the new position
        var newPos = hand.transform.position;
        var newvel = (newPos - _prevPos); // Time.fixedDeltaTime;  // velocity = dist/time
        var objVelocity = ((oldvel + newvel) / 2);
        Vector3 currentDirection = (newPos - _prevPos).normalized;
        _prevPos = newPos; // update position for next frame calculation
        var raydirection = objVelocity.normalized;
        var rayLength = 5.0f;
        oldvel = newvel;
        List<SpawnedPoint> pts2 = new List<SpawnedPoint>();
        foreach (var pt in PointsInCar)
        {
            pts2.Add(pt);
        }

        if (objVelocity.magnitude > 0.4f)
        {
            best = Use_AnglesHandvel(pts2,newPos, raydirection,_lambda);

        }
        else
        {
            best = first;
        }

        _nearestObj = best;
        Nearobpos = _nearestObj.transform.localPosition;
        Nearobrot = _nearestObj.transform.rotation;
        write_result2(_handtime, _nearestObj.Id,8);
        
        Debug.DrawLine(newPos,_nearestObj.transform.position,Color.red);
        if (first != _nearestObj)
            first = _nearestObj;
    }

    private SpawnedPoint Use_AnglesHandvel(List<SpawnedPoint> pt, Vector3 newpos, Vector3 raydirection, float lambda=Mathf.Infinity)
    {
                /*This function returns closest object based on the angle from camera
         * Input : point 1, point2, head ray
         * Output: point star
         *steps.
         * Draw ray from cam.
         * calclualte distance l1 from point from cam,
         * calcluate distance l2 from ray
         * 
         */
        
        var position = Hands[0].transform.position;
        SpawnedPoint nearest = pt[0];
        var minAng = 0.0f;
        var maxL = Mathf.Infinity;
        for (int i = 0; i < pt.Count; i++)
        {
            var tt= ClosestPointOnLineSegment(
                position, raydirection * 10f, pt[i].transform.position);
            Debug.DrawLine(position,raydirection * 10, Color.green);
            var l1 = Vector3.Distance(position, tt);
            var l2 = Vector3.Distance(pt[i].transform.position, tt);
            var ang = l2 / l1;

            if ((ang > minAng)&& ang < _lambda &&  IsVisible(pt[i].GetComponent<Renderer>()))
            {
                minAng = ang;
                nearest = pt[i];
            }
        }
        Debug.DrawLine(position,nearest.transform.position, Color.red);
        return nearest;
    }


    public Vector3 Getclosestobjectposition()
    {
        return Nearobpos;
    }

    public Quaternion Getclosestobjectrotation()
    {
        return Nearobrot;
    }
    
    //public Vector3 Getclosestobjectlocalposition()
    //{
    //    return _nearoblocalpose;
    //}
    //get the positio of the game object stored in a varaibale
    public void Setposition(Vector3 pos)
    {
        Nearobpos = pos;
    }
    
    public Vector3 Testraycast()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(
                cam.transform.position,
                cam.transform.forward,
                out hitInfo,
                20.0f,
                Physics.DefaultRaycastLayers))
        {
            Debug.DrawRay(cam.transform.position, cam.transform.TransformDirection(Vector3.forward) * hitInfo.distance, Color.green);
            return hitInfo.point;
        }
        else
        {
            Debug.DrawRay(cam.transform.position, cam.transform.TransformDirection(Vector3.forward) * 1000, Color.red);
            return hitInfo.point;
            // Debug.Log("Did not Hit");
        }

    }

    public SpawnedPoint Best(SpawnedPoint first, SpawnedPoint second, Vector3 raycasthit)
    {
        var best = first;
        float d1 = _distance(first.transform.position, raycasthit);
        float d2 = _distance(second.transform.position, raycasthit);
        //Add condition if in view

        if ((d1 <= d2) && IsVisible(first.GetComponent<Renderer>()))
            best = first;
        else if ((d2 <= d1) && IsVisible(second.GetComponent<Renderer>()))
            best = second;
        else
            best = first;
        return best;
    }
    
    
    
    public SpawnedPoint Best2(SpawnedPoint first, SpawnedPoint second, Vector3 raycasthit)
    {
        SpawnedPoint best = first;
        float d1 = _distance(first.transform.position, raycasthit);
        float d2 = _distance(second.transform.position, raycasthit);
        if (((d1 <= d2) && (d1 <= _tol)) && IsVisible(first.GetComponent<Renderer>()))
            best=first;
        else if(((d2 <= d1) && (d2 <= _tol)) && IsVisible(second.GetComponent<Renderer>()))
            best= second;
        return best;
    }
    

    protected float _distance(Vector3 a, Vector3 b)
    {
        //return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) + (a.z - b.z) * (a.z - b.z);
        return Vector3.Distance(a, b);
    }

    //void OutputVisibleRenderers(Renderer[] renderers)
    private bool IsVisible(Renderer renderer)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);

        if (GeometryUtility.TestPlanesAABB(planes, renderer.bounds))
            return true;
        else
            return false;
    }

    //section Hand Vel
   

    private SpawnedPoint Directray(RaycastHit hitcollider, Vector3 newPos, Vector3 objVelocity)
    {
        var b = hitcollider.collider.GetComponent<SpawnedPoint>();
        if (b != null)
        {
            print("Ray hit tennisball!");
            //Debug.Log("Ray hit Tennisball");
            _nearestObj = b;
        } // Debug.DrawRay(NewPos, ObjVelocity * 1000f, Color.red, 0.5f, false);
        return _nearestObj;
    }
    
    private SpawnedPoint Distfromray(RaycastHit hitcollider, Vector3 newPos, Vector3 objVelocity)
    {
        //find nearest point from ray hit point
        var smallestf = float.MaxValue;
        foreach (var ball in PointsInCar )
        {
            float d = _distance(ball.transform.position, hitcollider.point);
            if (d<=smallestf)
            {
                smallestf = d;
                _nearestObj = ball;
            }
        }
        Debug.DrawRay(newPos, objVelocity * 1000f, Color.red, 0.1f, false);
        return _nearestObj;
    }
    
    private void NaiveNn()
    {
        var smallestf = float.MaxValue;
        foreach (var hand in Hands)
        {
            foreach (var point in PointsInCar2)
            {
                var dist=Vector3.Distance(hand.transform.position, point.transform.position);
                if ((dist < smallestf))
                {
                    if (dist < _tol)
                    {
                        smallestf = dist;
                        _nearestObj = point;
                        //_trackhome = false;
                    }
                    else
                    {
                        //_trackhome = true;
                        _nearestObj = MovetoSafePoint();
                    }
                }
            }
            Nearobpos = _nearestObj.transform.localPosition;
            Nearobrot = _nearestObj.transform.rotation;
            Colorpose = _nearestObj.transform.position;
            Debug.DrawLine(hand.transform.position, _nearestObj.transform.position, Color.red);
        }
    }
    
    
    Vector3 ClosestPointOnLineSegment(Vector3 segmentStart, Vector3 segmentEnd, Vector3 point) {
        // Shift the problem to the origin to simplify the math.    
        var wander = point - segmentStart;
        var span = segmentEnd - segmentStart;

        // Compute how far along the line is the closest approach to our point.
        float t = Vector3.Dot(wander, span) / span.sqrMagnitude;

        // Restrict this point to within the line segment from start to end.
        t = Mathf.Clamp01(t);

        // Return this point.
        return segmentStart + t * span;
    }
    
    SpawnedPoint MovetoSafePoint()
    {
        //find closest point to ray from head cast,
        //input: raycast, safe points
        //output: safe point
        //SpawnedPoint best = Safepoints[0];
        SpawnedPoint best = _nearestObj;
        float closestSquaredRange = Mathf.Infinity;
        for (int i = 0; i < Safepoints.Count; i++)
        {
            var closestPoint = ClosestPointOnLineSegment(
                cam.transform.position,
                cam.transform.TransformDirection(Vector3.forward) * 10f,
                Safepoints[i].transform.position
            );
            
            var squaredRange = (Safepoints[i].transform.position - closestPoint).sqrMagnitude;
            if(squaredRange < closestSquaredRange) {
                closestSquaredRange = squaredRange;
                //best.transform.position=Safepoints[i].transform.position;
                //best.transform.rotation=Safepoints[i].transform.rotation;
                best = Safepoints[i];
                //Nearobpos = nearestObj.transform.localPosition;
            }
        } 
        // Debug.DrawLine(cam.transform.position,Nearobpos,Color.yellow);
       return best;
    }

    private SpawnedPoint Use_Angles(List<SpawnedPoint> pt,float lambda= Mathf.Infinity )
    {
        /*This function returns closest object based on the angle from camera
         * Input : point list, head ray
         * Output: point star
         *steps.
         * Draw ray from cam.
         * calclualte distance l1 from point from cam,
         * calcluate distance l2 from ray
         * 
         */
        var position = cam.transform.position;
        SpawnedPoint nearest = pt[0];
        //var minAng = 0.0f;
        var maxAng = Mathf.Infinity;
        for (int i = 0; i < pt.Count; i++)
        {
            var tt= ClosestPointOnLineSegment(
                position, cam.transform.forward * 10f, pt[i].transform.position);
            Debug.DrawLine(position,cam.transform.forward * 10, Color.green);
            var l1 = Vector3.Distance(position, tt);
            var l2 = Vector3.Distance(pt[i].transform.position, tt);
            var ang = l2 / l1;

            if ((ang < maxAng) && (ang < _lambda))
            {
                maxAng = ang;
                nearest = pt[i];
            }
        }
        //Debug.DrawLine(position,nearest.transform.position, Color.red);
        return nearest;
    }
    
    
    private SpawnedPoint Use_Angles2(List<SpawnedPoint> pt, float lambda= Mathf.Infinity )
    {
        /*This function returns closest object based on the angle from camera based on eye gaze tracking
         * Input : point list, head ray
         * Output: point star
         *steps.
         * Draw ray from cam.
         * calclualte distance l1 from point from cam,
         * calcluate distance l2 from ray
         * 
         */
        var position = cam.transform.position;
        SpawnedPoint nearest = pt[0];
        //var minAng = 0.0f;
        var maxAng = Mathf.Infinity;
        for (int i = 0; i < pt.Count; i++)
        {
            var tt= ClosestPointOnLineSegment(
                position, position+_gazevector * 10f, pt[i].transform.position);
            //Debug.DrawLine(position,position+_gazevector * 10, Color.green);
            var l1 = Vector3.Distance(position, tt);
            var l2 = Vector3.Distance(pt[i].transform.position, tt);
            //var tanang = l2 / l1;
            var ang = (float)(Math.Atan2(l2, l1)*180/3.14162);
           
            if ((ang < maxAng) && (ang < lambda))
            {
                maxAng = ang;
                nearest = pt[i];
                
            }
        }
        Debug.DrawLine(position,nearest.transform.position, Color.red);
        //Debug.DrawRay(position,nearest.transform.position, Color.red, duration:6);
        //Debug.Log("eyegzae next  "+ nearest.Id +"lambda"+lambda + "maxangl"+maxAng);
        return nearest;
    }

    
    private SpawnedPoint Associate_Safepointtopoint(SpawnedPoint pt)
    {
        /* Associates visible home pose to point, does not take head gaze into account.
         * input point 1, list of home poses/Safe points
         * output safe point 
         * procedure:
         * find closest home pose from point.
         */
   
        SpawnedPoint sp = _nearestObj;
        var Maxdist = Mathf.Infinity;
        foreach (var s in Safepoints)
        {
            var dist = Vector3.Distance(s.transform.position, pt.transform.position);
            if (dist < Maxdist)
            {
                Maxdist = dist;
                sp = s;
            }
        }
        return sp;
    }

    //TODO
    private void Use_Headgaze_toselectsafepoint()
    {
        /* The function selects a safe point based on head gaze and points
         * In put: list of safe points, desired point, previous point 
         * Output: Safe point
         * Procedure:
         * check 2 safe points close to the interest point.
         * check direction based on start and destination.
         * Unknown: criteria for selecting the safe point.???
         **/
    }

    //TODO
    private void Introduce_time_delay(float t)
    {
        /*This function introduces a delay in the selection of a desired point.
         * Input: List of points, Hand, delta_t
         * Output : best point
         * procedure.
         * detect point
         * start timer
         * while point is not changed
         * if timer value > certain number of frames,
         * detected point is valid
         * increment after every frame.
         * end while
         *
         *
         * when  a point is changed,
         * start timer, if time is greater = delta_t
         * send data of the point.
         */
        SpawnedPoint nearestObj = PointsInCar.FindClosest(Hands[0].transform.position);
    }
}
