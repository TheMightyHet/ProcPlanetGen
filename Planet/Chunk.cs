using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static PlanetFace;

public class Chunk
{
    public string path;
    public Planet planetScript;
    public PlanetFace planetFace;
    public NoiseFilter noiseFilter = new();
    public Chunk parentChunk;

    public int chunkBaseResolution = 8;

    public Vector3 chunkPosition;
    public Chunk[] subChunks;

    public float chunkRadius;
    public int chunkLODLevel;

    public Vector2 shiftOne;

    public Vector3[] vertices;
    public List<int> triangles = new();
    public Vector3[] normals;
    public Color[] colors;

    public Corners corner;
    public BorderPositions borderPosition;
    public BorderPositions topLeftBordPos = PlanetFace.BorderPositions.middle, topRightBordPos = PlanetFace.BorderPositions.middle, botLeftBordPos = PlanetFace.BorderPositions.middle, botRightBordPos = PlanetFace.BorderPositions.middle;
    public int[] neighbours;
    public Chunk(Chunk parentChunk, string path, Planet planetScript, PlanetFace planetFace, Chunk[] subChunks,
                Vector3 chunkPosition, float chunkRadius, int chunkLODLevel, PlanetFace.Corners corner, PlanetFace.BorderPositions borderPosition, int[] neighbours)
    {
        this.parentChunk = parentChunk;
        this.path = path;
        this.planetScript = planetScript;
        this.planetFace = planetFace;
        this.subChunks = subChunks;
        this.chunkPosition = chunkPosition;
        this.chunkRadius = chunkRadius;
        this.chunkLODLevel = chunkLODLevel;
        this.corner = corner;
        this.borderPosition = borderPosition;
        this.neighbours = neighbours;

        shiftOne = new Vector2(1, 1) / chunkBaseResolution;
    }

    public void GenerateSubChunks()
    {
        if (chunkLODLevel <= planetScript.distanceLOD.Length - 1 && chunkLODLevel >= 0)
        {
            float distToPlayerObj = Vector3.Distance(chunkPosition.normalized * planetScript.planetRadius + planetScript.transform.position, planetScript.playerObj.position);

            if (distToPlayerObj <= planetScript.distanceLOD[chunkLODLevel])
            {
                subChunks = new Chunk[4];

                Vector3 topLeft  = chunkPosition + (.5f * chunkRadius * planetFace.axisA) - (.5f * chunkRadius * planetFace.axisB);
                Vector3 topRight = chunkPosition + (.5f * chunkRadius * planetFace.axisA) + (.5f * chunkRadius * planetFace.axisB);
                Vector3 botLeft  = chunkPosition - (.5f * chunkRadius * planetFace.axisA) - (.5f * chunkRadius * planetFace.axisB);
                Vector3 botRight = chunkPosition - (.5f * chunkRadius * planetFace.axisA) + (.5f * chunkRadius * planetFace.axisB);

                subChunks[0] = new Chunk(this, path + "0", planetScript, planetFace, new Chunk[0], topLeft,  chunkRadius * .5f, chunkLODLevel + 1, Corners.topLeft,  topLeftBordPos,  new int[4]);
                subChunks[1] = new Chunk(this, path + "1", planetScript, planetFace, new Chunk[0], topRight, chunkRadius * .5f, chunkLODLevel + 1, Corners.topRight, topRightBordPos, new int[4]);
                subChunks[2] = new Chunk(this, path + "2", planetScript, planetFace, new Chunk[0], botLeft,  chunkRadius * .5f, chunkLODLevel + 1, Corners.botLeft,  botLeftBordPos,  new int[4]);
                subChunks[3] = new Chunk(this, path + "3", planetScript, planetFace, new Chunk[0], botRight, chunkRadius * .5f, chunkLODLevel + 1, Corners.botRight, botRightBordPos, new int[4]);

                foreach (Chunk chunk in subChunks)
                {
                    chunk.GenerateSubChunks();
                }
            }
        }
    }

