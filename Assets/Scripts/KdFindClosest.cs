using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class KdFindClosest : MonoBehaviour
{
    public Button InitialiseButton;
    //public Transform car; 
    public GameObject WhitePrefab;
    public GameObject BlackPrefab;

    public Transform CalTracker;
    public int CountWhite;
    public int CountBlack;
    public GameObject[] points;
    //private GameObject _closestobjectpose;
    private GameObject _ClosestObject;
    private bool _isnearestfound = false;

    private Vector3 nearobpostion;
    private Quaternion nearobrot;

    Camera cam;
    Collider objCollider;
    Plane[] planes;
    Renderer[] renderers;


    protected KdTree<SpawnedPoint> PointsInCar = new KdTree<SpawnedPoint>();
    protected KdTree<SpawnedPoint> Hands = new KdTree<SpawnedPoint>();

    SpawnedPoint second;
    
    // protected List<RandomMove> PointsInCar = new List<RandomMove>();

    // Spawn out balls at start of the game
    void Start()
    {
        //InitialiseButton.onClick.AddListener(delegate {init();});
        init();
    }

    public void init() {
        cam = Camera.main;
        planes = GeometryUtility.CalculateFrustumPlanes(cam);
        objCollider = GetComponent<Collider>();
        second = PointsInCar[0];
        second.tag = "nearestpoint";

        for (int i = 0; i < points.Length; i++)
        //foreach (var point in points)
        {
            //Quaternion spawnRotation = Quaternion.identity;
            points[i].transform.parent = CalTracker.transform; //only for visuals but no effect
            //Debug.Log("After tracker parent: Point :" + i + "Posiotn:" + points[i].transform.position.ToString("F3"));
            GameObject point = (Instantiate(BlackPrefab, points[i].transform.position, points[i].transform.rotation));
            //Debug.Log("Spawn at Point" + i + "Posiotn:" + point.transform.position.ToString("F3"));
            point.transform.parent = CalTracker.transform;

            PointsInCar.Add((point).GetComponent<SpawnedPoint>());
            // StartCoroutine(SpawnRoutine());
        }

        for (int i = 0; i < CountWhite; i++)
        {
            Hands.Add(Instantiate(WhitePrefab).GetComponent<SpawnedPoint>());
        }

        GameObject walls = GameObject.FindGameObjectWithTag("nearestpoints");
        //renderers = walls.GetComponentsInChildren<Renderer>();
        renderers = walls.GetComponents<Renderer>();



    }

    // Update is called once per frame

    void Update()
    { 
        PointsInCar.UpdatePositions();
        foreach (var whiteball in Hands)
        {
            SpawnedPoint nearestObj = PointsInCar.FindClosest(whiteball.transform.position);
            nearestObj.tag = "nearestpoint";         
            /**
            var nearobjs2 = new List<SpawnedPoint>();
            var nearobjs = PointsInCar.FindClose(whiteball.transform.position);
            //var knearestobjs = PointsInCar.FindKClosest(whiteball.transform.position, 3);
            foreach (var xr in nearobjs)
            {
                Debug.Log("nearobjs " + xr.GetComponent<SpawnedPoint>().getId().ToString());
            }
            **/
            _isnearestfound = true;

            Debug.DrawLine(whiteball.transform.position, nearestObj.transform.position, Color.red);
           // _ClosestObject.transform.position = nearestObj.transform.localPosition;
           // nearobpostion = nearestObj.transform.localPosition;
           // nearobrot = nearestObj.transform.localRotation;

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
            nearobrot = nearestObj.transform.localRotation;

            if (second != nearestObj)
                second = nearestObj;   
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
            // Debug.Log("Did Hit " + hitInfo.point);
            // If the Raycast has succeeded and hit a hologram
            // hitInfo's point represents the position being gazed at
            // hitInfo's collider GameObject represents the hologram being gazed at
            return hitInfo.point;
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            return hitInfo.point;
            //  Debug.Log("Did not Hit");
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



