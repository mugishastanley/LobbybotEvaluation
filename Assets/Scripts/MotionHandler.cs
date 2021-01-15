using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionHandler : MonoBehaviour
{
   
    //[SerializeField]
    private Hand _Hand;
    [SerializeField]
    private GameObject _Falling_capsule;

    [SerializeField]
    private int _numCapsules;
    [SerializeField]
    private List<GameObject> _Capsule_list= new List<GameObject>();// List to hold Capsules
    private FallingCapsule Capsule;

    void Awake()
    {
        _Capsule_list = new List<GameObject>();
    }

    // Start is called before the first frame update
    void Start()
    {
          StartCoroutine(SpawnRoutine());
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    void calculate_distance() {

        float dist = Vector3.Distance(_Hand.transform.position, _Falling_capsule.transform.position);
        print("Distance to other: " + dist);
    }

    IEnumerator SpawnRoutine()
    {
        /**
        while (true)
        {
            
            Vector3 posToSpawn = new Vector3(Random.Range(-8f, 8f), 7, 0);
            GameObject newEnemy = Instantiate(_Falling_capsule, posToSpawn, Quaternion.identity);
            //newEnemy.transform.parent = _ObjectContainer.transform;
            yield return new WaitForSeconds(8.0f);
        **/

        for (int i = 0; i < _numCapsules; i++)
            {
                Vector3 spawnPosition = new Vector3(Random.Range(-5, 5), 1, Random.Range(-5, 5));
                Quaternion spawnRotation = Quaternion.identity;
                Instantiate(_Falling_capsule, spawnPosition, spawnRotation);
                _Capsule_list.Add(_Falling_capsule);
                 //Capsule.ID = i;
                 Debug.Log("i = " + i + " : " + _Capsule_list[i].transform.position);
                 yield return new WaitForSeconds(3.0f);
             }
           
        //}
    }



    /* 
     * Motion prediction for the user
     * Input current position and direction of the hand,position of various objects
     * Calculate distance between a spawned object and the hand 
     * return the n-objects with shortest distance
     * Among the retturned objects
     * check the direction vector of each 
     * pick the object with the direcn vector facing the hand
     *
     * 
     * Some scenarios
     * Objects moving Hand moving
     * Objects stationary Hand moving
     * Hand stationary objects dynamic
     * 
     * 
     *Data structures
     *Array of vectors containing positions and orientation of size n
     * 
     * Additionals
     * Nice Environment
     * cool dynamic materials
     * cool visual effects
     * 
     * 
     * 
     */


}
