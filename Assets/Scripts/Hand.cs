using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {
        /**
        //give initial starting point for the hand
        _spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>(); //find the object and get the component

        
        if (_spawnManager == null)
        {
            Debug.LogError("Debug Manager is null");
        }

    **/
        transform.position = new Vector3(0, 0, 0);

    }

    // Update is called once per frame
    void Update()
    {
       // CalculateMovement();
    }
}
