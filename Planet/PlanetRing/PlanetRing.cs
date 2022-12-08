using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring : MonoBehaviour
{
    public int segments = 20;
    public float innerRadius = 2000f;
    public float thickness = 2000f;
    public Material ringMaterial;

    GameObject planetRing;
    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    void Start()
    {
        planetRing = new GameObject(name + " Ring");
        planetRing.transform.parent = transform;
        planetRing.transform.localScale = Vector3.one;
        planetRing.transform.localPosition = Vector3.zero;
        planetRing.transform.localRotation = Quaternion.identity;
        
        meshFilter = planetRing.AddComponent<MeshFilter>();
        mesh = meshFilter.mesh;
        meshRenderer = planetRing.AddComponent<MeshRenderer>();
        meshRenderer.material = ringMaterial;

        Vector3[] vertices = new Vector3[(segments + 1) * 4];
        int[] triangles = new int[segments * 12];
        Vector2[] uv = new Vector2[(segments + 1) * 4];

        int halfWay = (segments + 1) * 2;

        for (int i = 0; i < segments + 1; i++)
        {
            float progress = (float)i / (float)segments;
            float angle = Mathf.Deg2Rad * progress * 360;

            float x = Mathf.Sin(angle);
            float z = Mathf.Cos(angle);

            vertices[i * 2] = vertices[i * 2 + halfWay] = new Vector3(x, 0f, z) * (innerRadius + thickness);
            vertices[i * 2 + 1] = vertices[i * 2 + 1 + halfWay] = new Vector3(x, 0f, z) * innerRadius;
            uv[i * 2] = uv[i * 2 + halfWay] = new Vector2(progress, 0f);
            uv[i * 2 + 1] = uv[i * 2 + 1 + halfWay] = new Vector2(progress, 1f);

            if (i != segments)
            {
                triangles[i * 12] = i * 2;
                triangles[i * 12 + 1] = triangles[i * 12 + 4] = (i + 1) * 2;
                triangles[i * 12 + 2] = triangles[i * 12 + 3] = i * 2 + 1;
                triangles[i * 12 + 5] = (i + 1) * 2 + 1;

                triangles[i * 12 + 6] = i * 2 + halfWay;
                triangles[i * 12 + 7] = triangles[i * 12 + 10] = i * 2 + 1 + halfWay;
                triangles[i * 12 + 8] = triangles[i * 12 + 9] = (i + 1) * 2 + halfWay;
                triangles[i * 12 + 11] = (i + 1) * 2 + 1 + halfWay;
            }
        }

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
    }
}
