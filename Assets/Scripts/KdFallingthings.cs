using System.Collections;
using UnityEngine;

public class KdFallingthings : MonoBehaviour
{
    public GameObject Hand;
    public GameObject BlackPrefab;
    public int NumHands;
    public int CountBlack;
    private bool _isnearestfound = false;

    protected KdTree<SpawnedPoint> PointsInCar = new KdTree<SpawnedPoint>();
    protected KdTree<SpawnedPoint> Hands = new KdTree<SpawnedPoint>();
    // protected List<RandomMove> PointsInCar = new List<RandomMove>();

    // Spawn out balls at start of the game
    void Start()
    {

            StartCoroutine(SpawnRoutine());
        //}

        for (int i = 0; i < NumHands; i++)
        {
            Hands.Add(Instantiate(Hand).GetComponent<SpawnedPoint>());

        }
    }

    // Update is called once per frame


    void Update()
    {
        PointsInCar.UpdatePositions();
        foreach (var whiteball in Hands)
        {
            SpawnedPoint nearestObj = PointsInCar.FindClosest(whiteball.transform.position);
            _isnearestfound = true;
            float dist = Vector3.Distance(whiteball.transform.position, nearestObj.transform.position);

           
           Debug.Log(nearestObj.gameObject.GetComponent<SpawnedPoint>().getId());
           Debug.DrawLine(whiteball.transform.position, nearestObj.transform.position, Color.red);
            //change to a certain color
                var cubeRenderer = nearestObj.GetComponent<Renderer>();
                if (_isnearestfound)
                {
                    cubeRenderer.material.color = Color.red;
                    _isnearestfound = false;
                }
                _isnearestfound = false;

            if (dist < 1.0)
            {
                //delete the game object
                PointsInCar.RemoveAt(GetComponent<SpawnedPoint>().getId());
                //remove it form the tree
            }
        }
        // 
    }

    IEnumerator SpawnRoutine()
    {

        while (CountBlack > 0)
        {
            Vector3 posToSpawn = new Vector3(Random.Range(-8f, 8f), 7, 0);
            PointsInCar.Add(Instantiate(BlackPrefab, posToSpawn, Quaternion.identity).GetComponent<SpawnedPoint>());
            // newEnemy.transform.parent = _ObjectContainer.transform;
            BlackPrefab.GetComponent<SpawnedPoint>().setId(CountBlack);
           // PointsInCar[CountBlack].GetComponent<SpawnedPoint>().setId(CountBlack);
            CountBlack--;
            yield return new WaitForSeconds(5.0f);

        }
    }
}
