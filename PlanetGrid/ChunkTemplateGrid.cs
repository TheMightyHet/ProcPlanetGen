using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkTemplateGrid : MonoBehaviour
{
    public const int chunkResolution = 16;
    public Vector3[] vertices;
    public int[] triangles;
    int triangleOffset;

    private void Awake()
    {
        for (int y = 0; y < chunkResolution +1; y++)
        {
            for (int x = 0; x < chunkResolution + 1; x++)
            {
                int i = x + y * (chunkResolution + 1);
                Vector3 position = new Vector3(x  - chunkResolution * .5f, y - chunkResolution * .5f, 0) / chunkResolution * 2;
                vertices[i] = position;

                if (x != chunkResolution && y != chunkResolution)
                {
                    triangles[triangleOffset] = i;
                    triangles[triangleOffset + 1] = i + (chunkResolution - 1) + 1;
                    triangles[triangleOffset + 2] = i + (chunkResolution - 1);

                    triangles[triangleOffset + 3] = i;
                    triangles[triangleOffset + 4] = i + 1;
                    triangles[triangleOffset + 5] = i + (chunkResolution - 1) + 1;

                    triangleOffset += 6;
                }
            }
        }
    }
}