    public void UpdateChunk()
    {
        if (chunkLODLevel <= planetScript.distanceLOD.Length - 1)
        {
            float distToPlayerObj = Vector3.Distance(chunkPosition.normalized * planetScript.planetRadius + planetScript.transform.position, planetScript.playerObj.position);

            if (distToPlayerObj > planetScript.distanceLOD[chunkLODLevel])
            {
                subChunks = new Chunk[0];
            }
            else
            {
                if (subChunks.Length > 0)
                {
                    foreach (Chunk chunk in subChunks)
                    {
                        chunk.UpdateChunk();
                    }
                }
                else
                {
                    GenerateSubChunks();
                }
            }
        }
    }

    internal (Vector3[], Vector3[], List<int>, Color[]) GetSubChunkData()
    {
        CheckNeighbourChunkLODsSmaller();

        int topVerticesCount = neighbours[0] == 1 ? chunkBaseResolution / 2 + 1 : chunkBaseResolution + 1;
        int leftVerticesCount = neighbours[1] == 1 ? chunkBaseResolution / 2 + 1 : chunkBaseResolution + 1;
        int botVerticesCount = neighbours[2] == 1 ? chunkBaseResolution / 2 + 1 : chunkBaseResolution + 1;
        int righttVerticesCount = neighbours[3] == 1 ? chunkBaseResolution / 2 + 1 : chunkBaseResolution + 1;

        vertices = new Vector3[(chunkBaseResolution - 1) * (chunkBaseResolution - 1) + topVerticesCount + botVerticesCount + leftVerticesCount + righttVerticesCount];
        triangles.Clear();
        normals = new Vector3[vertices.Length];
        colors = new Color[vertices.Length];

        CalculateChunkMiddle();
        if (neighbours[0] == 1) CalculateChunkBorderEdgeFan(chunkBaseResolution, 0); else CalculateChunkBorder(chunkBaseResolution, 0);
        if (neighbours[1] == 1) CalculateChunkBorderEdgeFan(0, 1); else CalculateChunkBorder(0, 1);
        if (neighbours[2] == 1) CalculateChunkBorderEdgeFan(0, 2); else CalculateChunkBorder(0, 2);
        if (neighbours[3] == 1) CalculateChunkBorderEdgeFan(chunkBaseResolution, 3); else CalculateChunkBorder(chunkBaseResolution, 3);


        for (int i = 0; i < vertices.Length; ++i)
            colors[i] = Color.Lerp(Color.red, Color.green, UnityEngine.Random.value);//noiseFilter.Evaluate(vertices[i].normalized * planetScript.planetRadius));

        return (vertices, normals, triangles, colors); //GetTriangles());
    }

    public void CalculateChunkMiddle()
    {
        for (int y = 0; y < chunkBaseResolution - 1; y++)
        {
            for (int x = 0; x < chunkBaseResolution - 1; x++)
            {
                Vector2 percent = new Vector2(x + 1, y + 1) / chunkBaseResolution;
                Vector3 pointPosOnCube = chunkPosition + ((percent.x - .5f) * 2 * planetFace.axisA + (percent.y - .5f) * 2 * planetFace.axisB) * chunkRadius;
                Vector3 pointPosOnSphere = pointPosOnCube.normalized;

                CheckVertexInHashTable(pointPosOnSphere);

                if (x != chunkBaseResolution - 2 && y != chunkBaseResolution - 2)
                {
                    Vector3 vertA = (pointPosOnCube + ((shiftOne.x * 2 * planetFace.axisA) + (0 * 2 * planetFace.axisB)) * chunkRadius).normalized;
                    Vector3 vertB = (pointPosOnCube + ((shiftOne.x * 2 * planetFace.axisA) + (shiftOne.y * 2 * planetFace.axisB)) * chunkRadius).normalized;
                    Vector3 vertC = (pointPosOnCube + ((0 * 2 * planetFace.axisA) + (shiftOne.y * 2 * planetFace.axisB)) * chunkRadius).normalized;

                    CheckVertexInHashTable(vertA);
                    CheckVertexInHashTable(vertB);
                    CheckVertexInHashTable(vertC);

                    if ((y + x) % 2 == 0)
                    {
                        AddTriangle(pointPosOnSphere, vertA, vertC);
                        AddTriangle(vertA, vertB, vertC);
                    }
                    else
                    {
                        AddTriangle(pointPosOnSphere, vertA, vertB);
                        AddTriangle(pointPosOnSphere, vertB, vertC);
                    }
                }
            }
        }
    }

