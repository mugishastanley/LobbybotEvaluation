using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadGaze : MonoBehaviour
{
    // Detects manually if obj is being seen by the main camera

    GameObject obj;
    Collider objCollider;
    Camera cam;
    Plane[] planes;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        planes = GeometryUtility.CalculateFrustumPlanes(cam);
        objCollider = GetComponent<Collider>();

    }

    // Update is called once per frame

    void Update()
    {
        Testraycast();
    }

    public void TestGaze() {
        if (GeometryUtility.TestPlanesAABB(planes, objCollider.bounds))
        {
            Debug.Log(obj.name + " has been detected!");
        }
        else
        {
            Debug.Log("Nothing has been detected");
        }
    }

    public void Testraycast() {

        RaycastHit hitInfo;
        if (Physics.Raycast(
                cam.transform.position,
                cam.transform.forward,
                out hitInfo,
                20.0f,
                Physics.DefaultRaycastLayers))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hitInfo.distance, Color.black);
            //Debug.Log("Did Hit " +hitInfo.point);
            // If the Raycast has succeeded and hit a hologram
            // hitInfo's point represents the position being gazed at
            // hitInfo's collider GameObject represents the hologram being gazed at
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            //Debug.Log("Did not Hit");
        }

    }
}
