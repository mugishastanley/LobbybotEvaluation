using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformMatrix : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        getMatrix();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void getMatrix()
    {
        // get matrix from the Transform
        var matrix = transform.localToWorldMatrix;
        // get position from the last column
        var position = new Vector3(matrix[0, 3], matrix[1, 3], matrix[2, 3]);
        //Debug.Log("Transform position from matrix is: " + position);
        //Debug.Log("Transform matrix string is: " + matrix.ToString());
    }
}
