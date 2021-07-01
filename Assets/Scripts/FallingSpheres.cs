using UnityEngine;

public class FallingSpheres : MonoBehaviour
{
    private float _speed = 2f;
    // Start is called before the first frame update

    void Update()
    {

        transform.Translate(Vector3.down * _speed * Time.deltaTime);

        if (transform.position.y < -5f)
        {
            float randomX = Random.Range(-8f, 8f);
            transform.position = new Vector3(randomX, 7, 0);

        }

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