    public void CalculateChunkBorderEdgeFan(int border, int sideWays)
    {
        // BorderWithEdgeFan
        for (int y = 0; y < chunkBaseResolution / 2 + 1; y++)
        {
            Vector2 percent = sideWays % 2 == 0 ? new Vector2(border, y * 2) / chunkBaseResolution : new Vector2(y * 2, border) / chunkBaseResolution;
            Vector3 pointPosOnCube = chunkPosition + ((percent.x - .5f) * 2 * planetFace.axisA + (percent.y - .5f) * 2 * planetFace.axisB) * chunkRadius;

            CheckVertexInHashTable(pointPosOnCube.normalized);

            if (sideWays == 0) DrawEdgeFanTop(pointPosOnCube, y);
            if (sideWays == 1) DrawEdgeFanLeft(pointPosOnCube, y);
            if (sideWays == 2) DrawEdgeFanBot(pointPosOnCube, y);
            if (sideWays == 3) DrawEdgeFanRight(pointPosOnCube, y);
        }
    }

    void DrawEdgeFanTop(Vector3 cubePos, int vertY)
    {
        Vector3 vertA = (cubePos + (shiftOne.y * 4 * planetFace.axisB * chunkRadius)).normalized; // *4, so it 'skips' one
        Vector3 vertB = (cubePos - ((shiftOne.x * 2 * planetFace.axisA) - (shiftOne.y * 2 * planetFace.axisB)) * chunkRadius).normalized;

        CheckVertexInHashTable(vertA);
        CheckVertexInHashTable(vertB);

        if (vertY != chunkBaseResolution / 2)
        {
            AddTriangle(cubePos.normalized, vertA, vertB);
        }
        if (vertY > 0 && vertY < chunkBaseResolution / 2)
        {
            Vector3 vertC = (cubePos - (shiftOne.x * 2 * planetFace.axisA) * chunkRadius).normalized;
            Vector3 vertD = (cubePos - ((shiftOne.x * 2 * planetFace.axisA) + (shiftOne.y * 2 * planetFace.axisB)) * chunkRadius).normalized;

            CheckVertexInHashTable(vertC);
            CheckVertexInHashTable(vertD);

            AddTriangle(cubePos.normalized, vertB, vertC);
            AddTriangle(cubePos.normalized, vertC, vertD);
        }
    }

    void DrawEdgeFanLeft(Vector3 cubePos, int vertY)
    {
        Vector3 vertA = (cubePos + (shiftOne.x * 4 * planetFace.axisA) * chunkRadius).normalized; // *4, so it 'skips' one
        Vector3 vertB = (cubePos + ((shiftOne.x * 2 * planetFace.axisA) + (shiftOne.y * 2 * planetFace.axisB)) * chunkRadius).normalized;

        CheckVertexInHashTable(vertA);
        CheckVertexInHashTable(vertB);

        if (vertY != chunkBaseResolution / 2)
        {
            AddTriangle(cubePos.normalized, vertA, vertB);
        }
        if (vertY > 0 && vertY < chunkBaseResolution / 2)
        {
            Vector3 vertC = (cubePos + (shiftOne.y * 2 * planetFace.axisB) * chunkRadius).normalized;
            Vector3 vertD = (cubePos + (-(shiftOne.x * 2 * planetFace.axisA) + (shiftOne.y * 2 * planetFace.axisB)) * chunkRadius).normalized;

            CheckVertexInHashTable(vertC);
            CheckVertexInHashTable(vertD);

            AddTriangle(cubePos.normalized, vertB, vertC);
            AddTriangle(cubePos.normalized, vertC, vertD);
        }
    }

