using UnityEngine;


public class CubeMove : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Move()
    {
        Vector3 moveVector = new Vector3(10, 0, 0);
        transform.Translate(moveVector);
        
    }
}
