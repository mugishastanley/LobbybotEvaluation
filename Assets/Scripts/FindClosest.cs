using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindClosest : MonoBehaviour
{
    public GameObject WhitePrefab;
    public GameObject BlackPrefab;
    //private int CountWhite;
    public int CollectableObjects;
    protected List <GameObject> BlackballsList = new List<GameObject>();

    // Spawn out balls at start of the game
    void Start()
    {
        for (int i = 0; i < CollectableObjects; i++) {
            Vector3 spawnPosition = new Vector3(Random.Range(-5, 5), 1, Random.Range(-5, 5));
            Quaternion spawnRotation = Quaternion.identity;
            BlackballsList.Add(Instantiate(BlackPrefab, spawnPosition, spawnRotation));
        }
    }

    // Update is called once per frame
    void Update()
    {
        var nearestDist = float.MaxValue;
        GameObject nearestObj = null;
        foreach (var blackball in BlackballsList)
        {
            if (Vector3.Distance(WhitePrefab.transform.position, blackball.transform.position) < nearestDist)
            {
                nearestDist = Vector3.Distance(WhitePrefab.transform.position, blackball.transform.position);
                nearestObj = blackball;
            }
        }
        Debug.DrawLine(WhitePrefab.transform.position, nearestObj.transform.position, Color.red);
        Debug.Log("Nearest distance:" + nearestDist);
    }

    //Iterative algo
    void iterativeAlgo (){
        //foreach (var whiteball in WhiteballsList)
        {
            var nearestDist = float.MaxValue;
            GameObject nearestObj = null;

            foreach (var blackball in BlackballsList)
            {
                if (Vector3.Distance(WhitePrefab.transform.position, blackball.transform.position) < nearestDist)
                {
                    nearestDist = Vector3.Distance(WhitePrefab.transform.position, blackball.transform.position);
                    nearestObj = blackball;
                }
            }
            Debug.DrawLine(WhitePrefab.transform.position, nearestObj.transform.position, Color.red);
            Debug.Log("Nearest distance:" + nearestDist);
        }
    }
}