    public void DrawEdgeFanBot(Vector3 cubePos, int vertY)
    {
        Vector3 vertA = (cubePos + (shiftOne.y * 4 * planetFace.axisB) * chunkRadius).normalized; // *4, so it 'skips' one
        Vector3 vertB = (cubePos + ((shiftOne.x * 2 * planetFace.axisA) + (shiftOne.y * 2 * planetFace.axisB)) * chunkRadius).normalized;

        CheckVertexInHashTable(vertA);
        CheckVertexInHashTable(vertB);

        if (vertY != chunkBaseResolution / 2)
        {
            AddTriangle(cubePos.normalized, vertB, vertA);
        }
        if (vertY > 0 && vertY < chunkBaseResolution / 2)
        {
            Vector3 vertC = (cubePos + (shiftOne.x * 2 * planetFace.axisA) * chunkRadius).normalized;
            Vector3 vertD = (cubePos + ((shiftOne.x * 2 * planetFace.axisA) - (shiftOne.y * 2 * planetFace.axisB)) * chunkRadius).normalized;

            CheckVertexInHashTable(vertC);
            CheckVertexInHashTable(vertD);

            AddTriangle(cubePos.normalized, vertC, vertB);
            AddTriangle(cubePos.normalized, vertD, vertC);
        }
    }

    void DrawEdgeFanRight(Vector3 cubePos, int vertY)
    {
        Vector3 vertA = (cubePos + (shiftOne.x * 4 * planetFace.axisA) * chunkRadius).normalized; // *4, so it 'skips' one
        Vector3 vertB = (cubePos + ((shiftOne.x * 2 * planetFace.axisA) - (shiftOne.y * 2 * planetFace.axisB)) * chunkRadius).normalized;

        CheckVertexInHashTable(vertA);
        CheckVertexInHashTable(vertB);

        if (vertY != chunkBaseResolution / 2)
        {
            AddTriangle(cubePos.normalized, vertB, vertA);
        }
        if (vertY > 0 && vertY < chunkBaseResolution / 2)
        {
            Vector3 vertC = (cubePos - (shiftOne.y * 2 * planetFace.axisB) * chunkRadius).normalized;
            Vector3 vertD = (cubePos - ((shiftOne.x * 2 * planetFace.axisA) + (shiftOne.y * 2 * planetFace.axisB)) * chunkRadius).normalized;

            CheckVertexInHashTable(vertC);
            CheckVertexInHashTable(vertD);

            AddTriangle(cubePos.normalized, vertC, vertB);
            AddTriangle(cubePos.normalized, vertD, vertC);
        }
    }

    public void CalculateChunkBorder(int border, int sideWays)
    { 
        // BorderWithoutEdgeFans
        for (int y = 0; y < chunkBaseResolution + 1; y++)
        {
            Vector2 percent = sideWays % 2 == 0 ? new Vector2(border, y) / chunkBaseResolution : new Vector2(y, border) / chunkBaseResolution;
            Vector3 pointPosOnCube = chunkPosition + ((percent.x - .5f) * 2 * planetFace.axisA + (percent.y - .5f) * 2 * planetFace.axisB) * chunkRadius;

            CheckVertexInHashTable(pointPosOnCube.normalized);

            if (sideWays == 0) DrawSimpleBorderTop(pointPosOnCube, y);
            if (sideWays == 1) DrawSimpleBorderLeft(pointPosOnCube, y);
            if (sideWays == 2) DrawSimpleBorderBot(pointPosOnCube, y);
            if (sideWays == 3) DrawSimpleBorderRight(pointPosOnCube, y);
        }
    }

