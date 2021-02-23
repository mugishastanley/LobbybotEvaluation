using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class KdFindClosestMesh : MonoBehaviour
{
    public Button InitialiseButton;
    //public Transform car; 
    public GameObject WhitePrefab;
    public GameObject BlackPrefab;
    
    public Transform CalTracker;
    public int CountWhite;
    public int CountBlack;
    public GameObject[] points;
    public GameObject TouchableObjects;
    //private GameObject _closestobjectpose;
    private GameObject _ClosestObject;
    private bool _isnearestfound = false;

    private Vector3 nearobpostion;
    private Quaternion nearobrot;


    protected KdTree<FallingBlackObj> BlackballsList = new KdTree<FallingBlackObj>();
    protected KdTree<FallingBlackObj> WhiteballsList = new KdTree<FallingBlackObj>();


    // protected List<RandomMove> BlackballsList = new List<RandomMove>();

    private MeshFilter mf;
    private Vector3[] origVerts;
    private Vector3[] newVerts;

    // Spawn out balls at start of the game
    void Start()
    {
        //InitialiseButton.onClick.AddListener(delegate {init();});
        init();
    }

  

    public void init() {

        mf = TouchableObjects.GetComponent<MeshFilter>();
        origVerts = mf.mesh.vertices;
        newVerts = new Vector3[origVerts.Length];

        for (int i = 0; i < origVerts.Length; i++)
        {
            //newVertices [i]= mf.mesh.vertices[i];
            Debug.Log("Before " + origVerts[i]);
            //newVerts[i] = localToWorldMatrix.MultiplyPoint3x4(origVerts[i]);
            newVerts[i] = transform.TransformPoint(origVerts[i]);
            Debug.Log("After " + newVerts[i] );
        }

        //Extract Mesh Information.

        /****
        TouchableObjects = GameObject.FindGameObjectsWithTag("Touchable");
        //TouchableObjects[i].transform.TransformPoint(mesh.vertices[vert]);
        foreach (GameObject go in TouchableObjects)
        {
            //transform.TransformPoint(mesh.vertices[vert]);
            //transform vertices position into world space
            Mesh mesh = go.GetComponent<MeshFilter>().mesh;
            testing = new Vector3[mesh.vertices.Length];
            for (int test = 0; test < mesh.vertices.Length; test++)
            {
                go.transform.TransformPoint(mesh.vertices[test]);
                Debug.Log("Testing Vertex info" + mesh.vertices[test]);
            }
        }
        ***/



            for (int i = 0; i < points.Length; i++)
        //foreach (var point in points)
        {
            //Quaternion spawnRotation = Quaternion.identity;

            //GameObject point1 = (Instantiate(BlackPrefab, new Vector3(-0.846f, -0.1983f, 0.747f), spawnRotation));
            //Debug.Log("Before tracker parent: point " + i + "Posiotn:" + points[i].transform.position.ToString("F3"));
            //points[i].transform.parent = CalTracker.transform;          
            //Debug.Log("After tracker parent: Point :" + i + "Posiotn:" + points[i].transform.position.ToString("F3"));

            points[i].transform.parent = CalTracker.transform; //only for visuals but no effect
            //Debug.Log("After tracker parent: Point :" + i + "Posiotn:" + points[i].transform.position.ToString("F3"));
            GameObject point = (Instantiate(BlackPrefab, points[i].transform.position, points[i].transform.rotation));
            //Debug.Log("Spawn at Point" + i + "Posiotn:" + point.transform.position.ToString("F3"));
            point.transform.parent = CalTracker.transform;
            //Debug.Log("After parent Point" + i + "Posiotn:" + point.transform.position.ToString("F3"));
           // Debug.Log("Spawn at Point" + i + "Posiotn:" + points[i].transform.position.ToString("F3"));
            //point.transform.parent = CalTracker.transform;
            //Debug.Log("after Parent at Point" + i + "Posiotn:" + point.transform.position.ToString("F3"));
            //point.transform.position = points[i].transform.position;
            //Debug.Log("Position based on Point transform" + i + "Posiotn:" + point.transform.position.ToString("F3"));
            //point.transform.localRotation = points[i].transform.localRotation;
            BlackballsList.Add((point).GetComponent<FallingBlackObj>());

            // StartCoroutine(SpawnRoutine());
        }

        for (int i = 0; i < CountWhite; i++)
        {
            WhiteballsList.Add(Instantiate(WhitePrefab).GetComponent<FallingBlackObj>());
        }


    }

    // Update is called once per frame
    

    void Update()
    { 
        BlackballsList.UpdatePositions();
        foreach (var whiteball in WhiteballsList)
        {
            FallingBlackObj nearestObj = BlackballsList.FindClosest(whiteball.transform.position);

            _isnearestfound = true;

            Debug.DrawLine(whiteball.transform.position, nearestObj.transform.position, Color.red);
           // _ClosestObject.transform.position = nearestObj.transform.localPosition;
            nearobpostion = nearestObj.transform.localPosition;
            nearobrot = nearestObj.transform.localRotation;
            //Debug.Log("From Kd Found next " + nearestObj.transform.localPosition.ToString("F3")); // This is the final location to send.


            //ClosestObject.transform.position = nearestObj.transform.position;
            //change to a certain color        

            //var cubeRenderer = nearestObj.GetComponent<Renderer>();
            if (_isnearestfound)
            {
                var cubeRenderer = nearestObj.GetComponent<Renderer>();
                cubeRenderer.material.color = Color.red;
                //Call SetColor using the shader property name "_Color" and setting the color to red
                cubeRenderer.material.SetColor("_Color", Color.red);
                _isnearestfound = false;
            }
           // _isnearestfound = false;
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

    //extract the Mesh 


}
