using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ChunkTemplate : MonoBehaviour
{
    public const int chunkResolution = 4;

    public Vector3[,] vertices;
    public static Vector3[][,] templateVertices = new Vector3[6][,];

    public static (int, int)[] templateMiddleTriangles;

    public static (int, int)[] templateTopTriangles;
    public static (int, int)[] templateLeftTriangles;
    public static (int, int)[] templateBotTriangles;
    public static (int, int)[] templateRightTriangles;

    public static (int, int)[] templateTopEdgeFanTriangles;
    public static (int, int)[] templateLeftEdgeFanTriangles;
    public static (int, int)[] templateBotEdgeFanTriangles;
    public static (int, int)[] templateRightEdgeFanTriangles;


    private void Awake()
    {
        GenerateVerticesAndTriangles();
    }

    void GenerateVerticesAndTriangles()
    {
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.up, Vector3.down, Vector3.right, Vector3.left };

        List<(int, int)> triangles = new();

        List<(int, int)> topTriangles = new();
        List<(int, int)> leftTriangles = new();
        List<(int, int)> botTriangles = new();
        List<(int, int)> rightTriangles = new();

        List<(int, int)> topEdgeFanTriangles = new();
        List<(int, int)> leftEdgeFanTriangles = new();
        List<(int, int)> botEdgeFanTriangles = new();
        List<(int, int)> rightEdgeFanTriangles = new();

        bool trianglesSet = false;

        for (int i = 0; i < directions.Length; ++i)
        {
            Vector3 localUp = directions[i];
            Vector3 axisA = new Vector3(localUp.y, localUp.z, localUp.x);
            Vector3 axisB = Vector3.Cross(localUp, axisA);
            vertices = new Vector3[(chunkResolution + 1), (chunkResolution + 1)];
            for (int y = 0; y < chunkResolution + 1; y++)
            {
                for (int x = 0; x < chunkResolution + 1; x++)
                {
                    Vector3 position = (new Vector3(0, 0, 0) + (x - chunkResolution * .5f) * axisA + (y - chunkResolution * .5f) * axisB) / (chunkResolution / 2);
                    vertices[y, x] = position;

                    if (!trianglesSet)
                    {
                        if (y == chunkResolution && x % 2 == 0 && x < chunkResolution)
                            AddTopAndBotBorderTriangles(y, x, -1, rightEdgeFanTriangles, rightTriangles, true);

                        if (y == 0 && x % 2 == 0 && x < chunkResolution)
                            AddTopAndBotBorderTriangles(y, x, 1, leftEdgeFanTriangles, leftTriangles, true);

                        if (x == chunkResolution && y % 2 == 0 && y < chunkResolution)
                            AddTopAndBotBorderTriangles(y, x, -1, topEdgeFanTriangles, topTriangles, false);

                        if (x == 0 && y % 2 == 0 && y < chunkResolution)
                            AddTopAndBotBorderTriangles(y, x, 1, botEdgeFanTriangles, botTriangles, false);

                        if (x > 0 && y > 0 && x < chunkResolution - 1 && y < chunkResolution - 1)
                            AddMiddleTriangles(y, x, triangles);
                    }
                }
            }
            templateVertices[i] = vertices;
            trianglesSet = true;
        }
        templateMiddleTriangles = triangles.ToArray();

        botTriangles.Reverse();
        botEdgeFanTriangles.Reverse();

        templateBotTriangles = botTriangles.ToArray();
        templateBotEdgeFanTriangles = botEdgeFanTriangles.ToArray();

        templateTopTriangles = topTriangles.ToArray();
        templateTopEdgeFanTriangles = topEdgeFanTriangles.ToArray();

        rightTriangles.Reverse();
        rightEdgeFanTriangles.Reverse();

        templateRightTriangles = rightTriangles.ToArray();
        templateRightEdgeFanTriangles = rightEdgeFanTriangles.ToArray();

        templateLeftTriangles = leftTriangles.ToArray();
        templateLeftEdgeFanTriangles = leftEdgeFanTriangles.ToArray();
    }

    void AddMiddleTriangles(int y, int x, List<(int, int)> triangles)
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

    void AddTopAndBotBorderTriangles(int y, int x, int step, List<(int, int)> edgeFanBorderTriangles, List<(int, int)> borderTriangles, bool topOrBot)
    {
        if (topOrBot)
        {
            borderTriangles.Add((y, x + 1));
            borderTriangles.Add((y, x + 2));
            borderTriangles.Add((y + step, x + 1));

            borderTriangles.Add((y, x));
            borderTriangles.Add((y, x + 1));
            borderTriangles.Add((y + step, x + 1));

            edgeFanBorderTriangles.Add((y, x));
            edgeFanBorderTriangles.Add((y, x + 2));
            edgeFanBorderTriangles.Add((y + step, x + 1));

            if (x > 0)
            {
                borderTriangles.Add((y, x));
                borderTriangles.Add((y + step, x + 1));
                borderTriangles.Add((y + step, x));

                borderTriangles.Add((y, x));
                borderTriangles.Add((y + step, x));
                borderTriangles.Add((y + step, x - 1));

                edgeFanBorderTriangles.Add((y, x));
                edgeFanBorderTriangles.Add((y + step, x + 1));
                edgeFanBorderTriangles.Add((y + step, x));

                edgeFanBorderTriangles.Add((y, x));
                edgeFanBorderTriangles.Add((y + step, x));
                edgeFanBorderTriangles.Add((y + step, x - 1));
            }
        }
        else
        { 
            borderTriangles.Add((y + 1, x));
            borderTriangles.Add((y + 2, x));
            borderTriangles.Add((y + 1, x + step));

            borderTriangles.Add((y, x));
            borderTriangles.Add((y + 1, x));
            borderTriangles.Add((y + 1, x + step));

            edgeFanBorderTriangles.Add((y, x));
            edgeFanBorderTriangles.Add((y + 2, x));
            edgeFanBorderTriangles.Add((y + 1, x + step));

            if (y > 0)
            {
                borderTriangles.Add((y, x));
                borderTriangles.Add((y + 1, x + step));
                borderTriangles.Add((y, x + step));

                borderTriangles.Add((y, x));
                borderTriangles.Add((y, x + step));
                borderTriangles.Add((y - 1, x + step));

                edgeFanBorderTriangles.Add((y, x));
                edgeFanBorderTriangles.Add((y + 1, x + step));
                edgeFanBorderTriangles.Add((y, x + step));

                edgeFanBorderTriangles.Add((y, x));
                edgeFanBorderTriangles.Add((y, x + step));
                edgeFanBorderTriangles.Add((y - 1, x + step));
            }
        }
    }
}