    public void DrawSimpleBorderTop(Vector3 cubePos, int vertY)
    {
        Vector3 vertA = (cubePos + (shiftOne.y * 2 * planetFace.axisB) * chunkRadius).normalized; 

        CheckVertexInHashTable(vertA);

        if (vertY == 0)
        {
            Vector3 vertB = (cubePos - ((shiftOne.x * 2 * planetFace.axisA) - (shiftOne.y * 2 * planetFace.axisB)) * chunkRadius).normalized;

            CheckVertexInHashTable(vertB);

            AddTriangle(cubePos.normalized, vertA, vertB);
        }
        if (vertY == chunkBaseResolution - 1)
        {
            Vector3 vertB = (cubePos - (shiftOne.x * 2 * planetFace.axisA) * chunkRadius).normalized;

            CheckVertexInHashTable(vertB);

            AddTriangle(cubePos.normalized, vertA, vertB);
        }
        if (vertY > 0 && vertY < chunkBaseResolution - 1)
        {
            Vector3 vertB = (cubePos - (shiftOne.x * 2 * planetFace.axisA) * chunkRadius).normalized;
            Vector3 vertC = (cubePos - ((shiftOne.x * 2 * planetFace.axisA) - (shiftOne.y * 2 * planetFace.axisB)) * chunkRadius).normalized;

            CheckVertexInHashTable(vertB);
            CheckVertexInHashTable(vertC);

            if (vertY % 2 == 1)
            {
                AddTriangle(cubePos.normalized, vertA, vertB);
                AddTriangle(vertA, vertC, vertB);
            }
            else
            {
                AddTriangle(cubePos.normalized, vertC, vertB);
                AddTriangle(cubePos.normalized, vertA, vertC);
            }
        }
    }

    public void DrawSimpleBorderLeft(Vector3 cubePos, int vertY)
    {
        Vector3 vertA = (cubePos + (shiftOne.y * 2 * planetFace.axisA) * chunkRadius).normalized;

        CheckVertexInHashTable(vertA);

        if (vertY == 0)
        {
            Vector3 vertB = (cubePos + ((shiftOne.x * 2 * planetFace.axisA) + (shiftOne.y * 2 * planetFace.axisB)) * chunkRadius).normalized;

            CheckVertexInHashTable(vertB);

            AddTriangle(cubePos.normalized, vertA, vertB);
        }
        if (vertY == chunkBaseResolution - 1)
        {
            Vector3 vertB = (cubePos + (shiftOne.y * 2 * planetFace.axisB) * chunkRadius).normalized;

            CheckVertexInHashTable(vertB);

            AddTriangle(cubePos.normalized, vertA, vertB);
        }
        if (vertY > 0 && vertY < chunkBaseResolution - 1)
        {
            Vector3 vertB = (cubePos + (shiftOne.y * 2 * planetFace.axisB) * chunkRadius).normalized;
            Vector3 vertC = (cubePos + ((shiftOne.x * 2 * planetFace.axisA) + (shiftOne.y * 2 * planetFace.axisB)) * chunkRadius).normalized;

            CheckVertexInHashTable(vertB);
            CheckVertexInHashTable(vertC);

            if (vertY % 2 == 1)
            {
                AddTriangle(cubePos.normalized, vertA, vertB);
                AddTriangle(vertA, vertC, vertB);
            }
            else
            {
                AddTriangle(cubePos.normalized, vertC, vertB);
                AddTriangle(cubePos.normalized, vertA, vertC);
            }
        }
    }

