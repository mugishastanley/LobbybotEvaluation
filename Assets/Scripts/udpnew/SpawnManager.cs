using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private int number_of_pumpkins;
    [SerializeField]
    private GameObject PumpkinContainer;
    [SerializeField]
    private GameObject PumpkinPrefab;
    private bool _StopSpawning = false;

    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(SpawnPumpkinRoutine());
    }
    IEnumerator SpawnPumpkinRoutine()
    {
        
        while (true && !_StopSpawning)
        {
            Vector3 posToSpawn = new Vector3(Random.Range(-0.6f, 0.7f), Random.Range(1.5f, 2.0f), 0.2f);
            GameObject newPumpkin = Instantiate(PumpkinPrefab, posToSpawn, Quaternion.identity);
            newPumpkin.transform.parent = PumpkinContainer.transform;
            yield return new WaitForSeconds(2.5f);
            number_of_pumpkins--;
            if (number_of_pumpkins == 0) { _StopSpawning = true; }
        }
    }
}
