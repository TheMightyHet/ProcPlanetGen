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
    public NoiseFilter noiseFilter;

    public int chunkBaseResolution = ChunkTemplate.chunkResolution;

    public Vector3 chunkPosition;
    public Chunk[] subChunks;

    public float chunkRadius;
    public int chunkLODLevel;

    public (Vector3, Vector3) step;

    public Vector3[,] chunkVertices = new Vector3[ChunkTemplate.chunkResolution + 1, ChunkTemplate.chunkResolution + 1];

    public List<int> chunkMiddleTrianglesList = new();

    public List<int> chunkBorderTrianglesList = new();


    public List<int> chunkTriangles = new();

    public int[] neighbours;

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

        Matrix4x4 transformMatrix;
        Vector3 rotationMatrixAttrib = new Vector3(0, 0, 0);
        Vector3 scaleMatrixAttrib = new Vector3(chunkRadius, chunkRadius, 1);

        if (planetFace.localUp == Vector3.forward)
        {
            rotationMatrixAttrib = new Vector3(0, 0, 0);
        }
        else if (planetFace.localUp == Vector3.back)
        {
            rotationMatrixAttrib = new Vector3(0, 180, 180);
        }
        else if (planetFace.localUp == Vector3.right)
        {
            rotationMatrixAttrib = new Vector3(0, 90, 90);
        }
        else if (planetFace.localUp == Vector3.left)
        {
            rotationMatrixAttrib = new Vector3(0, 270, 90);
        }
        else if (planetFace.localUp == Vector3.up)
        {
            rotationMatrixAttrib = new Vector3(270, 0, 270);
        }
        else if (planetFace.localUp == Vector3.down)
        {
            rotationMatrixAttrib = new Vector3(90, 0, 90);
        }

        transformMatrix = Matrix4x4.TRS(chunkPosition, Quaternion.Euler(rotationMatrixAttrib), scaleMatrixAttrib);


        for (int i = 0; i < chunkBaseResolution + 1; ++i)
        {
            for (int j = 0; j < chunkBaseResolution + 1; ++j)
            {
                Vector3 pointOnSphere = transformMatrix.MultiplyPoint(ChunkTemplate.templateVertices[i, j]).normalized;
                chunkVertices[i, j] = pointOnSphere;
            }
        }
    }

    public void GenerateSubChunks()
    {
        if (chunkLODLevel <= planetScript.distanceLOD.Length - 1 && chunkLODLevel >= 0)
        {
            float distToPlayerObj = Vector3.Distance(chunkPosition.normalized * planetScript.planetRadius + planetScript.transform.position, planetScript.playerObj.position);

            if (distToPlayerObj <= planetScript.distanceLOD[chunkLODLevel])
            {
                subChunks = new Chunk[4];

                Vector3 topLeft = chunkPosition + (.5f * chunkRadius * planetFace.axisA) - (.5f * chunkRadius * planetFace.axisB);
                Vector3 topRight = chunkPosition + (.5f * chunkRadius * planetFace.axisA) + (.5f * chunkRadius * planetFace.axisB);
                Vector3 botLeft = chunkPosition - (.5f * chunkRadius * planetFace.axisA) - (.5f * chunkRadius * planetFace.axisB);
                Vector3 botRight = chunkPosition - (.5f * chunkRadius * planetFace.axisA) + (.5f * chunkRadius * planetFace.axisB);

                subChunks[0] = new Chunk(path + "0", planetScript, planetFace, new Chunk[0], topLeft, chunkRadius * .5f, chunkLODLevel + 1, new int[4]);
                subChunks[1] = new Chunk(path + "1", planetScript, planetFace, new Chunk[0], topRight, chunkRadius * .5f, chunkLODLevel + 1, new int[4]);
                subChunks[2] = new Chunk(path + "2", planetScript, planetFace, new Chunk[0], botLeft, chunkRadius * .5f, chunkLODLevel + 1, new int[4]);
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
        AddVertices();
        ClearTriangles();

        CalculateChunkMiddleTriangles();
        CalculateChunkBorderTriangles();

        chunkTriangles.AddRange(chunkMiddleTrianglesList);
        chunkTriangles.AddRange(chunkBorderTrianglesList);

        return chunkTriangles;
    }

    void AddVertices()
    {
        for (int i = 0; i < chunkBaseResolution + 1; i++)
        {
            for (int j = 0; j < chunkBaseResolution + 1; j++)
            {
                if (i != 0 && j != 0 && i != chunkBaseResolution && j != chunkBaseResolution)
                {
                    planetFace.faceVertices.Add(chunkVertices[i, j]);
                    planetFace.verticeSN.Add(chunkVertices[i, j], planetFace.hashCounter++);
                }
                else
                    CheckVertexInHashTable(chunkVertices[i, j]);

            }
        }
    }

    void ClearTriangles()
    {
        chunkTriangles.Clear();

        chunkMiddleTrianglesList.Clear();
        chunkBorderTrianglesList.Clear();
    }

    void CalculateChunkMiddleTriangles()
    {
        for (int i = 0; i < ChunkTemplate.templateMiddleTriangles.Length; ++i)
        {
            chunkMiddleTrianglesList.Add((int)planetFace.verticeSN[chunkVertices[ChunkTemplate.templateMiddleTriangles[i].Item1, ChunkTemplate.templateMiddleTriangles[i].Item2]]);
        }
    }

    void CalculateChunkBorderTriangles()
    {
        CalculateNeighbouringChunkLODs();

        if (neighbours[0] == 0)
            for (int i = 0; i < ChunkTemplate.templateTopTriangles.Length; ++i)
            {
                chunkBorderTrianglesList.Add((int)planetFace.verticeSN[chunkVertices[ChunkTemplate.templateTopTriangles[i].Item1, ChunkTemplate.templateTopTriangles[i].Item2]]);
            }
        else
            for (int i = 0; i < ChunkTemplate.templateTopEdgeFanTriangles.Length; ++i)
            {
                chunkBorderTrianglesList.Add((int)planetFace.verticeSN[chunkVertices[ChunkTemplate.templateTopEdgeFanTriangles[i].Item1, ChunkTemplate.templateTopEdgeFanTriangles[i].Item2]]);
            }


        if (neighbours[2] == 0)
            for (int i = 0; i < ChunkTemplate.templateBotTriangles.Length; ++i)
            {
                chunkBorderTrianglesList.Add((int)planetFace.verticeSN[chunkVertices[ChunkTemplate.templateBotTriangles[i].Item1, ChunkTemplate.templateBotTriangles[i].Item2]]);
            }
        else
            for (int i = 0; i < ChunkTemplate.templateBotEdgeFanTriangles.Length; ++i)
            {
                chunkBorderTrianglesList.Add((int)planetFace.verticeSN[chunkVertices[ChunkTemplate.templateBotEdgeFanTriangles[i].Item1, ChunkTemplate.templateBotEdgeFanTriangles[i].Item2]]);
            }


        if (neighbours[1] == 0)
            for (int i = 0; i < ChunkTemplate.templateRightTriangles.Length; ++i)
            {
                chunkBorderTrianglesList.Add((int)planetFace.verticeSN[chunkVertices[ChunkTemplate.templateRightTriangles[i].Item1, ChunkTemplate.templateRightTriangles[i].Item2]]);
            }
        else
            for (int i = 0; i < ChunkTemplate.templateRightEdgeFanTriangles.Length; ++i)
            {
                chunkBorderTrianglesList.Add((int)planetFace.verticeSN[chunkVertices[ChunkTemplate.templateRightEdgeFanTriangles[i].Item1, ChunkTemplate.templateRightEdgeFanTriangles[i].Item2]]);
            }


        if (neighbours[3] == 0)
            for (int i = 0; i < ChunkTemplate.templateLeftTriangles.Length; ++i)
            {
                chunkBorderTrianglesList.Add((int)planetFace.verticeSN[chunkVertices[ChunkTemplate.templateLeftTriangles[i].Item1, ChunkTemplate.templateLeftTriangles[i].Item2]]);
            }
        else
            for (int i = 0; i < ChunkTemplate.templateLeftEdgeFanTriangles.Length; ++i)
            {
                chunkBorderTrianglesList.Add((int)planetFace.verticeSN[chunkVertices[ChunkTemplate.templateLeftEdgeFanTriangles[i].Item1, ChunkTemplate.templateLeftEdgeFanTriangles[i].Item2]]);
            }
    }

    void CheckVertexInHashTable(Vector3 vertex)
    {
        if (!planetFace.verticeSN.ContainsKey(vertex))
        {
            planetFace.faceVertices.Add(vertex);
            planetFace.verticeSN.Add(vertex, planetFace.hashCounter++);
        }
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

    public void CalculateNeighbouringChunkLODs()
    {
        //Top, Left, Bot, Right
        switch (path[^1]) // (corner)
        {
            case '0':
                {
                    neighbours[0] = CheckNeighbourLOD(0);
                    neighbours[1] = CheckNeighbourLOD(1);
                    neighbours[2] = 0;
                    neighbours[3] = 0;
                    break;
                }
            case '1':
                {
                    neighbours[0] = CheckNeighbourLOD(0);
                    neighbours[1] = 0;
                    neighbours[2] = 0;
                    neighbours[3] = CheckNeighbourLOD(3);
                    break;
                }
            case '2':
                {
                    neighbours[0] = 0;
                    neighbours[1] = CheckNeighbourLOD(1);
                    neighbours[2] = CheckNeighbourLOD(2);
                    neighbours[3] = 0;
                    break;
                }
            case '3':
                {
                    neighbours[0] = 0;
                    neighbours[1] = 0;
                    neighbours[2] = CheckNeighbourLOD(2);
                    neighbours[3] = CheckNeighbourLOD(3);

                    break;
                }
            default: break;
        }
    }

    public int CheckNeighbourLOD(int dir)
    {
        string neighbourPath = InvertPath(dir);
        if (neighbourPath == "") return 0;
        Chunk neighbour = planetFace.baseChunk;

        while (neighbourPath.Length > 0)
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

    public string InvertPath(int dir)
    {
        string chunkPath = path;
        chunkPath = chunkPath.Remove(0, 1); // Remove baseChunk
        string neighbourPath = "";

        while (chunkPath.Length > 0)
        {
            if (!HasSiblingTowards(dir, chunkPath[^1]))
            {
                neighbourPath = neighbourPath.Insert(0, InvertDirection(dir, chunkPath[^1]));
                chunkPath = chunkPath.Remove(chunkPath.Length - 1);
                if (chunkPath.Length == 0) return "";
            }
            else if (HasSiblingTowards(dir, chunkPath[^1]))
            {
                neighbourPath = neighbourPath.Insert(0, InvertDirection(dir, chunkPath[^1]));
                chunkPath = chunkPath.Remove(chunkPath.Length - 1);
                break;
            }
        }
        neighbourPath = neighbourPath.Insert(0, chunkPath);
        return neighbourPath;
    }

    public string InvertDirection(int dir, char pathPart)
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

    public bool HasSiblingTowards(int dir, char pathPart)
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
