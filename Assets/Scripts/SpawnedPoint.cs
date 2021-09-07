using UnityEngine;

public class SpawnedPoint : MonoBehaviour
{
    //public int id = 0;

    public string Id { get; set; }
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        // this.transform.position= GetComponent<LoadData>()
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
    
}