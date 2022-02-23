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
            points[1] = Vector3.zero + new Vector3(0, 0, hit.distance);
            rend.startColor = Color.red;
            rend.endColor = Color.red;
            btn = hit.collider.gameObject.GetComponent<Button>();
            hitBtn = true;
            // Debug.Log("Hit button "+btn.name);
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
            if (btn.name == "red_btn")
            {
                //Facenum = 1;
                img.color = Color.red;
                cubeRenderer.material.SetColor("_Color", Color.red);
                Debug.Log("Red");
            }
            else if (btn.name == "blue_btn")
            {
                //Facenum = 2;
                img.color = Color.blue;
                Debug.Log("Blue");
                cubeRenderer.material.SetColor("_Color", Color.blue);
            }
            else if (btn.name == "green_btn")
            {
                //Facenum = 3;
                img.color = Color.green;
                Debug.Log("Green");
                cubeRenderer.material.SetColor("_Color", Color.green);
            }

        }
    }


}