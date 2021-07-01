using UnityEngine;
using UnityEngine.UI;

public class LineRenderSettings : MonoBehaviour
{
    public GameObject panel;
    private Image img;
    public Button btn;

    //Declare a Line Renderer to store the component attached to the Game object
    [SerializeField]
    LineRenderer rend;

    //settings for the lineRenderer are stored as a Vector3 array of points. set up a v3 array to 
    //initialize in start
    Vector3[] points;

    // Start is called before the first frame update
    void Start()
    {
        img = panel.GetComponent<Image>();
        //get the Line renderer attached to the gameobject

        rend = gameObject.GetComponent<LineRenderer>();
        //initialize the LineRenderer
        points = new Vector3[2];

        //set the start point of the Linerender to the position of the game object
        points[0] = Vector3.zero;

        //set the end point 20 units awy from the GO on the z axis (poininting forward)
        points[1] = transform.position + new Vector3(0, 0, 20);

        //finally set the positions array on the LineRenderer to our new values

        rend.SetPositions(points);
        rend.enabled = true;

        
        
    }

    //
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
            points[1] = transform.forward + new Vector3(0, 0, hit.distance);
            rend.startColor = Color.red;
            rend.endColor = Color.red;
            btn = hit.collider.gameObject.GetComponent<Button>();
            hitBtn = true;
        }
        else
        {
            points[1] = transform.forward + new Vector3(0, 0, 20);
            rend.startColor = Color.green;
            rend.endColor = Color.green;
            hitBtn = false;
        }
        rend.SetPositions(points);
        rend.material.color = rend.startColor;
        //btn = hit.collider.gameObject.GetComponent<Button>();
        return hitBtn;
    }

    public void ColorChangeOnClick() 
    {
        if(btn != null)
        {
            if (btn.name == "red_btn")
            {
                img.color = Color.red;
                Debug.Log("Red");
            }
            else if (btn.name == "blue_btn")
            {
                img.color = Color.blue;
                Debug.Log("Blue");
            }
            else if (btn.name == "green_btn")
            {
                img.color = Color.green;
                Debug.Log("Green");
            }


        }



    }

    // Update is called once per frame
    void Update()
    {
        AlignLineRender(rend);
        rend.material.color = rend.startColor;

        if (AlignLineRender(rend) && Input.GetAxis("Submit") > 0)
        {
            btn.onClick.Invoke();
        }
    }
}
