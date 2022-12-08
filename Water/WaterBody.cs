using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class WaterBody : MonoBehaviour
{
    float waterBodyRadius = 1000;

    Mesh mesh;
    Hashtable verticeSerialNumbers = new Hashtable();
    List<Vector3> meshVertices = new List<Vector3>();
    List<int> meshTriangles = new List<int>();

    private void Awake()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GenerateWaterBody();

        gameObject.AddComponent<MeshFilter>();
        gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    void GenerateWaterBody()
    {
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
        foreach (Vector3 direction in directions) 
        {
            CalculateVerticesTrianglesAndUVs(direction);
        }

        Vector2[] uvs = new Vector2[meshVertices.Count];

        float planetScriptSizeDivide = (1 / waterBodyRadius);
        float twoPiDivide = (1 / (2 * Mathf.PI));

        for (int i = 0; i < uvs.Length; i++)
        {
            Vector3 d = meshVertices[i] * planetScriptSizeDivide;
            float u = 0.5f + Mathf.Atan2(d.z, d.x) * twoPiDivide;
            float v = 0.5f - Mathf.Asin(d.y) / Mathf.PI;

            uvs[i] = new Vector2(u, v);
        }

        mesh.vertices = meshVertices.ToArray();
        mesh.triangles = meshTriangles.ToArray();
        mesh.uv = uvs;
        mesh.RecalculateNormals();
    }

    void CalculateVerticesTrianglesAndUVs(Vector3 localUp)
    {
        int chunkResolution = 128;

        Vector3 axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        Vector3 axisB = Vector3.Cross(localUp, axisA);

        Vector3[,] vertices = new Vector3[(chunkResolution + 1), (chunkResolution + 1)];
        List<(int, int)> triangles = new List<(int, int)>();

        for (int y = 0; y < chunkResolution + 1; y++)
        {
            for (int x = 0; x < chunkResolution + 1; x++)
            {
                Vector2 percent = new Vector2(x, y) / (chunkResolution);
                Vector3 position = (localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB).normalized * waterBodyRadius;
                vertices[y, x] = position;
                if (x == 0 || y == 0 || x == chunkResolution || y == chunkResolution)
                {
                    CheckVertexInHashTable(position);
                }
                else
                {
                    verticeSerialNumbers.Add(position, verticeSerialNumbers.Count);
                    meshVertices.Add(position);
                }

                if (y < chunkResolution && x < chunkResolution)
                    AddTriangles(y, x, triangles);
            }
        }

        foreach (var tri in triangles) 
        {
            meshTriangles.Add( (int) verticeSerialNumbers[ vertices[ tri.Item1, tri.Item2 ] ]);
        }
    }

    void AddTriangles(int y, int x, List<(int, int)> triangles)
    {
        if ((y + x) % 2 == 0)
        {
            triangles.Add((y, x));
            triangles.Add((y, x + 1));
            triangles.Add((y + 1, x));

            triangles.Add((y + 1, x));
            triangles.Add((y, x + 1));
            triangles.Add((y + 1, x + 1));
        }
        else
        {
            triangles.Add((y, x));
            triangles.Add((y + 1, x + 1));
            triangles.Add((y + 1, x));

            triangles.Add((y, x));
            triangles.Add((y, x + 1));
            triangles.Add((y + 1, x + 1));
        }
    }

    void CheckVertexInHashTable(Vector3 vertex)
    {
        if (!verticeSerialNumbers.ContainsKey(vertex))
        {
            meshVertices.Add(vertex);
            verticeSerialNumbers.Add(vertex, verticeSerialNumbers.Count);
        }
    }
}
