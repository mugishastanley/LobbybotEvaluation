using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectablecubes : MonoBehaviour
{
    private float _speed = 0.1f;
    public int ID; //unique ID for each Capsule

    //capsule constructor


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.right * _speed * Time.deltaTime);
        if (transform.position.y < -5f)
        {
            float randomX = Random.Range(-8f, 8f);
            transform.position = new Vector3(randomX, 7, 0);

        }
        //print("Position of capsule: " + transform.position);

    }

    private void OnTriggerEnter(Collider other)
    {


        if (other.tag == "Hand")
        {
            Hand player = other.transform.GetComponent<Hand>();
            if (player != null)
            {
                Destroy(this.gameObject);
            }


        }

    }
}

