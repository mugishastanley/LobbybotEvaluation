using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class KdFindClosest : MonoBehaviour
{
    public Button InitialiseButton;
    public GameObject WhitePrefab;
    public GameObject BlackPrefab;

    public Transform CalTracker;

    public int CountWhite;
    public int CountBlack;

    public GameObject[] points;
    private GameObject _ClosestObject;
    private bool _isnearestfound = false;

    private Vector3 nearobpostion;
    private Vector3 nearoblocalpose;
    private Quaternion nearobrot;

    Camera cam;
    Collider objCollider;
    Plane[] planes;
    Renderer[] renderers;
    GameObject pts;

    Matrix4x4 nearestobjmat;


    protected KdTree<SpawnedPoint> PointsInCar = new KdTree<SpawnedPoint>();
    protected KdTree<SpawnedPoint> Hands = new KdTree<SpawnedPoint>();

    SpawnedPoint second;
    
   

    // Spawn out Capsules at start of the game
    void Start()
    {
        //InitialiseButton.onClick.AddListener(delegate {init();});
        init();
    }

    public Matrix4x4 matnearestob
    {
        get { return nearestobjmat; }
        set { nearestobjmat = value; }
    }

    public void init() {
        cam = Camera.main;
          
        for (int i = 0; i < points.Length; i++)
        //foreach (var point in points)
        {
            //Quaternion spawnRotation = Quaternion.identity;
            Debug.Log("points " + i + " rotation before caltracker transform" + points[i].transform.rotation.eulerAngles.ToString("F3"));
            Debug.Log("points " + i + " local rotation before caltracker transform" + points[i].transform.localRotation.eulerAngles.ToString("F3"));
            Debug.Log("points " + i + " position before caltracker transform" + points[i].transform.position.ToString("F3"));
            Debug.Log("points " + i + " localposition before caltracker transform" + points[i].transform.localPosition.ToString("F3"));

            points[i].transform.parent = null;
            points[i].transform.parent = CalTracker.transform;
            //Debug.Log("After tracker parent: Point :" + i + "Posiotn:" + points[i].transform.position.ToString("F3"));

            //Debug.Log("points " + i + " rotation after caltracker transform" + points[i].transform.rotation.eulerAngles.ToString("F3"));
            //Debug.Log("points " + i + " local rotation after caltracker transform" + points[i].transform.localRotation.eulerAngles.ToString("F3"));
            //Debug.Log("points " + i + " position after caltracker transform" + points[i].transform.position.ToString("F3"));
            //Debug.Log("points " + i + " localposition after caltracker transform" + points[i].transform.localPosition.ToString("F3"));

            GameObject point = (Instantiate(BlackPrefab, points[i].transform.position, points[i].transform.rotation));

           
            //Debug.Log("point " + i + " rotation before intiate" + point.transform.rotation.eulerAngles.ToString("F3"));
            //Debug.Log("point " + i + " local rotation before intiate" + point.transform.localRotation.eulerAngles.ToString("F3"));
            //Debug.Log("point " + i + " position  before intiate" + point.transform.position.ToString("F3"));
            //Debug.Log("point " + i + " localposition  before intiate" + point.transform.localPosition.ToString("F3"));


            point.transform.parent = CalTracker.transform;

            //Debug.Log("point " + i + " rotation after intiate" + point.transform.rotation.eulerAngles.ToString("F3"));
            //Debug.Log("point " + i + " local rotation after intiate" + point.transform.localRotation.eulerAngles.ToString("F3"));
            //Debug.Log("point " + i + " position  after intiate" + point.transform.position.ToString("F3"));
            //Debug.Log("point " + i + " localposition  after intiate" + point.transform.localPosition.ToString("F3"));
            PointsInCar.Add((point).GetComponent<SpawnedPoint>());

            //Debug.Log("Matrix transform point:"+ i + " " +T22T1(CalTracker, point.transform));
            //Debug.Log("Matrix transform TRS point:" + i + " " + T22T12(point.transform));


            //points[i].transform.parent = CalTracker.transform;
            //StartCoroutine(SpawnRoutine());
        }

        for (int i = 0; i < CountWhite; i++)
        {
            Hands.Add(Instantiate(WhitePrefab).GetComponent<SpawnedPoint>());
        }
        second = PointsInCar[0];

    }


    public Matrix4x4 T22T1(Transform T1, Transform T2) // T2 wrt T1
    {
        return T1.worldToLocalMatrix * T2.localToWorldMatrix;
    }

    public Matrix4x4 T22T12(Transform T1) // T2 wrt parent
    {
        return Matrix4x4.TRS(T1.localPosition, T1.localRotation, T1.transform.localScale);
    }


    // Update is called once per frame

    void Update()
    { 
        PointsInCar.UpdatePositions();
        //withHead();
        withoutHead();

    }


    public void withoutHead() {
        foreach (var whiteball in Hands)
        {
            SpawnedPoint nearestObj = PointsInCar.FindClosest(whiteball.transform.position);
            nearestObj.tag = "nearestpoint";
            second.tag = "nearestpoint";
            pts = GameObject.FindGameObjectWithTag("nearestpoint");
            renderers = pts.GetComponents<Renderer>();
            _isnearestfound = true;

            Debug.DrawLine(whiteball.transform.position, nearestObj.transform.position, Color.red);


            if (_isnearestfound)
            {
                var cubeRenderer = nearestObj.GetComponent<Renderer>();
                cubeRenderer.material.color = Color.red;
                //Call SetColor using the shader property name "_Color" and setting the color to red
                cubeRenderer.material.SetColor("_Color", Color.red);
                _isnearestfound = false;
            }
          //  nearestObj = best(nearestObj, second, Testraycast());
            nearobpostion = nearestObj.transform.localPosition;
            nearobrot = nearestObj.transform.localRotation;
            Debug.Log("Nearest is at " + nearestObj.transform.position);
        }


    }


    public void withHead() {
        foreach (var whiteball in Hands)
        {
            SpawnedPoint nearestObj = PointsInCar.FindClosest(whiteball.transform.position);
            nearestObj.tag = "nearestpoint";
            second.tag = "nearestpoint";
            pts = GameObject.FindGameObjectWithTag("nearestpoint");
            renderers = pts.GetComponents<Renderer>();
            _isnearestfound = true;

            Debug.DrawLine(whiteball.transform.position, nearestObj.transform.position, Color.red);


            if (_isnearestfound)
            {
                var cubeRenderer = nearestObj.GetComponent<Renderer>();
                cubeRenderer.material.color = Color.red;
                //Call SetColor using the shader property name "_Color" and setting the color to red
                cubeRenderer.material.SetColor("_Color", Color.red);
                _isnearestfound = false;
            }
            nearestObj = best(nearestObj, second, Testraycast());
            nearobpostion = nearestObj.transform.localPosition;
            nearoblocalpose = nearestObj.transform.position;
            nearobrot = nearestObj.transform.localRotation;

            if (second != nearestObj)
                second = nearestObj;
          //  Debug.Log("Nearest is at " + nearestObj.transform.position);
        }


    }

    public Vector3 getclosestobjectposition()
    { 
         return nearobpostion;
    }

    public Quaternion getclosestobjectrotation()
    {
        return nearobrot;
    }
    public Vector3 getclosestobjectlocalposition()
    {
        return nearoblocalpose;
    }
    //get the positio of the game object stored in a varaibale
    public Vector3 getclosestobjectpose(){        
        return _ClosestObject.transform.localPosition;
    }
    public Quaternion getclosestobjectrot() { 
    return _ClosestObject.transform.rotation;
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
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hitInfo.distance, Color.black);
            return hitInfo.point;
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            return hitInfo.point;
            Debug.Log("Did not Hit");
        }

    }
    
    public SpawnedPoint best(SpawnedPoint first, SpawnedPoint second, Vector3 raycasthit){
        float d1 = _distance(first.transform.position, raycasthit) ;
        float d2 = _distance(second.transform.position, raycasthit);
        //Add condition if in view

        if ((d1 <= d2) && IsVisible(first.GetComponent<Renderer>()))
            return first;
        else if ((d2 <= d1) && IsVisible(second.GetComponent<Renderer>()))
            return second;
        else
            return first;
    }
    
    protected float _distance(Vector3 a, Vector3 b)
    {
            return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) + (a.z - b.z) * (a.z - b.z);
    }

    public void TestGaze()
    {
        if (GeometryUtility.TestPlanesAABB(planes, objCollider.bounds))
        {
           // Debug.Log(obj.name + " has been detected!");
        }
        else
        {
            Debug.Log("Nothing has been detected");
        }
    }

    //void OutputVisibleRenderers(Renderer[] renderers)
    private bool IsVisible(Renderer renderer)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        if (GeometryUtility.TestPlanesAABB(planes, renderer.bounds))
            return true;
        else
            return false;
    }

}



