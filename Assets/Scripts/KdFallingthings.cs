using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KdFallingthings : MonoBehaviour
{
    public GameObject Hand;
    public GameObject BlackPrefab;
    public int Hands;
    public int CountBlack;
    private bool _isnearestfound = false;

    protected KdTree<FallingBlackObj> BlackballsList = new KdTree<FallingBlackObj>();
    protected KdTree<FallingBlackObj> WhiteballsList = new KdTree<FallingBlackObj>();
    // protected List<RandomMove> BlackballsList = new List<RandomMove>();

    // Spawn out balls at start of the game
    void Start()
    {

            StartCoroutine(SpawnRoutine());
        //}

        for (int i = 0; i < Hands; i++)
        {
            WhiteballsList.Add(Instantiate(Hand).GetComponent<FallingBlackObj>());

        }
    }

    // Update is called once per frame


    void Update()
    {
        BlackballsList.UpdatePositions();
        foreach (var whiteball in WhiteballsList)
        {
            FallingBlackObj nearestObj = BlackballsList.FindClosest(whiteball.transform.position);
            _isnearestfound = true;
            float dist = Vector3.Distance(whiteball.transform.position, nearestObj.transform.position);

           
           Debug.Log(nearestObj.gameObject.GetComponent<FallingBlackObj>().getId());
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
                BlackballsList.RemoveAt(GetComponent<FallingBlackObj>().getId());
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
            BlackballsList.Add(Instantiate(BlackPrefab, posToSpawn, Quaternion.identity).GetComponent<FallingBlackObj>());
            // newEnemy.transform.parent = _ObjectContainer.transform;
            BlackPrefab.GetComponent<FallingBlackObj>().setId(CountBlack);
           // BlackballsList[CountBlack].GetComponent<FallingBlackObj>().setId(CountBlack);
            CountBlack--;
            yield return new WaitForSeconds(5.0f);

        }
    }
}
