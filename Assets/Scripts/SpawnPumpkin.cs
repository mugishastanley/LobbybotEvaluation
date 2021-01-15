using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPumpkin : MonoBehaviour
{
    [SerializeField]
    private GameObject _FallingObject;
    [SerializeField]
    private GameObject _ObjectContainer;
    [SerializeField]
    private int num_of_spheres;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {

        while (num_of_spheres>0)
        {
            Vector3 posToSpawn = new Vector3(Random.Range(-0.4f, 0.4f), 2, 0);
            GameObject newEnemy = Instantiate(_FallingObject, posToSpawn, Quaternion.identity);
            newEnemy.transform.parent = _ObjectContainer.transform;
            num_of_spheres--;
            yield return new WaitForSeconds(5.0f);
            
        }
    }

 

}

