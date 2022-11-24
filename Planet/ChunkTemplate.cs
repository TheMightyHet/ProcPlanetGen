using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ChunkTemplate : MonoBehaviour
{
    public const int chunkResolution = 8;

    public Vector3[,] vertices;
    public static Vector3[,] templateVertices;

    public List<(int, int)> triangles;

    public List<(int, int)> topTriangles;
    public List<(int, int)> leftTriangles;
    public List<(int, int)> botTriangles;
    public List<(int, int)> rightTriangles;

    public List<(int, int)> topEdgeFanTriangles;
    public List<(int, int)> leftEdgeFanTriangles;
    public List<(int, int)> botEdgeFanTriangles;
    public List<(int, int)> rightEdgeFanTriangles;

    public static (int, int)[] templateMiddleTriangles;

    public static (int, int)[] templateTopTriangles;
    public static (int, int)[] templateLeftTriangles;
    public static (int, int)[] templateBotTriangles;
    public static (int, int)[] templateRightTriangles;

    public static (int, int)[] templateTopEdgeFanTriangles;
    public static (int, int)[] templateLeftEdgeFanTriangles;
    public static (int, int)[] templateBotEdgeFanTriangles;
    public static (int, int)[] templateRightEdgeFanTriangles;


    private void Start()
    {
        GenerateVerticesAndTriangles();
    }

    void GenerateVerticesAndTriangles()
    {
        vertices = new Vector3[(chunkResolution + 1), (chunkResolution + 1)];
        triangles = new();

        topTriangles = new();
        leftTriangles = new();
        botTriangles = new();
        rightTriangles = new();

        topEdgeFanTriangles = new();
        leftEdgeFanTriangles = new();
        botEdgeFanTriangles = new();
        rightEdgeFanTriangles = new();

        for (int y = 0; y < chunkResolution + 1; y++)
        {
            for (int x = 0; x < chunkResolution + 1; x++)
            {
                Vector3 position = new Vector3(x - chunkResolution * .5f, y - chunkResolution * .5f, 0) / chunkResolution * 2;
                vertices[y, x] = position;

                if (y == chunkResolution && x % 2 == 0 && x < chunkResolution)
                    AddTopAndBotBorderTriangles(y, x, -1, topEdgeFanTriangles, topTriangles, true);

                if (y == 0 && x % 2 == 0 && x < chunkResolution)
                    AddTopAndBotBorderTriangles(y, x, 1, botEdgeFanTriangles, botTriangles, true);

                if (x == chunkResolution && y % 2 == 0 && y < chunkResolution)
                    AddTopAndBotBorderTriangles(y, x, -1, rightEdgeFanTriangles, rightTriangles, false);

                if (x == 0 && y % 2 == 0 && y < chunkResolution)
                    AddTopAndBotBorderTriangles(y, x, 1, leftEdgeFanTriangles, leftTriangles, false);

                if (x > 0 && y > 0 && x < chunkResolution - 1 && y < chunkResolution - 1)
                    AddMiddleTriangles(y, x);
            }
        }

        templateVertices = vertices;
        templateMiddleTriangles = triangles.ToArray();

        topTriangles.Reverse();
        topEdgeFanTriangles.Reverse();

        templateBotTriangles = botTriangles.ToArray();
        templateBotEdgeFanTriangles = botEdgeFanTriangles.ToArray();

        templateTopTriangles = topTriangles.ToArray();
        templateTopEdgeFanTriangles = topEdgeFanTriangles.ToArray();

        leftTriangles.Reverse();
        leftEdgeFanTriangles.Reverse();

        templateRightTriangles = rightTriangles.ToArray();
        templateRightEdgeFanTriangles = rightEdgeFanTriangles.ToArray();

        templateLeftTriangles = leftTriangles.ToArray();
        templateLeftEdgeFanTriangles = leftEdgeFanTriangles.ToArray();
    }

    void AddMiddleTriangles(int y, int x)
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

    void AddTopAndBotBorderTriangles(int y, int x, int step, List<(int, int)> edgeFaneList, List<(int, int)> list, bool topOrBot)
    {
        if (topOrBot)
        {
            //EdgeFan
            edgeFaneList.Add((y, x));
            edgeFaneList.Add((y, x + 2));
            edgeFaneList.Add((y + step, x + 1));

            if (x > 0)
            {
                edgeFaneList.Add((y, x));
                edgeFaneList.Add((y + step, x + 1));
                edgeFaneList.Add((y + step, x));

                edgeFaneList.Add((y, x));
                edgeFaneList.Add((y + step, x));
                edgeFaneList.Add((y + step, x - 1));
            }

            //NoEdgeFan
            list.Add((y, x + 1));
            list.Add((y, x + 2));
            list.Add((y + step, x + 1));

            list.Add((y, x));
            list.Add((y, x + 1));
            list.Add((y + step, x + 1));

            if (x > 0)
            {
                list.Add((y, x));
                list.Add((y + step, x + 1));
                list.Add((y + step, x));

                list.Add((y, x));
                list.Add((y + step, x));
                list.Add((y + step, x - 1));
            }
        }
        else
        { 
            //EdgeFan
            edgeFaneList.Add((y, x));
            edgeFaneList.Add((y + 2, x));
            edgeFaneList.Add((y + 1, x + step));

            if (y > 0)
            {
                edgeFaneList.Add((y, x));
                edgeFaneList.Add((y + 1, x + step));
                edgeFaneList.Add((y, x + step));

                edgeFaneList.Add((y, x));
                edgeFaneList.Add((y, x + step));
                edgeFaneList.Add((y - 1, x + step));
            }

            //NoEdgeFan
            list.Add((y + 1, x));
            list.Add((y + 2, x));
            list.Add((y + 1, x + step));

            list.Add((y, x));
            list.Add((y + 1, x));
            list.Add((y + 1, x + step));

            if (y > 0)
            {
                list.Add((y, x));
                list.Add((y + 1, x + step));
                list.Add((y, x + step));

                list.Add((y, x));
                list.Add((y, x + step));
                list.Add((y - 1, x + step));
            }
        }
    }
}