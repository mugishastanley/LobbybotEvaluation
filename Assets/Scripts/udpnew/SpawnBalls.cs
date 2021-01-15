using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpawnBalls : MonoBehaviour
{
    [SerializeField]
    private int num_projectile;
    [SerializeField]
    private GameObject projectile;
    [SerializeField]
    private GameObject PlatformBallPosition;
    private bool _StopSpawning = false;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Spawnprojectile());
    }

    IEnumerator Spawnprojectile()
    {
        //when a ball hits the floor, spawn another one, wait 5 seconds then destroy the ball.
        

        while (true && !_StopSpawning)
        {
            Vector3 SpawnPos = PlatformBallPosition.transform.position;
            GameObject NewProjectile = Instantiate(projectile, SpawnPos, Quaternion.identity, PlatformBallPosition.transform);
            yield return new WaitForSeconds(4.0f);
            num_projectile--;
            if (num_projectile == 0) { _StopSpawning = true; }
        }
    }

    // Update is called once per frame
    void Update()
    {
         
    }
}
