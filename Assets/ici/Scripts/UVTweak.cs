using UnityEngine;
using System.Collections;

public class UVTweak : MonoBehaviour
{
	public float uvScale = 1.0f;
	
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
		Vector2[] uvs = mesh.uv;
        Vector2[] new_uvs = new Vector2[uvs.Length];

        for (int i = 0; i < uvs.Length; i++)
        {
            new_uvs[i] = uvScale * new Vector2(uvs[i].x, uvs[i].y);
        }
        mesh.uv = new_uvs;
    }
}
