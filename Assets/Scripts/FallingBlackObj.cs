using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingBlackObj : MonoBehaviour
{
    public int id = 0;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.tag == "Hand")
        {
            Hand player = other.transform.GetComponent<Hand>();
            if (player != null)
            {
                //Destroy(this.gameObject);
            }
        }


    }

    public void setId(int i) {
        id = i;
    }
    public int getId() {
        return id;
    }
}
