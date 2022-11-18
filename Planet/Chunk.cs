using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static PlanetFace;

public class Chunk
{
    public string path;
    public Planet planetScript;
    public PlanetFace planetFace;
    public NoiseFilter noiseFilter = new();

    public int chunkBaseResolution = 4;

    public Vector3 chunkPosition;
    public Chunk[] subChunks;

    public float chunkRadius;
    public int chunkLODLevel;

    public Vector2 shiftOne;
    public (Vector3, Vector3) step;

    public List<int> chunkTriangles = new();
    public List<int> middleTriangles = new();
    public List<int> topBorderTriangles = new();
    public List<int> leftBorderTriangles = new();
    public List<int> botBorderTriangles = new();
    public List<int> rightBorderTriangles = new();

    public int[] neighbours;
    public int[] neighboursPrev = new[]{-1, -1, -1, -1};

    public Chunk(string path, Planet planetScript, PlanetFace planetFace, Chunk[] subChunks, Vector3 chunkPosition, float chunkRadius, int chunkLODLevel, int[] neighbours)
    {
        this.path = path;
        this.planetScript = planetScript;
        this.planetFace = planetFace;
        this.subChunks = subChunks;
        this.chunkPosition = chunkPosition;
        this.chunkRadius = chunkRadius;
        this.chunkLODLevel = chunkLODLevel;
        this.neighbours = neighbours;

        step = ((2f / (float)chunkBaseResolution) * chunkRadius * planetFace.axisA, (2f / (float)chunkBaseResolution) * chunkRadius * planetFace.axisB);
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

                subChunks[0] = new Chunk(path + "0", planetScript, planetFace, new Chunk[0], topLeft,  chunkRadius * .5f, chunkLODLevel + 1, new int[4]);
                subChunks[1] = new Chunk(path + "1", planetScript, planetFace, new Chunk[0], topRight, chunkRadius * .5f, chunkLODLevel + 1, new int[4]);
                subChunks[2] = new Chunk(path + "2", planetScript, planetFace, new Chunk[0], botLeft,  chunkRadius * .5f, chunkLODLevel + 1, new int[4]);
                subChunks[3] = new Chunk(path + "3", planetScript, planetFace, new Chunk[0], botRight, chunkRadius * .5f, chunkLODLevel + 1, new int[4]);

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

    internal List<int> GetChunkData()
    {
        chunkTriangles.Clear();

        CheckNeighbourChunkLODsSmaller();

        if (!middleTriangles.Any()) CalculateChunkMiddle();

        if (neighbours[0] != neighboursPrev[0])
        {
            topBorderTriangles.Clear();
            if (neighbours[0] == 1) CalculateChunkBorderEdgeFan(chunkBaseResolution, 0); else CalculateChunkBorder(chunkBaseResolution, 0);
        }
        if (neighbours[1] != neighboursPrev[1])
        {
            leftBorderTriangles.Clear();
            if (neighbours[1] == 1) CalculateChunkBorderEdgeFan(0, 1); else CalculateChunkBorder(0, 1);
        }
        if (neighbours[2] != neighboursPrev[2])
        {
            botBorderTriangles.Clear();
            if (neighbours[2] == 1) CalculateChunkBorderEdgeFan(0, 2); else CalculateChunkBorder(0, 2);
        }
        if (neighbours[3] != neighboursPrev[3])
        {
            rightBorderTriangles.Clear();
            if (neighbours[3] == 1) CalculateChunkBorderEdgeFan(chunkBaseResolution, 3); else CalculateChunkBorder(chunkBaseResolution, 3);
        }

        neighboursPrev = neighbours;

        chunkTriangles.AddRange(middleTriangles);
        chunkTriangles.AddRange(topBorderTriangles);
        chunkTriangles.AddRange(leftBorderTriangles);
        chunkTriangles.AddRange(botBorderTriangles);
        chunkTriangles.AddRange(rightBorderTriangles);

        return chunkTriangles;
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
                    Vector3 vertA = (pointPosOnCube + step.Item1).normalized;
                    Vector3 vertB = (pointPosOnCube + step.Item1 + step.Item2).normalized;
                    Vector3 vertC = (pointPosOnCube + step.Item2).normalized;
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
        Vector3 vertA = (cubePos + step.Item2 * 2).normalized;
        Vector3 vertB = (cubePos - step.Item1 + step.Item2).normalized;
        CheckVertexInHashTable(vertA);
        CheckVertexInHashTable(vertB);
        if (vertY != chunkBaseResolution / 2)
        {
            AddTopBorderTriangle(cubePos.normalized, vertA, vertB);
        }
        if (vertY > 0 && vertY < chunkBaseResolution / 2)
        {
            Vector3 vertC = (cubePos - step.Item1).normalized;
            Vector3 vertD = (cubePos - step.Item1 - step.Item2).normalized;
            CheckVertexInHashTable(vertC);
            CheckVertexInHashTable(vertD);
            AddTopBorderTriangle(cubePos.normalized, vertB, vertC);
            AddTopBorderTriangle(cubePos.normalized, vertC, vertD);
        }
    }

    void DrawEdgeFanLeft(Vector3 cubePos, int vertY)
    {
        Vector3 vertA = (cubePos + step.Item1 * 2).normalized;
        Vector3 vertB = (cubePos + step.Item1 + step.Item2).normalized;
        CheckVertexInHashTable(vertA);
        CheckVertexInHashTable(vertB);
        if (vertY != chunkBaseResolution / 2)
        {
            AddLeftBorderTriangle(cubePos.normalized, vertA, vertB);
        }
        if (vertY > 0 && vertY < chunkBaseResolution / 2)
        {
            Vector3 vertC = (cubePos + step.Item2).normalized;
            Vector3 vertD = (cubePos - step.Item1 + step.Item2).normalized;
            CheckVertexInHashTable(vertC);
            CheckVertexInHashTable(vertD);
            AddLeftBorderTriangle(cubePos.normalized, vertB, vertC);
            AddLeftBorderTriangle(cubePos.normalized, vertC, vertD);
        }
    }

    public void DrawEdgeFanBot(Vector3 cubePos, int vertY)
    {
        Vector3 vertA = (cubePos + step.Item2 * 2).normalized;
        Vector3 vertB = (cubePos + step.Item1 + step.Item2).normalized;
        CheckVertexInHashTable(vertA);
        CheckVertexInHashTable(vertB);
        if (vertY != chunkBaseResolution / 2)
        {
            AddBotBorderTriangle(cubePos.normalized, vertB, vertA);
        }
        if (vertY > 0 && vertY < chunkBaseResolution / 2)
        {
            Vector3 vertC = (cubePos + step.Item1).normalized;
            Vector3 vertD = (cubePos + step.Item1 - step.Item2).normalized;
            CheckVertexInHashTable(vertC);
            CheckVertexInHashTable(vertD);
            AddBotBorderTriangle(cubePos.normalized, vertC, vertB);
            AddBotBorderTriangle(cubePos.normalized, vertD, vertC);
        }
    }

    void DrawEdgeFanRight(Vector3 cubePos, int vertY)
    {
        Vector3 vertA = (cubePos + step.Item1 * 2).normalized;
        Vector3 vertB = (cubePos + step.Item1 - step.Item2).normalized;
        CheckVertexInHashTable(vertA);
        CheckVertexInHashTable(vertB);
        if (vertY != chunkBaseResolution / 2)
        {
            AddRightBorderTriangle(cubePos.normalized, vertB, vertA);
        }
        if (vertY > 0 && vertY < chunkBaseResolution / 2)
        {
            Vector3 vertC = (cubePos - step.Item2).normalized;
            Vector3 vertD = (cubePos - step.Item1 - step.Item2).normalized;
            CheckVertexInHashTable(vertC);
            CheckVertexInHashTable(vertD);
            AddRightBorderTriangle(cubePos.normalized, vertC, vertB);
            AddRightBorderTriangle(cubePos.normalized, vertD, vertC);
        }
    }

    public void CalculateChunkBorder(int border, int sideWays)
    { 
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
        Vector3 vertA = (cubePos + step.Item2).normalized; 
        CheckVertexInHashTable(vertA);
        if (vertY == 0)
        {
            Vector3 vertB = (cubePos - step.Item1 + step.Item2).normalized;
            CheckVertexInHashTable(vertB);
            AddTopBorderTriangle(cubePos.normalized, vertA, vertB);
        }
        if (vertY == chunkBaseResolution - 1)
        {
            Vector3 vertB = (cubePos - step.Item1).normalized;
            CheckVertexInHashTable(vertB);
            AddTopBorderTriangle(cubePos.normalized, vertA, vertB);
        }
        if (vertY > 0 && vertY < chunkBaseResolution - 1)
        {
            Vector3 vertB = (cubePos - step.Item1).normalized;
            Vector3 vertC = (cubePos - step.Item1 + step.Item2).normalized;
            CheckVertexInHashTable(vertB);
            CheckVertexInHashTable(vertC);
            if (vertY % 2 == 1)
            {
                AddTopBorderTriangle(cubePos.normalized, vertA, vertB);
                AddTopBorderTriangle(vertA, vertC, vertB);
            }
            else
            {
                AddTopBorderTriangle(cubePos.normalized, vertC, vertB);
                AddTopBorderTriangle(cubePos.normalized, vertA, vertC);
            }
        }
    }

    public void DrawSimpleBorderLeft(Vector3 cubePos, int vertY)
    {
        Vector3 vertA = (cubePos + step.Item1).normalized;
        CheckVertexInHashTable(vertA);
        if (vertY == 0)
        {
            Vector3 vertB = (cubePos + step.Item1 + step.Item2).normalized;
            CheckVertexInHashTable(vertB);
            AddLeftBorderTriangle(cubePos.normalized, vertA, vertB);
        }
        if (vertY == chunkBaseResolution - 1)
        {
            Vector3 vertB = (cubePos + step.Item2).normalized;
            CheckVertexInHashTable(vertB);
            AddLeftBorderTriangle(cubePos.normalized, vertA, vertB);
        }
        if (vertY > 0 && vertY < chunkBaseResolution - 1)
        {
            Vector3 vertB = (cubePos + step.Item2).normalized;
            Vector3 vertC = (cubePos + step.Item1 + step.Item2).normalized;
            CheckVertexInHashTable(vertB);
            CheckVertexInHashTable(vertC);
            if (vertY % 2 == 1)
            {
                AddLeftBorderTriangle(cubePos.normalized, vertA, vertB);
                AddLeftBorderTriangle(vertA, vertC, vertB);
            }
            else
            {
                AddLeftBorderTriangle(cubePos.normalized, vertC, vertB);
                AddLeftBorderTriangle(cubePos.normalized, vertA, vertC);
            }
        }
    }

    public void DrawSimpleBorderBot(Vector3 cubePos, int vertY)
    {
        Vector3 vertA = (cubePos + step.Item2).normalized;
        CheckVertexInHashTable(vertA);
        if (vertY == 0)
        {
            Vector3 vertB = (cubePos + step.Item1 + step.Item2).normalized;
            CheckVertexInHashTable(vertB);
            AddBotBorderTriangle(cubePos.normalized, vertB, vertA);
        }
        if (vertY == chunkBaseResolution - 1)
        {
            Vector3 vertB = (cubePos + step.Item1).normalized;
            CheckVertexInHashTable(vertB);
            AddBotBorderTriangle(cubePos.normalized, vertB, vertA);
        }
        if (vertY > 0 && vertY < chunkBaseResolution - 1)
        {
            Vector3 vertB = (cubePos + step.Item1).normalized;
            Vector3 vertC = (cubePos + step.Item1 + step.Item2).normalized;
            CheckVertexInHashTable(vertB);
            CheckVertexInHashTable(vertC);
            if (vertY % 2 == 1)
            {
                AddBotBorderTriangle(cubePos.normalized, vertB, vertA);
                AddBotBorderTriangle(vertA, vertB, vertC);
            }
            else
            {
                AddBotBorderTriangle(cubePos.normalized, vertB, vertC);
                AddBotBorderTriangle(cubePos.normalized, vertC, vertA);
            }
        }
    }

    public void DrawSimpleBorderRight(Vector3 cubePos, int vertY)
    {
        Vector3 vertA = (cubePos + step.Item1).normalized;
        CheckVertexInHashTable(vertA);
        if (vertY == 0)
        {
            Vector3 vertB = (cubePos + step.Item1 - step.Item2).normalized;

            CheckVertexInHashTable(vertB);
            AddRightBorderTriangle(cubePos.normalized, vertB, vertA);
        }
        if (vertY == chunkBaseResolution - 1)
        {
            Vector3 vertB = (cubePos - step.Item2).normalized;
            CheckVertexInHashTable(vertB);
            AddRightBorderTriangle(cubePos.normalized, vertB, vertA);
        }
        if (vertY > 0 && vertY < chunkBaseResolution - 1)
        {
            Vector3 vertB = (cubePos - step.Item2).normalized;
            Vector3 vertC = (cubePos + step.Item1 - step.Item2).normalized;
            CheckVertexInHashTable(vertB);
            CheckVertexInHashTable(vertC);
            if (vertY % 2 == 1)
            {
                AddRightBorderTriangle(cubePos.normalized, vertB, vertA);
                AddRightBorderTriangle(vertA, vertB, vertC);
            }
            else
            {
                AddRightBorderTriangle(cubePos.normalized, vertB, vertC);
                AddRightBorderTriangle(cubePos.normalized, vertC, vertA);
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
        middleTriangles.Add((int)planetFace.verticeSN[vertA]);
        middleTriangles.Add((int)planetFace.verticeSN[vertB]);
        middleTriangles.Add((int)planetFace.verticeSN[vertC]);
    }

    void AddTopBorderTriangle(Vector3 vertA, Vector3 vertB, Vector3 vertC)
    {
        topBorderTriangles.Add((int)planetFace.verticeSN[vertA]);
        topBorderTriangles.Add((int)planetFace.verticeSN[vertB]);
        topBorderTriangles.Add((int)planetFace.verticeSN[vertC]);
    }

    void AddLeftBorderTriangle(Vector3 vertA, Vector3 vertB, Vector3 vertC)
    {
        leftBorderTriangles.Add((int)planetFace.verticeSN[vertA]);
        leftBorderTriangles.Add((int)planetFace.verticeSN[vertB]);
        leftBorderTriangles.Add((int)planetFace.verticeSN[vertC]);
    }

    void AddBotBorderTriangle(Vector3 vertA, Vector3 vertB, Vector3 vertC)
    {
        botBorderTriangles.Add((int)planetFace.verticeSN[vertA]);
        botBorderTriangles.Add((int)planetFace.verticeSN[vertB]);
        botBorderTriangles.Add((int)planetFace.verticeSN[vertC]);
    }

    void AddRightBorderTriangle(Vector3 vertA, Vector3 vertB, Vector3 vertC)
    {
        rightBorderTriangles.Add((int)planetFace.verticeSN[vertA]);
        rightBorderTriangles.Add((int)planetFace.verticeSN[vertB]);
        rightBorderTriangles.Add((int)planetFace.verticeSN[vertC]);
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
            if (Vector3.Angle(chunkPosition, planetScript.playerObj.position) < 90)
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
                    neighbours[0] = CheckIfNeighbourLODSmallerPathV(0);
                    neighbours[1] = CheckIfNeighbourLODSmallerPathV(1);
                    neighbours[2] = 0; // Bot
                    neighbours[3] = 0; // Right
                    break;
                }
            case '1': // Corners.topRight:
                {
                    neighbours[0] = CheckIfNeighbourLODSmallerPathV(0);
                    neighbours[1] = 0; // Left
                    neighbours[2] = 0; // Bot
                    neighbours[3] = CheckIfNeighbourLODSmallerPathV(3);
                    break;
                }
            case '2': // Corners.botLeft:
                {
                    neighbours[0] = 0; // Top
                    neighbours[1] = CheckIfNeighbourLODSmallerPathV(1);
                    neighbours[2] = CheckIfNeighbourLODSmallerPathV(2);
                    neighbours[3] = 0; // Right
                    break;
                }
            case '3': // Corners.botRight:
                {
                    neighbours[0] = 0; // Top
                    neighbours[1] = 0; // Left
                    neighbours[2] = CheckIfNeighbourLODSmallerPathV(2);
                    neighbours[3] = CheckIfNeighbourLODSmallerPathV(3);

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
