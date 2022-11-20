using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ChunkTemplate : MonoBehaviour
{
    public const int chunkResolution = 8;
    public readonly int sBVC = chunkResolution + 1; // Simple Border Vertices Count
    public readonly int efBVC = chunkResolution / 2 + 1; // EdgeFan Border Vertices Count
    public readonly int sBTC = (chunkResolution - 1) * 6; // Simple Border Triangles Count
    public readonly int efBTC = (chunkResolution / 2) * 3 + (chunkResolution / 2 - 1) * 6; // EdgeFan Border Triangles Count

    public static int[] topVertices = new int[] { 6, 13, 20, 27, 34, 41, 48 };
    public static int[] botVertices = new int[]  { 0, 7, 14, 21, 28, 35, 42 };
    public static int[] leftVertices = new int[] { 0, 1, 2, 3, 4, 5, 6 };
    public static int[] rightVertices = new int[] { 42, 43, 44, 45, 46, 47, 48 };

    public Vector3[] vertices;
    public int[] triangles;
    int triangleOffset;
    int borderOffset;

    // { NoEF/Simple, TopEF, BotEF, LeftEF, RightEF, TopLeftEF, TopRightEF, BotLeftEF, BotRightEF }
    public static Vector3[][] templateVertices = new Vector3[9][];
    public static int[][] templateTriangles = new int[9][];

    /*private void Awake()
    {
        GenerateChunkTemplates();
    }*/

    private void Start()
    {
        GenerateChunkTemplates();
    }

    private void GenerateChunkTemplates()
    {

    }

    public void GenerateMiddle()
    {
        vertices = new Vector3[(chunkResolution + 1) * (chunkResolution + 1)];
        triangles = new int[chunkResolution * chunkResolution * 6];
        for (int y = 0; y < chunkResolution - 1; y++)
        {
            for (int x = 0; x < chunkResolution - 1; x++)
            {
                int i = x + y * (chunkResolution - 1);
                Vector3 position = new Vector3((x + 1) - chunkResolution * .5f, (y + 1) - chunkResolution * .5f, 0) / chunkResolution * 2;
                vertices[i] = position;

                if (x != chunkResolution - 2 && y != chunkResolution - 2)
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

/*

Possible configurations:
    noEdgeFan
    topEdgeFan
    leftEdgeFan
    rightEdgeFan
    botEdgeFan              = 9 variations
    topLeftEdgeFan
    topRightEdgeFan
    botLeftEdgeFan
    botRightEdgeFan

Possible, but unnecessary configurations, since there wont be lover LOD neighbour on opposite sides at once:
    topBotEdgeFan
    leftRightEdgeFan
    topLeftRightEdgeFan
    topLeftBotEdgeFan
    topRightBotEdgeFan
    leftRightBotEdgeFan
    topLeftRightBotEdgeFan
*/