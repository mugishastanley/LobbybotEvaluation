using UnityEngine;
using UnityEngine.UI;

public class LineRenderSettings : MonoBehaviour
{
    public GameObject panel;
    private Image img;
    public Button btn;
    public GameObject cube;
    public int Facenum { get; set; }
    


    //Declare a Line Renderer to store the component attached to the Game object
    [SerializeField] LineRenderer rend;
 
    //settings for the lineRenderer are stored as a Vector3 array of points. set up a v3 array to 

    Vector3[] points;

    void Start()
    {
        
        //get the Line renderer attached to the gameobject

        rend = gameObject.GetComponent<LineRenderer>();
        img = panel.GetComponent<Image>();

        points = new Vector3[2];
        
        //set the start point of the Linerender to the position of the game object
        points[0] = Vector3.zero;
        
        //set the end point 20 units awy from the GO on the z axis (pointing forward)
        points[1] = transform.position + new Vector3(0, 0, 20);

        //finally set the positions array on the LineRenderer to our new values

        rend.SetPositions(points);
        rend.enabled = true;

    }

    public LayerMask layerMask;
    
    public bool AlignLineRender(LineRenderer rend)
    {
        
        Ray ray;
        ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction);
        bool hitBtn = false;
        if (Physics.Raycast(ray, out hit, layerMask))
        {
            //points[1] = Vector3.zero + new Vector3(0, 0, hit.distance);
            points[1] = Vector3.zero + new Vector3(0, 0, 40f);
            rend.startColor = Color.red;
            rend.endColor = Color.red;
            btn = hit.collider.gameObject.GetComponent<Button>();
            hitBtn = true;
            Debug.Log("Hit button "+btn.name);
            ColorChangeOnClick();
        }
        else
        {
            points[1] = Vector3.zero + new Vector3(0, 0, 100);
            rend.startColor = Color.green;
            rend.endColor = Color.green;
            hitBtn = false;
        }
        rend.SetPositions(points);
        rend.material.color = rend.startColor;
        //btn = hit.collider.gameObject.GetComponent<Button>();
        return hitBtn;
    }

    void Update()
    {
        
        AlignLineRender(rend);
        rend.material.color = rend.startColor;

        if (AlignLineRender(rend) && Input.GetAxis("Submit") > 0)
        {
            btn.onClick.Invoke();
        }
    }
    
    public void ColorChangeOnClick() 
    {
        var cubeRenderer = cube.GetComponent<Renderer>();
        if(btn != null)
        {
            switch (btn.name)
            {
                case "red_btn":
                    Facenum = 1;
                    //img.color = Color.red;
                    cubeRenderer.material.SetColor("_Color", Color.red);
                    Debug.Log("Red");
                    break;
                case "green_btn":
                    Facenum = 2;
                    //img.color = Color.green;
                    Debug.Log("Green");
                    cubeRenderer.material.SetColor("_Color", Color.green);
                    break;
                case "blue_btn":
                    Facenum = 3;
                    //img.color = Color.blue;
                    Debug.Log("Blue");
                    cubeRenderer.material.SetColor("_Color", Color.blue);
                    break;
                case "black_btn":
                    Facenum = 4;
                    //img.color = Color.black;
                    //Debug.Log("Black");
                    cubeRenderer.material.SetColor("_Color", Color.black);
                    break;
                case "yellow_btn":
                    Facenum = 5;
                    //img.color = Color.yellow;
                    //Debug.Log("Green");
                    cubeRenderer.material.SetColor("_Color", Color.yellow);
                    break;
            }

            //Debug.Log("Btn name ="+btn.name);
        }
        else
        {
            Debug.Log("Btn is null");
        }
    }
    
    public void BlueFace()
    {
        //Opens Data stream  from UNity to robot
        Facenum = 3;
        var cubeRenderer = cube.GetComponent<Renderer>();
            //GameObject.Find("Home").GetComponentInChildren<Text>().text = "Click to Start";
            img.color = Color.blue;
            Debug.Log("Blue");
            cubeRenderer.material.SetColor("_Color", Color.blue);

    }
    
    public void RedFace()
    {
        //Opens Data stream  from UNity to robot
        //
             Facenum = 1;
           var cubeRenderer = cube.GetComponent<Renderer>();
            //GameObject.Find("Home").GetComponentInChildren<Text>().text = "Click to Start";
            img.color = Color.red;
            Debug.Log("Blue");
            cubeRenderer.material.SetColor("_Color", Color.red);
    }
    public void GreenFace()
        {
            //Opens Data stream  from UNity to robot
            //
                Facenum = 2;
               var cubeRenderer = cube.GetComponent<Renderer>();
                //GameObject.Find("Home").GetComponentInChildren<Text>().text = "Click to Start";
                img.color = Color.green;
                Debug.Log("green");
                cubeRenderer.material.SetColor("_Color", Color.green);
        }
        
        
        public void BlackFace()
        {
            //Opens Data stream  from UNity to robot
            //
            Facenum = 4;
            var cubeRenderer = cube.GetComponent<Renderer>();
            //GameObject.Find("Home").GetComponentInChildren<Text>().text = "Click to Start";
            img.color = Color.black;
            Debug.Log("Blue");
            cubeRenderer.material.SetColor("_Color", Color.black);
        }
        
        public void YellowFace()
        {
            //Opens Data stream  from UNity to robot
            //
            Facenum = 5;
            var cubeRenderer = cube.GetComponent<Renderer>();
            //GameObject.Find("Home").GetComponentInChildren<Text>().text = "Click to Start";
            img.color = Color.yellow;
            Debug.Log("Blue");
            cubeRenderer.material.SetColor("_Color", Color.yellow);
        }
        
}