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
        // NoEFGen
        GenerateChunkTemplate_NoEdgeFans();

        // OneEFGen
        GenerateChunkTemplate_TopEdgeFan();
        GenerateChunkTemplate_BotEdgeFan();
        GenerateChunkTemplate_LeftEdgeFan();
        GenerateChunkTemplate_RightEdgeFan();

        // TwoEFGen
        GenerateChunkTemplate_TopLeftEdgeFan();
        GenerateChunkTemplate_TopRightEdgeFan();
        GenerateChunkTemplate_BotLeftEdgeFan();
        GenerateChunkTemplate_BotRightEdgeFan();
    }

    private void GenerateChunkTemplate_NoEdgeFans()
    {
        vertices = new Vector3[((chunkResolution - 1) * (chunkResolution - 1)) + sBVC + sBVC + sBVC + sBVC];
        triangles = new int[((chunkResolution - 2) * (chunkResolution - 2) * 6) + sBTC + sBTC + sBTC + sBTC];
        triangleOffset = 0;
        borderOffset = 0;

        GenerateMiddle();

        GenerateTop();
        GenerateBot();
        GenerateLeft();
        GenerateRight();

        templateVertices[0] = vertices;
        templateTriangles[0] = triangles;
    }

    private void GenerateChunkTemplate_TopEdgeFan()
    {
        vertices = new Vector3[((chunkResolution - 1) * (chunkResolution - 1)) + efBVC + sBVC + sBVC + sBVC];
        triangles = new int[((chunkResolution - 2) * (chunkResolution - 2) * 6) + efBTC + sBTC + sBTC + sBTC];
        triangleOffset = 0;
        borderOffset = 0;

        GenerateMiddle();

        GenerateBot();
        GenerateLeft();
        GenerateRight();
        GenerateTopEdgeFan();

        templateVertices[1] = vertices;
        templateTriangles[1] = triangles;
    }

    private void GenerateChunkTemplate_BotEdgeFan()
    {
        vertices = new Vector3[((chunkResolution - 1) * (chunkResolution - 1)) + efBVC + sBVC + sBVC + sBVC];
        triangles = new int[((chunkResolution - 2) * (chunkResolution - 2) * 6) + efBTC + sBTC + sBTC + sBTC];
        triangleOffset = 0;
        borderOffset = 0;

        GenerateMiddle();

        GenerateTop();
        GenerateLeft();
        GenerateRight();
        GenerateBotEdgeFan();

        templateVertices[2] = vertices;
        templateTriangles[2] = triangles;
    }

    private void GenerateChunkTemplate_LeftEdgeFan()
    {
        vertices = new Vector3[((chunkResolution - 1) * (chunkResolution - 1)) + efBVC + sBVC + sBVC + sBVC];
        triangles = new int[((chunkResolution - 2) * (chunkResolution - 2) * 6) + efBTC + sBTC + sBTC + sBTC];
        triangleOffset = 0;
        borderOffset = 0;

        GenerateMiddle();
        
        GenerateTop();
        GenerateBot();
        GenerateRight();
        GenerateLeftEdgeFan();

        templateVertices[3] = vertices;
        templateTriangles[3] = triangles;
    }

    private void GenerateChunkTemplate_RightEdgeFan()
    {
        vertices = new Vector3[((chunkResolution - 1) * (chunkResolution - 1)) + efBVC + sBVC + sBVC + sBVC];
        triangles = new int[((chunkResolution - 2) * (chunkResolution - 2) * 6) + efBTC + sBTC + sBTC + sBTC];
        triangleOffset = 0;
        borderOffset = 0;

        GenerateMiddle();

        GenerateTop();
        GenerateBot();
        GenerateLeft();
        GenerateRightEdgeFan();

        templateVertices[4] = vertices;
        templateTriangles[4] = triangles;
    }

    private void GenerateChunkTemplate_TopLeftEdgeFan()
    {
        vertices = new Vector3[((chunkResolution - 1) * (chunkResolution - 1)) + efBVC + efBVC + sBVC + sBVC];
        triangles = new int[((chunkResolution - 2) * (chunkResolution - 2) * 6) + efBTC + efBTC + sBTC + sBTC];
        triangleOffset = 0;
        borderOffset = 0;

        GenerateMiddle();

        GenerateBot();
        GenerateRight();
        GenerateTopEdgeFan();
        GenerateLeftEdgeFan();

        templateVertices[5] = vertices;
        templateTriangles[5] = triangles;
    }

    private void GenerateChunkTemplate_TopRightEdgeFan()
    {
        vertices = new Vector3[((chunkResolution - 1) * (chunkResolution - 1)) + efBVC + efBVC + sBVC + sBVC];
        triangles = new int[((chunkResolution - 2) * (chunkResolution - 2) * 6) + efBTC + efBTC + sBTC + sBTC];
        triangleOffset = 0;
        borderOffset = 0;

        GenerateMiddle();

        //GenerateBot();
        GenerateLeft();
        //GenerateTopEdgeFan();
        //GenerateRightEdgeFan();

        templateVertices[6] = vertices;
        templateTriangles[6] = triangles;
    }

    private void GenerateChunkTemplate_BotLeftEdgeFan()
    {
        vertices = new Vector3[((chunkResolution - 1) * (chunkResolution - 1)) + efBVC + efBVC + sBVC + sBVC];
        triangles = new int[((chunkResolution - 2) * (chunkResolution - 2) * 6) + efBTC + efBTC + sBTC + sBTC];
        triangleOffset = 0;
        borderOffset = 0;

        GenerateMiddle();

        GenerateTop();
        GenerateRight();
        GenerateBotEdgeFan();
        GenerateLeftEdgeFan();

        templateVertices[7] = vertices;
        templateTriangles[7] = triangles;
    }

    private void GenerateChunkTemplate_BotRightEdgeFan()
    {
        vertices = new Vector3[((chunkResolution - 1) * (chunkResolution - 1)) + efBVC + efBVC + sBVC + sBVC];
        triangles = new int[((chunkResolution - 2) * (chunkResolution - 2) * 6) + efBTC + efBTC + sBTC + sBTC];
        triangleOffset = 0;
        borderOffset = 0;

        GenerateMiddle();

        GenerateTop();
        GenerateLeft();
        GenerateBotEdgeFan();
        GenerateRightEdgeFan();

        templateVertices[8] = vertices;
        templateTriangles[8] = triangles;
    }

    public void GenerateMiddle()
    {

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

    public void GenerateTop()
    {
        for (int y = 0; y < chunkResolution + 1; y++)
        {
            int i = (chunkResolution - 1) * (chunkResolution - 1) + y + borderOffset;
            Vector3 position = new Vector3(chunkResolution - chunkResolution * .5f, y  - chunkResolution * .5f, 0) / chunkResolution * 2;
            vertices[i] = position;

            if (y > 0 && y < chunkResolution - 1)
            {
                triangles[triangleOffset] = topVertices[y - 1];
                triangles[triangleOffset + 1] = i;
                triangles[triangleOffset + 2] = i + 1;

                triangles[triangleOffset + 3] = topVertices[y - 1];
                triangles[triangleOffset + 4] = i + 1;
                triangles[triangleOffset + 5] = topVertices[y];

                triangleOffset += 6;
            }
            if (y == 0)
            {
                triangles[triangleOffset] = topVertices[0];
                triangles[triangleOffset + 1] = i;
                triangles[triangleOffset + 2] = i + 1;

                triangleOffset += 3;
            }
            if (y == chunkResolution - 1)
            {
                triangles[triangleOffset] = topVertices[^1];
                triangles[triangleOffset + 1] = i;
                triangles[triangleOffset + 2] = i + 1;

                triangleOffset += 3;
            }
        }
        borderOffset += chunkResolution + 1;
    }

    public void GenerateBot()
    {
        for (int y = 0; y < chunkResolution + 1; y++)
        {
            int i = (chunkResolution - 1) * (chunkResolution - 1) + y + borderOffset;
            Vector3 position = new Vector3(0 - chunkResolution * .5f, y - chunkResolution * .5f, 0) / chunkResolution * 2;
            vertices[i] = position;

            if (y > 0 && y < chunkResolution - 1)
            {
                triangles[triangleOffset] = i;
                triangles[triangleOffset + 1] = botVertices[y - 1];
                triangles[triangleOffset + 2] = botVertices[y];

                triangles[triangleOffset + 3] = i;
                triangles[triangleOffset + 4] = botVertices[y];
                triangles[triangleOffset + 5] = i + 1;

                triangleOffset += 6;
            }
            if (y == 0)
            {
                triangles[triangleOffset] = i;
                triangles[triangleOffset + 1] = botVertices[0];
                triangles[triangleOffset + 2] = i + 1;

                triangleOffset += 3;
            }
            if (y == chunkResolution - 1)
            {
                triangles[triangleOffset] = i;
                triangles[triangleOffset + 1] = botVertices[^1];
                triangles[triangleOffset + 2] = i + 1;

                triangleOffset += 3;
            }
        }
        borderOffset += chunkResolution + 1;
    }

    public void GenerateLeft()
    {
        for (int y = 0; y < chunkResolution + 1; y++)
        {
            int i = (chunkResolution - 1) * (chunkResolution - 1) + y + borderOffset;
            Vector3 position = new Vector3(y - chunkResolution * .5f, 0 - chunkResolution * .5f, 0) / chunkResolution * 2;
            vertices[i] = position;

            if (y > 0 && y < chunkResolution - 1)
            {
                triangles[triangleOffset] = i;
                triangles[triangleOffset + 1] = leftVertices[y];
                triangles[triangleOffset + 2] = leftVertices[y - 1];

                triangles[triangleOffset + 3] = i;
                triangles[triangleOffset + 4] = i + 1;
                triangles[triangleOffset + 5] = leftVertices[y];

                triangleOffset += 6;
            }
            if (y == 0)
            {
                triangles[triangleOffset] = leftVertices[0];
                triangles[triangleOffset + 1] = i;
                triangles[triangleOffset + 2] = i + 1;

                triangleOffset += 3;
            }
            if (y == chunkResolution - 1)
            {
                triangles[triangleOffset] = leftVertices[^1];
                triangles[triangleOffset + 1] = i;
                triangles[triangleOffset + 2] = i + 1;

                triangleOffset += 3;
            }
        }
        borderOffset += chunkResolution + 1;
    }

    public void GenerateRight()
    {
        for (int y = 0; y < chunkResolution + 1; y++)
        {
            int i = (chunkResolution - 1) * (chunkResolution - 1) + y + borderOffset;
            Vector3 position = new Vector3(y - chunkResolution * .5f, chunkResolution - chunkResolution * .5f, 0) / chunkResolution * 2;
            vertices[i] = position;

            if (y > 0 && y < chunkResolution - 1)
            {
                triangles[triangleOffset] = i;
                triangles[triangleOffset + 1] = rightVertices[y - 1];
                triangles[triangleOffset + 2] = i + 1;

                triangles[triangleOffset + 3] = rightVertices[y - 1];
                triangles[triangleOffset + 4] = rightVertices[y];
                triangles[triangleOffset + 5] = i + 1;

                triangleOffset += 6;
            }
            if (y == 0)
            {
                triangles[triangleOffset] = i;
                triangles[triangleOffset + 1] = rightVertices[0];
                triangles[triangleOffset + 2] = i + 1;

                triangleOffset += 3;
            }
            if (y == chunkResolution - 1)
            {
                triangles[triangleOffset] = i;
                triangles[triangleOffset + 1] = rightVertices[^1];
                triangles[triangleOffset + 2] = i + 1;

                triangleOffset += 3;
            }
        }
        borderOffset += chunkResolution + 1;
    }

    public void GenerateTopEdgeFan()
    {
        for (int y = 0; y < chunkResolution / 2 + 1; y++)
        {
            int i = (chunkResolution - 1) * (chunkResolution - 1) + y + borderOffset;
            Vector3 position = new Vector3(chunkResolution - chunkResolution * .5f, (y * 2) - chunkResolution * .5f, 0) / chunkResolution * 2;
            vertices[i] = position;

            if (y != chunkResolution / 2)
            {
                triangles[triangleOffset] = i;
                triangles[triangleOffset + 1] = i + 1;
                triangles[triangleOffset + 2] = topVertices[y * 2];

                triangleOffset += 3;
            }

            if (y > 0 && y < chunkResolution / 2)
            {
                triangles[triangleOffset] = topVertices[y * 2 - 2];
                triangles[triangleOffset + 1] = i;
                triangles[triangleOffset + 2] = topVertices[y * 2 - 1];

                triangles[triangleOffset + 3] = topVertices[y * 2 - 1];
                triangles[triangleOffset + 4] = i;
                triangles[triangleOffset + 5] = topVertices[y * 2];

                triangleOffset += 6;
            }
        }
        borderOffset += (chunkResolution / 2 + 1);
    }

    public void GenerateBotEdgeFan()
    {
        for (int y = 0; y < chunkResolution / 2 + 1; y++)
        {
            int i = (chunkResolution - 1) * (chunkResolution - 1) + y + borderOffset;
            Vector3 position = new Vector3(0 - chunkResolution * .5f, (y * 2) - chunkResolution * .5f, 0) / chunkResolution * 2;
            vertices[i] = position;

            if (y != chunkResolution / 2)
            {
                triangles[triangleOffset] = i;
                triangles[triangleOffset + 1] = botVertices[y * 2];
                triangles[triangleOffset + 2] = i + 1;

                triangleOffset += 3;
            }

            if (y > 0 && y < chunkResolution / 2)
            {
                triangles[triangleOffset] = i;
                triangles[triangleOffset + 1] = botVertices[y * 2 - 2];
                triangles[triangleOffset + 2] = botVertices[y * 2 - 1];

                triangles[triangleOffset + 3] = i;
                triangles[triangleOffset + 4] = botVertices[y * 2 - 1];
                triangles[triangleOffset + 5] = botVertices[y * 2];

                triangleOffset += 6;
            }
        }
        borderOffset += (chunkResolution / 2 + 1);
    }

    public void GenerateLeftEdgeFan()
    {
        for (int y = 0; y < chunkResolution / 2 + 1; y++)
        {
            int i = (chunkResolution - 1) * (chunkResolution - 1) + y + borderOffset;
            Vector3 position = new Vector3((y * 2) - chunkResolution * .5f, chunkResolution - chunkResolution * .5f, 0) / chunkResolution * 2;
            vertices[i] = position;

            if (y != chunkResolution / 2)
            {
                triangles[triangleOffset] = i;
                triangles[triangleOffset + 1] = i + 1;
                triangles[triangleOffset + 2] = rightVertices[y * 2];

                triangleOffset += 3;
            }

            if (y > 0 && y < chunkResolution / 2)
            {
                triangles[triangleOffset] = rightVertices[y * 2 - 2];
                triangles[triangleOffset + 1] = i;
                triangles[triangleOffset + 2] = rightVertices[y * 2 - 1];

                triangles[triangleOffset + 3] = rightVertices[y * 2 - 1];
                triangles[triangleOffset + 4] = i;
                triangles[triangleOffset + 5] = rightVertices[y * 2];

                triangleOffset += 6;
            }
        }
        borderOffset += (chunkResolution / 2 + 1);
    }

    public void GenerateRightEdgeFan()
    {
        for (int y = 0; y < chunkResolution / 2 + 1; y++)
        {
            int i = (chunkResolution - 1) * (chunkResolution - 1) + y + borderOffset;
            Vector3 position = new Vector3((y * 2) - chunkResolution * .5f, chunkResolution - chunkResolution * .5f, 0) / chunkResolution * 2;
            vertices[i] = position;

            if (y != chunkResolution / 2)
            {
                triangles[triangleOffset] = i;
                triangles[triangleOffset + 1] = rightVertices[y * 2];
                triangles[triangleOffset + 2] = i + 1;

                triangleOffset += 3;
            }

            if (y > 0 && y < chunkResolution / 2)
            {
                triangles[triangleOffset] = i;
                triangles[triangleOffset + 1] = rightVertices[y * 2 - 2];
                triangles[triangleOffset + 2] = rightVertices[y * 2 - 1];

                triangles[triangleOffset + 3] = i;
                triangles[triangleOffset + 4] = rightVertices[y * 2 - 1];
                triangles[triangleOffset + 5] = rightVertices[y * 2];

                triangleOffset += 6;
            }
        }
        borderOffset += (chunkResolution / 2 + 1);
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