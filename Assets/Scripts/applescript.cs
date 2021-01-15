using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class applescript : MonoBehaviour
{
    private float _speed = 0.05f;
    //public static int scorecount = 0;
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Hand")
        {
            Debug.Log("Collided with Hand");
            Destroy(this.gameObject);
        }
    }


    private void pumpkinfall()
    {
        transform.Translate(Vector3.down * _speed * Time.deltaTime);
        if (transform.position.y < -1f)
        {
            float randomX = Random.Range(-0.6f, 0.7f);
            transform.position = new Vector3(randomX, 2, 0.2f);
        }
    }

    void Update()
    {
        pumpkinfall();
    }
}
