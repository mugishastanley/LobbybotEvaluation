using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class KdFindClosest : MonoBehaviour
{
    public Button InitialiseButton;
    public GameObject WhitePrefab;
    public GameObject BlackPrefab;
    public GameObject Plane;
    public bool ProjectiononPlane;
    public Transform CalTracker;
    public Transform RobotUrdf;
    public float velinside = 0.25f;
    public float veloutside =0.6f;
    public int CountWhite;
    public int CountBlack;
    float tol = 0.3f;

    public GameObject[] points;
    private GameObject _ClosestObject;
    private bool _isnearestfound = false;


    private Vector3 nearobpostion;
    private Vector3 nearoblocalpose;
    private Quaternion nearobrot;
    private float d1, d2, d3, d4;
    private float time_throughplane, time_inside;

    Camera cam;
    Collider objCollider;
    Plane[] planes;
    Renderer[] renderers;
    GameObject pts;

    Matrix4x4 nearestobjmat;
    
    protected KdTree<SpawnedPoint> PointsInCar = new KdTree<SpawnedPoint>();
    protected KdTree<SpawnedPoint> Hands = new KdTree<SpawnedPoint>();

    SpawnedPoint First;
    SpawnedPoint second;

    public Vector3 Nearobpos { get; set; }
    public Vector3 Colorpose { get; set; }



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

    public void init()
    {
        PointsInCar.UpdatePositions();
        cam = Camera.main;
        for (int i = 0; i < points.Length; i++)
        //foreach (var point in points)
        {

            GameObject point = (Instantiate(BlackPrefab, points[i].transform.position, points[i].transform.rotation, CalTracker.transform));
            PointsInCar.Add((point).GetComponent<SpawnedPoint>());
        }

        for (int i = 0; i < CountWhite; i++)
        {
            Hands.Add(Instantiate(WhitePrefab).GetComponent<SpawnedPoint>());
        }
        First = PointsInCar[0];
        second = PointsInCar[0];
    }


    // Update is called once per frame

    void Update()
    {
        PointsInCar.UpdatePositions();
        withHead();
       // withHead2();
        //WithoutHeadPlane();
        //WithoutHeadOld();
        //AdaptiveselectionwithHead(Plane);

    }
    public void WithoutHeadPlane()
    {
        foreach (var whiteball in Hands)
        {

            SpawnedPoint nearestObj = PointsInCar.FindClosest(whiteball.transform.position);
            nearestObj.tag = "nearestpoint";
            First.tag = "nearestpoint";
            pts = GameObject.FindGameObjectWithTag("nearestpoint");
            renderers = pts.GetComponents<Renderer>();
            _isnearestfound = true;
            //Debug.Log("First1 is at " + First.transform.localPosition);
            Debug.DrawLine(whiteball.transform.position, nearestObj.transform.position, Color.red);
            //  nearestObj = best(nearestObj, First, Testraycast());
            Nearobpos = nearestObj.transform.localPosition;
            //nearobrot = nearestObj.transform.localRotation;
            nearobrot = nearestObj.transform.localRotation;
            Colorpose = nearestObj.transform.position;
            //Debug.Log("nearest is found is second is at " + nearobpostion);
            //last object becomes first

            if (First != nearestObj)
            {
                var p1 = First;
                //Debug.Log("p1 is at = " + p1.transform.localPosition);
                var p2 = nearestObj;
                //Debug.Log("p2 is at = " + p2.transform.localPosition);

                
                Vector3 p1prime = PlaneProjection(p1, Plane);
                Vector3 p2prime = PlaneProjection(p2, Plane);
                //var pstar = p2.transform.localPosition;

                d1 = _distance(p1prime, p1.transform.localPosition);
                Debug.Log("distance d1 = " + d1);
                d2 = _distance(p2.transform.localPosition, p1.transform.localPosition);
                Debug.Log("distance d2 = " + d2);
                d3 = _distance(p1prime, p2prime);
                Debug.Log("distance d3 = " + d3);
                d4 = _distance(p2prime, p2.transform.localPosition);
                Debug.Log("distance d4 = " + d4);
                time_throughplane = d1 / velinside + d3 / veloutside + d4 / velinside;
                time_inside = d2 / velinside;

                //if ((d1) >= d2)
                if (time_inside < time_throughplane)
                {
                    //pstar = p2.transform.localPosition;
                    //nearobpostion = pstar;
                    Nearobpos = p2.transform.localPosition; ;
                    //Debug.Log("route inside:" + pstar + "time inside " + time_inside + " < time outside " + time_throughplane);
                    Debug.Log("route inside:");
                }
                
                else
                {

                    
                    //Debug.Log("route outside:" + pstar + "time inside " + time_inside + " > time outside" + time_throughplane);
                    Debug.Log("route outside:");
                    //create a local stack
                    Stack<Vector3> ts = new Stack<Vector3>();
                    //fill the stck
                    ts.Push(p2.transform.localPosition);
                    ts.Push(p2prime);
                    ts.Push(p1prime);

                    
                        
                    
                    //ts.Push(p2.transform.localPosition);

                    //empty stack
                    //pstar=Adaptiveselection(p1) //recursive call;
                    
                    Debug.Log("Before ts size :" + ts.Count);
                    
                    while (ts.Count > 0)
                    {
                        Debug.Log("ts size :" + ts.Count);
                        Nearobpos = ts.Peek();
                        Nearobpos = ts.Pop();
                        Debug.Log("Stack contains projections:" + Nearobpos.ToString("F3"));
                      
                    }   
                    
                                        

                }
                
                /***

                else {
                    Nearobpos = p1prime;
                    Colorpose = p1prime;
                    Debug.Log("Taking plane proj Nearets pstar 1:" + Nearobpos.ToString("F3"));
                    //StartCoroutine(ProjectionCoroutine());
                    Nearobpos = p2prime;
                    Colorpose = p2prime;
                    //StartCoroutine(ProjectionCoroutine());
                    Debug.Log("Taking plane proj Nearets pstar 2:" + Nearobpos.ToString("F3"));
                    Nearobpos = p2.transform.localPosition;
                    Colorpose = p2.transform.position;
                    Debug.Log("Taking plane proj Nearets pstar 3:" + Nearobpos.ToString("F3"));
                    //StartCoroutine(ProjectionCoroutine());
                }

                ****/

                Debug.DrawLine(whiteball.transform.position, nearestObj.transform.position, Color.red);

                //Nearobpos = nearestObj.transform.localPosition;
                //Colorpose = nearestObj.transform.position;
                //nearobrot = nearestObj.transform.localRotation;


            }

            //Debug.Log("First changed is at " + First.transform.localPosition);
            First = nearestObj;

        }

    }

    


    public void withHead()
    {
        foreach (var hand in Hands)
        {
            SpawnedPoint nearestObj = PointsInCar.FindClosest(hand.transform.position);
            nearestObj.tag = "nearestpoint";
            First.tag = "nearestpoint";
            pts = GameObject.FindGameObjectWithTag("nearestpoint");
            renderers = pts.GetComponents<Renderer>();
            _isnearestfound = true;

            //Debug.DrawLine(whiteball.transform.position, nearestObj.transform.position, Color.red);
            nearestObj = best(nearestObj, First, Testraycast());
            nearestObj = best2(nearestObj, First, Testraycast());
            

            if (Vector3.Distance(nearestObj.transform.position, hand.transform.position) < 0.3f)
            {
                Debug.DrawLine(hand.transform.position, nearestObj.transform.position, Color.red);
                //nearobpostion = nearestObj.transform.localPosition;
                Nearobpos = nearestObj.transform.localPosition;
                Colorpose = nearestObj.transform.position;
                nearobrot = nearestObj.transform.localRotation;
            }
            
            if (First != nearestObj)
                First = nearestObj;
        }


    }


    public void withHead2()
    {
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
            nearestObj = best2(nearestObj, second, Testraycast());
            nearobpostion = nearestObj.transform.localPosition;
            nearoblocalpose = nearestObj.transform.position;
            nearobrot = nearestObj.transform.localRotation;

            if (second != nearestObj)
                second = nearestObj;
            //  Debug.Log("Nearest is at " + nearestObj.transform.position);
        }


    }


    public void WithoutHeadOld()
    {
        foreach (var hand in Hands)
        {
            SpawnedPoint nearestObj = PointsInCar.FindClosest(hand.transform.position);
            nearestObj.tag = "nearestpoint";
            First.tag = "nearestpoint";
            pts = GameObject.FindGameObjectWithTag("nearestpoint");
            renderers = pts.GetComponents<Renderer>();
            _isnearestfound = true;

            if (Vector3.Distance(nearestObj.transform.position, hand.transform.position) < 0.3f)
            {
                Debug.DrawLine(hand.transform.position, nearestObj.transform.position, Color.red);
                //nearestObj = best(nearestObj, First, Testraycast());
                Nearobpos = nearestObj.transform.localPosition;
                //nearobpostion = nearestObj.transform.localPosition;
                //nearoblocalpose = nearestObj.transform.position;
                nearobrot = nearestObj.transform.localRotation;
            }
/***
            if (First != nearestObj)
                First = nearestObj;
***/
        }


    }


    private void AdaptiveselectionwithHead(GameObject Plane)
    {
        ///Inputs point from kd tree and Returns projection of point on plane.
        foreach (var whiteball in Hands)
        {
            SpawnedPoint nearestObj = PointsInCar.FindClosest(whiteball.transform.position);
            nearestObj.tag = "nearestpoint";
            First.tag = "nearestpoint";
            pts = GameObject.FindGameObjectWithTag("nearestpoint");
            renderers = pts.GetComponents<Renderer>();

            Debug.DrawLine(whiteball.transform.position, nearestObj.transform.position, Color.red);

            nearestObj = best(nearestObj, First, Testraycast()); //finds nearest object based on headgaze

            var p1 = First;
            Debug.Log("p1 is at {0} " + p1.transform.position);
            var p2 = nearestObj;
            Debug.Log("p2 is at {0} " + p2.transform.position);

            //nearobpostion = nearestObj.transform.localPosition;
            //nearoblocalpose = nearestObj.transform.position;
            nearobrot = nearestObj.transform.localRotation;

            if (First != nearestObj)
                First = nearestObj;

            var pstar = nearestObj.transform.localPosition;
            Vector3 p1prime = PlaneProjection(p1, Plane);      
            Vector3 p2prime = PlaneProjection(p2, Plane);
  


            float d1 = _distance(p1prime, p1.transform.localPosition);
            //Debug.Log("d1 is +" + d1);
            float d2 = _distance(p2.transform.localPosition, p1.transform.localPosition);
            //Debug.Log("d2 is +" + d1);

            if ((2 * d1) >= d2)
            {
                pstar = p2.transform.localPosition;
                nearobpostion = pstar;
                //Debug.Log("Nearets obj at:" + pstar);
            }
            else
            {
                //pstar=Adaptiveselection(p1) //recursive call;
                pstar = p1prime;
                nearobpostion = pstar;

                //wait for a few seconds
                StartCoroutine(ProjectionCoroutine());
                pstar = p2prime;
                nearobpostion = pstar;

                //wait a few secs
                StartCoroutine(ProjectionCoroutine());
                pstar = p2.transform.localPosition;
                nearobpostion = pstar;
            }

        }
    }

    IEnumerator ProjectionCoroutine()
    {
        //Print the time of when the function is first called.
        Debug.Log("Started Coroutine at timestamp : " + Time.time);

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(0.8f);

        //After we have waited 5 seconds print the time again.
        Debug.Log("Finished Coroutine at timestamp : " + Time.time);
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
    public Vector3 getclosestobjectpose()
    {
        return _ClosestObject.transform.localPosition;
    }
    public Quaternion getclosestobjectrot()
    {
        return _ClosestObject.transform.rotation;
    }

    public void setposition(Vector3 pos)
    {
        nearobpostion = pos;
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
            // Debug.Log("Did not Hit");
        }

    }

    public SpawnedPoint best(SpawnedPoint first, SpawnedPoint second, Vector3 raycasthit)
    {
        float d1 = _distance(first.transform.position, raycasthit);
        float d2 = _distance(second.transform.position, raycasthit);
        //Add condition if in view

        if ((d1 <= d2) && IsVisible(first.GetComponent<Renderer>()))
            return first;
        else //if ((d2 <= d1) && IsVisible(second.GetComponent<Renderer>()))
            return second;
        //else
           // return first;
    }


    public SpawnedPoint best2(SpawnedPoint first, SpawnedPoint second, Vector3 raycasthit)
    {
        SpawnedPoint best = first;
        float d1 = _distance(first.transform.position, raycasthit);
        float d2 = _distance(second.transform.position, raycasthit);
        if (((d1 <= d2) && (d1 <= tol)) && IsVisible(first.GetComponent<Renderer>()))
            best=first;
        else if(((d2 <= d1) && (d2 <= tol)) && IsVisible(second.GetComponent<Renderer>()))
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
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        if (GeometryUtility.TestPlanesAABB(planes, renderer.bounds))
            return true;
        else
            return false;
    }

    public Vector3 PlaneProjection(SpawnedPoint start, GameObject Plane)
    {
        //Inputs point p 
        //returns position of projection of p on plane
        Vector3 Interpoint = Vector3.ProjectOnPlane(start.transform.localPosition, Plane.transform.up) + Vector3.Dot(Plane.transform.localPosition, Plane.transform.up) * Plane.transform.up; ;
        return Interpoint;
    }

    private void Adaptiveselection(SpawnedPoint p, GameObject Plane, SpawnedPoint closest)
    {
        //inputs point p
        //outpus optimal points or route.
        Vector3 pstar = closest.transform.position;
        Vector3 p1prime = PlaneProjection(p, Plane);
        Vector3 p2prime = PlaneProjection(closest, Plane);


        float d1 = _distance(p1prime, p.transform.position);
        float d2 = _distance(p2prime, p.transform.position);
        if ((2 * d1) >= d2)
        {
            pstar = closest.transform.position;
        }
        else
        {
            //pstar=Adaptiveselection(p1) //recursive call;
            pstar = p1prime;
            pstar = p2prime;
            pstar = closest.transform.position;
        }

    }
    
}