    public void DrawSimpleBorderBot(Vector3 cubePos, int vertY)
    {
        Vector3 vertA = (cubePos + (shiftOne.y * 2 * planetFace.axisB) * chunkRadius).normalized;

        CheckVertexInHashTable(vertA);

        if (vertY == 0)
        {
            Vector3 vertB = (cubePos + ((shiftOne.x * 2 * planetFace.axisA) + (shiftOne.y * 2 * planetFace.axisB)) * chunkRadius).normalized;

            CheckVertexInHashTable(vertB);

            AddTriangle(cubePos.normalized, vertB, vertA);
        }
        if (vertY == chunkBaseResolution - 1)
        {
            Vector3 vertB = (cubePos + (shiftOne.x * 2 * planetFace.axisA) * chunkRadius).normalized;

            CheckVertexInHashTable(vertB);

            AddTriangle(cubePos.normalized, vertB, vertA);
        }
        if (vertY > 0 && vertY < chunkBaseResolution - 1)
        {
            Vector3 vertB = (cubePos + (shiftOne.x * 2 * planetFace.axisA) * chunkRadius).normalized;
            Vector3 vertC = (cubePos + ((shiftOne.x * 2 * planetFace.axisA) + (shiftOne.y * 2 * planetFace.axisB)) * chunkRadius).normalized;

            CheckVertexInHashTable(vertB);
            CheckVertexInHashTable(vertC);

            if (vertY % 2 == 1)
            {
                AddTriangle(cubePos.normalized, vertB, vertA);
                AddTriangle(vertA, vertB, vertC);
            }
            else
            {
                AddTriangle(cubePos.normalized, vertB, vertC);
                AddTriangle(cubePos.normalized, vertC, vertA);
            }
        }
    }

    public void DrawSimpleBorderRight(Vector3 cubePos, int vertY)
    {
        Vector3 vertA = (cubePos + (shiftOne.y * 2 * planetFace.axisA) * chunkRadius).normalized;

        CheckVertexInHashTable(vertA);

        if (vertY == 0)
        {
            Vector3 vertB = (cubePos + ((shiftOne.x * 2 * planetFace.axisA) - (shiftOne.y * 2 * planetFace.axisB)) * chunkRadius).normalized;

            CheckVertexInHashTable(vertB);

            AddTriangle(cubePos.normalized, vertB, vertA);
        }
        if (vertY == chunkBaseResolution - 1)
        {
            Vector3 vertB = (cubePos - (shiftOne.y * 2 * planetFace.axisB) * chunkRadius).normalized;

            CheckVertexInHashTable(vertB);

            AddTriangle(cubePos.normalized, vertB, vertA);
        }
        if (vertY > 0 && vertY < chunkBaseResolution - 1)
        {
            Vector3 vertB = (cubePos - (shiftOne.y * 2 * planetFace.axisB) * chunkRadius).normalized;
            Vector3 vertC = (cubePos + ((shiftOne.x * 2 * planetFace.axisA) - (shiftOne.y * 2 * planetFace.axisB)) * chunkRadius).normalized;

            CheckVertexInHashTable(vertB);
            CheckVertexInHashTable(vertC);

            if (vertY % 2 == 1)
            {
                AddTriangle(cubePos.normalized, vertB, vertA);
                AddTriangle(vertA, vertB, vertC);
            }
            else
            {
                AddTriangle(cubePos.normalized, vertB, vertC);
                AddTriangle(cubePos.normalized, vertC, vertA);
            }
        }
    }

    void CheckVertexInHashTable(Vector3 vertex)
    {

        if (!planetFace.verticeSN.ContainsKey(vertex))
        {
            planetFace.faceVertices.Add(vertex);
            planetFace.verticeSN.Add(vertex, planetFace.hashCounter);
            ++planetFace.hashCounter;
        }
    }

    void AddTriangle(Vector3 vertA, Vector3 vertB, Vector3 vertC)
    {
        triangles.Add((int)planetFace.verticeSN[vertA]);
        triangles.Add((int)planetFace.verticeSN[vertB]);
        triangles.Add((int)planetFace.verticeSN[vertC]);
    }

