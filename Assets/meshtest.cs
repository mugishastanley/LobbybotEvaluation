using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class meshtest : MonoBehaviour
{
    // Distorts the mesh vertically.
    void Update()
    {
        /**
        // Get instantiated mesh
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        // Randomly change vertices
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        int p = 0;
        while (p < vertices.Length)
        {
            vertices[p] += new Vector3(0, Random.Range(-0.3F, 0.3F), 0);
            p++;
        }
        while (p < normals.Length)
        {
            normals[p] += new Vector3(0, Random.Range(-0.3F, 0.3F), 0);
            p++;
        }
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.RecalculateNormals();

        **/
    }
}
