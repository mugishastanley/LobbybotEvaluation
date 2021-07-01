using System.Collections.Generic;
using UnityEngine;

public class FindClosest : MonoBehaviour
{
    public GameObject WhitePrefab;
    public GameObject BlackPrefab;
    //private int CountWhite;
    public int CollectableObjects;
    protected List <GameObject> PointsInCar = new List<GameObject>();

    // Spawn out balls at start of the game
    void Start()
    {
        Init();
    }

    private void Init()
    {
        for (int i = 0; i < CollectableObjects; i++)
        {
            Vector3 spawnPosition = new Vector3(Random.Range(-5, 5), 1, Random.Range(-5, 5));
            Quaternion spawnRotation = Quaternion.identity;
            PointsInCar.Add(Instantiate(BlackPrefab, spawnPosition, spawnRotation));
        }
    }

    // Update is called once per frame
    void Update()
    {
       IterativeAlgo();
    }

    //Iterative algo
    void IterativeAlgo (){
        //foreach (var whiteball in Hands)
        {
            var nearestDist = float.MaxValue;
            GameObject nearestObj = null;

            foreach (var blackball in PointsInCar)
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