    public Vector3 GetSurfaceNormal(int a, int b, int c)
    {
        Vector3 pA = vertices[a];
        Vector3 pB = vertices[b];
        Vector3 pC = vertices[c];
        
        Vector3 sideAB = pB - pA;
        Vector3 sideAC = pC - pB;

        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    public void GetSubChunks()
    {
        if (subChunks.Length > 0)
        {
            foreach (Chunk subChunk in subChunks)
            {
                subChunk.GetSubChunks();
            }
        }
        else
        {
            if (Vector3.Angle(chunkPosition, planetScript.playerObj.position) < 70)
                planetFace.displayedChunk.Add(this);
        }
    }

    public void CheckNeighbourChunkLODsSmaller()
    {
        //Top, Left, Bot, Right
        switch (path[^1]) // (corner)
        {
            case '0': // Corners.topLeft:
                {
                    neighbours[0] = CheckIfNeighbourLODSmallerPathV(0); // Top  //borderPosition != BorderPositions.topLeftCorner && borderPosition != BorderPositions.topRightCorner && borderPosition != BorderPositions.topMidBorder ? CheckIfNeighbourLODSmaller(0) : 0; // <- 0 if on faceBorder
                    neighbours[1] = CheckIfNeighbourLODSmallerPathV(1); // Left //borderPosition != BorderPositions.topLeftCorner && borderPosition != BorderPositions.botLeftCorner && borderPosition != BorderPositions.leftMidBorder ? CheckIfNeighbourLODSmaller(1) : 0; // <- 0 if on faceBorder
                    neighbours[2] = 0; // Bot
                    neighbours[3] = 0; // Right
                    break;
                }
            case '1': // Corners.topRight:
                {
                    neighbours[0] = CheckIfNeighbourLODSmallerPathV(0); // Top //borderPosition != BorderPositions.topLeftCorner && borderPosition != BorderPositions.topRightCorner && borderPosition != BorderPositions.topMidBorder ? CheckIfNeighbourLODSmaller(0) : 0; // <- 0 if on faceBorder
                    neighbours[1] = 0; // Left
                    neighbours[2] = 0; // Bot
                    neighbours[3] = CheckIfNeighbourLODSmallerPathV(3); // Right //borderPosition != BorderPositions.topRightCorner && borderPosition != BorderPositions.botRightCorner && borderPosition != BorderPositions.rightMidBorder ? CheckIfNeighbourLODSmaller(3) : 0; // <- 0 if on faceBorder
                    break;
                }
            case '2': // Corners.botLeft:
                {
                    neighbours[0] = 0; // Top
                    neighbours[1] = CheckIfNeighbourLODSmallerPathV(1); // Left //borderPosition != BorderPositions.topLeftCorner && borderPosition != BorderPositions.botLeftCorner && borderPosition != BorderPositions.leftMidBorder ? CheckIfNeighbourLODSmaller(1) : 0; // <- 0 if on faceBorder
                    neighbours[2] = CheckIfNeighbourLODSmallerPathV(2); // Bot  //borderPosition != BorderPositions.botLeftCorner && borderPosition != BorderPositions.botRightCorner && borderPosition != BorderPositions.botMidBorder ? CheckIfNeighbourLODSmaller(2) : 0; // <- 0 if faceBorder
                    neighbours[3] = 0; // Right
                    break;
                }
            case '3': // Corners.botRight:
                {
                    neighbours[0] = 0; // Top
                    neighbours[1] = 0; // Left
                    neighbours[2] = CheckIfNeighbourLODSmallerPathV(2); // Bot //borderPosition != BorderPositions.botLeftCorner && borderPosition != BorderPositions.botRightCorner && borderPosition != BorderPositions.botMidBorder ? CheckIfNeighbourLODSmaller(2) : 0; // <- 0 if faceBorder
                    neighbours[3] = CheckIfNeighbourLODSmallerPathV(3); // Right //borderPosition != BorderPositions.topRightCorner && borderPosition != BorderPositions.botRightCorner && borderPosition != BorderPositions.rightMidBorder ? CheckIfNeighbourLODSmaller(3) : 0; // <- 0 if on faceBorder

                    break;
                }
            default: break;
        }
    }

    public int CheckIfNeighbourLODSmallerPathV(int dir)
    {
        string neighbourPath = InvertedPathPathV(dir);
        if (neighbourPath == "") return 0;
        Chunk neighbour = planetFace.baseChunk;

        while(neighbourPath.Length > 0)
        {
            if (neighbour.subChunks.Length > 0)
            {
                if (neighbourPath[0] == '0') neighbour = neighbour.subChunks[0];
                else if (neighbourPath[0] == '1') neighbour = neighbour.subChunks[1];
                else if (neighbourPath[0] == '2') neighbour = neighbour.subChunks[2];
                else if (neighbourPath[0] == '3') neighbour = neighbour.subChunks[3];
                neighbourPath = neighbourPath.Remove(0, 1);
            }
            else
                break;
        }

        if (neighbour.chunkLODLevel < chunkLODLevel) return 1;
        return 0;
    }

    public string InvertedPathPathV(int dir)
    {
        string chunkPath = path;
        chunkPath = chunkPath.Remove(0, 1); // Remove baseChunk
        string neighbourPath = "";

        while (chunkPath.Length > 0)
        {
            if (!HasSiblingTowardsPathV(dir, chunkPath[^1]))
            {
                neighbourPath = neighbourPath.Insert(0, InvertDirectionPathV(dir, chunkPath[^1]));
                chunkPath = chunkPath.Remove(chunkPath.Length - 1);
                if (chunkPath.Length == 0) return "";
            }
            else if (HasSiblingTowardsPathV(dir, chunkPath[^1]))
            {
                neighbourPath = neighbourPath.Insert(0, InvertDirectionPathV(dir, chunkPath[^1]));
                chunkPath = chunkPath.Remove(chunkPath.Length - 1);
                break;
            }
        }
        neighbourPath = neighbourPath.Insert(0, chunkPath);
        return neighbourPath;
    }

    public string InvertDirectionPathV(int dir, char pathPart)
    {
        if ((dir % 2) == 1)
        {
            if (pathPart == '0') return "1"; // topLeft  -> topRight
            else
            if (pathPart == '1') return "0"; // topRight -> topLeft
            else
            if (pathPart == '2') return "3"; // botLeft  -> botRight
            else
            if (pathPart == '3') return "2"; // botRight -> botLeft
        }
        else
        if ((dir % 2) == 0)
        {
            if (pathPart == '0') return "2"; // topLeft -> botLeft
            else
            if (pathPart == '1') return "3"; // topRight -> botRight
            else
            if (pathPart == '2') return "0"; // botLeft -> topLeft
            else
            if (pathPart == '3') return "1"; // botRight -> topRight
        }
        return "";
    }

    public bool HasSiblingTowardsPathV(int dir, char pathPart)
    {
        if (dir == 0)
        {
            if (pathPart == '2' || pathPart == '3') // BotSide
                return true;
            else if (pathPart == '1' || pathPart == '4') // TopSide
                return false;
        }
        else if (dir == 1)
        {
            if (pathPart == '1' || pathPart == '3') // RightSide
                return true;
            else if (pathPart == '0' || pathPart == '2') // LeftSide
                return false;
        }
        else if (dir == 2)
        {
            if (pathPart == '0' || pathPart == '1') // TopSide
                return true;
            else if (pathPart == '2' || pathPart == '3') // BotSide
                return false;
        }
        else if (dir == 3)
        {
            if (pathPart == '0' || pathPart == '2') // LeftSide
                return true;
            else if (pathPart == '1' || pathPart == '3') // RightSide
                return false;
        }

        return false;
    }
}
