using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectFace : MonoBehaviour
{
    // Start is called before the first frame update
    public enum Surface { VeryRough, Rough, Neutral, Smooth, VerySmooth};
    float rotation;
    public Surface mySurface;

    void Start()
    {
  
        mySurface = Surface.VerySmooth;
    }


    private void Update()
    {
        ChangeSurface();
    }

    
    public float ChangeSurface()
    {
        switch (mySurface)
        {

            case Surface.VeryRough:
                rotation = 0.0f;
                //Debug.Log("Very Rough Surface selected with rot" + rotation);
                break;
            case Surface.Rough:
                rotation = 72.0f;
               // Debug.Log("Rough Surface selected with rot" + rotation);
                break;
            case Surface.Neutral:
                rotation = 144.0f;
               // Debug.Log("Neutral Surface selected with rot" + rotation);
                break;
            case Surface.Smooth:
                rotation = 216.0f;
               // Debug.Log("Smooth Surface selected with rot" + rotation);
                break;
            case Surface.VerySmooth:
                rotation = 288.0f;
               // Debug.Log("Very Smooth Surface selected with rot" + rotation);
                break;
        }

        return rotation;
    }
    
}
