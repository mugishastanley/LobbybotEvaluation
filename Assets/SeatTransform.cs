using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeatTransform : MonoBehaviour
{
    public GameObject Seat;
    Matrix4x4 DTS;

    // Start is called before the first frame update
    void Start()
    {
       
        DTS[0, 0] = 0.99994f;
        DTS[0, 1] = -0.01134f;
        DTS[0, 2] = 0.00000f;
        DTS[0, 3] = -0.00011f;
        DTS[1, 0] = 0.01134f;
        DTS[1, 1] = 0.99994f;
        DTS[1, 2] = 0.00000f;
        DTS[1, 3] = 0.01000f;
        DTS[2, 0] = 0.00000f;
        DTS[2, 1] = 0.00000f;
        DTS[2, 2] = 1.0f;
        DTS[3, 0] = 0.00000f;
        DTS[3, 1] = 0.00000f;
        DTS[3, 2] = 0.00000f;
        DTS[3, 3] = 1.00000f;

        Seat.transform.position = new Vector3(DTS[0, 3], DTS[1, 3], DTS[2, 3]);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
