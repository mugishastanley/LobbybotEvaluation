using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    [SerializeField]
    private Material highlightMaterial;

    Camera cam;
    Vector3 pos = new Vector3(200, 200, 0);
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = cam.ScreenPointToRay(pos);
        //var ray = Camera.main.ScreenPointToRay(3.0f,2.0f,0.0f);

        Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);

        RaycastHit hit;

        if(Physics.Raycast(ray,out hit))
        {
            var selection = hit.transform;
            var selectionRenderer = selection.GetComponent<Renderer>();
            if(selectionRenderer != null)
            {
                selectionRenderer.material = highlightMaterial;

            }
        }
        
    }
}
