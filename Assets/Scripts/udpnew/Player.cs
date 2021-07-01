using UnityEngine;

public class Player : MonoBehaviour
{
    private SpawnManager _spawnManager;
    


    void Start()
    {
        _spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>(); //find the object and get the component
        //start player at 0,0,0 by default
        //but remember player is the controller
        transform.position = new Vector3(0, 0, 0);
        if (_spawnManager == null)
        {
            Debug.LogError("Debug Manager is null");
        }
    }
}
