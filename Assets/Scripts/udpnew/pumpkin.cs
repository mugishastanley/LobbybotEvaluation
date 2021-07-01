using UnityEngine;


public class pumpkin : MonoBehaviour
{
    private float _speed = 4.0f;
    //public static int scorecount = 0;
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Hand")
        {
            //check if player is in scene
            Player player = other.transform.GetComponent<Player>();
            if (player != null)
            {
               
            }
            Debug.Log("Collided with Hand");
            Destroy(this.gameObject);
        }
    }


    private void pumpkinfall() {
        transform.Translate(Vector3.down * _speed * Time.deltaTime);
        if (transform.position.y < -3f)
        {
            float randomX = Random.Range(-1f, 1f);
            transform.position = new Vector3(randomX, 4, 0);
        }
    }



    void Update()
    {
        

    }
}
