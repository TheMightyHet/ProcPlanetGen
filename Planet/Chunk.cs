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
    public Chunk[] subChunks;
    public Vector3 chunkPosition;
    public float chunkRadius;
    public int chunkLODLevel;
    public int[] neighbouringChunks; // top, left, bot, right

    public (Vector3, Vector3) step;
    public Vector3[,] chunkVertices = new Vector3[ChunkTemplate.chunkResolution + 1, ChunkTemplate.chunkResolution + 1];
    public List<int> chunkMiddleTrianglesList = new();
    public List<int> chunkBorderTrianglesList = new();
    public List<int> chunkTriangles = new();

    public Chunk(string path, Planet planetScript, PlanetFace planetFace, Chunk[] subChunks, Vector3 chunkPosition, float chunkRadius, int chunkLODLevel, int[] neighbouringChunks)
    {
        this.path = path;
        this.planetScript = planetScript;
        this.planetFace = planetFace;
        this.subChunks = subChunks;
        this.chunkPosition = chunkPosition;
        this.chunkRadius = chunkRadius;
        this.chunkLODLevel = chunkLODLevel;
        this.neighbouringChunks = neighbouringChunks;

        step = ((2f / (float)ChunkTemplate.chunkResolution) * chunkRadius * planetFace.axisA, (2f / (float)ChunkTemplate.chunkResolution) * chunkRadius * planetFace.axisB);

        Matrix4x4 transformMatrix;
        int templDir = planetFace.localUp != Vector3.forward ? 
                       planetFace.localUp == Vector3.back ? 1 : 
                       planetFace.localUp == Vector3.up ? 2 :
                       planetFace.localUp == Vector3.down ? 3 :
                       planetFace.localUp == Vector3.right ? 4 : 5 : 0;
        Vector3 scale = templDir == 0 || templDir == 1 ? new Vector3(chunkRadius, chunkRadius, 1) :
                        templDir == 2 || templDir == 3 ? new Vector3(chunkRadius, 1, chunkRadius) :
                        new Vector3(1, chunkRadius, chunkRadius);
        transformMatrix = Matrix4x4.TRS(chunkPosition, Quaternion.Euler(new Vector3(0, 0, 0)), scale);

        for (int i = 0; i < ChunkTemplate.chunkResolution + 1; ++i)
        {
            for (int j = 0; j < ChunkTemplate.chunkResolution + 1; ++j)
            {
                Vector3 pointOnSphere = transformMatrix.MultiplyPoint(ChunkTemplate.templateVertices[templDir][i, j]).normalized;
                chunkVertices[i, j] = pointOnSphere;
            }
        }
    }

    public void GenerateSubChunks(Vector3 playerPos)
    {
        if (chunkLODLevel <= planetScript.distanceLOD.Length - 1 && chunkLODLevel >= 0)
        {
            float distToPlayerObj = Vector3.Distance(chunkPosition.normalized * planetScript.planetRadius + planetScript.transform.position, playerPos);

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
                    chunk.GenerateSubChunks(playerPos);
                }
            }
        }
    }

    public void UpdateChunk(Vector3 playerPos)
    {
        if (chunkLODLevel <= planetScript.distanceLOD.Length - 1)
        {
            float distToPlayerObj = Vector3.Distance(chunkPosition.normalized * planetScript.planetRadius, playerPos);

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
                        chunk.UpdateChunk(playerPos);
                    }
                }
                else
                {
                    GenerateSubChunks(playerPos);
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
        for (int i = 0; i < ChunkTemplate.chunkResolution + 1; i++)
        {
            for (int j = 0; j < ChunkTemplate.chunkResolution + 1; j++)
            {
                if (i != 0 && j != 0 && i != ChunkTemplate.chunkResolution && j != ChunkTemplate.chunkResolution)
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
            chunkMiddleTrianglesList.Add((int)planetFace.verticeSN[chunkVertices[ChunkTemplate.templateMiddleTriangles[i].Item1, ChunkTemplate.templateMiddleTriangles[i].Item2]]);
    }

    void CalculateChunkBorderTriangles()
    {
        CalculateNeighbouringChunkLODs();
        if (neighbouringChunks[0] == 1)
            for (int i = 0; i < ChunkTemplate.templateTopEdgeFanTriangles.Length; ++i)
                chunkBorderTrianglesList.Add((int)planetFace.verticeSN[chunkVertices[ChunkTemplate.templateTopEdgeFanTriangles[i].Item1, ChunkTemplate.templateTopEdgeFanTriangles[i].Item2]]);
        else
            for (int i = 0; i < ChunkTemplate.templateTopTriangles.Length; ++i)
                chunkBorderTrianglesList.Add((int)planetFace.verticeSN[chunkVertices[ChunkTemplate.templateTopTriangles[i].Item1, ChunkTemplate.templateTopTriangles[i].Item2]]);


        if (neighbouringChunks[2] == 1)
            for (int i = 0; i < ChunkTemplate.templateBotEdgeFanTriangles.Length; ++i)
                chunkBorderTrianglesList.Add((int)planetFace.verticeSN[chunkVertices[ChunkTemplate.templateBotEdgeFanTriangles[i].Item1, ChunkTemplate.templateBotEdgeFanTriangles[i].Item2]]);
        else
            for (int i = 0; i < ChunkTemplate.templateBotTriangles.Length; ++i)
                chunkBorderTrianglesList.Add((int)planetFace.verticeSN[chunkVertices[ChunkTemplate.templateBotTriangles[i].Item1, ChunkTemplate.templateBotTriangles[i].Item2]]);


        if (neighbouringChunks[1] == 1)
            for (int i = 0; i < ChunkTemplate.templateLeftEdgeFanTriangles.Length; ++i)
                chunkBorderTrianglesList.Add((int)planetFace.verticeSN[chunkVertices[ChunkTemplate.templateLeftEdgeFanTriangles[i].Item1, ChunkTemplate.templateLeftEdgeFanTriangles[i].Item2]]);
        else
            for (int i = 0; i < ChunkTemplate.templateLeftTriangles.Length; ++i)
                chunkBorderTrianglesList.Add((int)planetFace.verticeSN[chunkVertices[ChunkTemplate.templateLeftTriangles[i].Item1, ChunkTemplate.templateLeftTriangles[i].Item2]]);


        if (neighbouringChunks[3] == 1)
            for (int i = 0; i < ChunkTemplate.templateRightEdgeFanTriangles.Length; ++i)
                chunkBorderTrianglesList.Add((int)planetFace.verticeSN[chunkVertices[ChunkTemplate.templateRightEdgeFanTriangles[i].Item1, ChunkTemplate.templateRightEdgeFanTriangles[i].Item2]]);
        else
            for (int i = 0; i < ChunkTemplate.templateRightTriangles.Length; ++i)
                chunkBorderTrianglesList.Add((int)planetFace.verticeSN[chunkVertices[ChunkTemplate.templateRightTriangles[i].Item1, ChunkTemplate.templateRightTriangles[i].Item2]]);
    }

    void CheckVertexInHashTable(Vector3 vertex)
    {
        if (!planetFace.verticeSN.ContainsKey(vertex))
        {
            planetFace.faceVertices.Add(vertex);
            planetFace.verticeSN.Add(vertex, planetFace.hashCounter++);
        }
    }

    public void GetSubChunks(Vector3 playerPos)
    {
        if (subChunks.Length > 0)
            foreach (Chunk subChunk in subChunks)
                subChunk.GetSubChunks(playerPos);
        else
            if (Vector3.Angle(chunkPosition, playerPos) < 90) 
                planetFace.displayedChunk.Add(this);
    }

    public void CalculateNeighbouringChunkLODs()
    {
        if (path[^1] == '0')
        {
            neighbouringChunks[0] = CheckNeighbourLOD(0);
            neighbouringChunks[1] = CheckNeighbourLOD(1);
            neighbouringChunks[2] = 0;
            neighbouringChunks[3] = 0;
        }
        if (path[^1] == '1')
        {
            neighbouringChunks[0] = CheckNeighbourLOD(0);
            neighbouringChunks[1] = 0;
            neighbouringChunks[2] = 0;
            neighbouringChunks[3] = CheckNeighbourLOD(3);
        }
        if (path[^1] == '2')
        {
            neighbouringChunks[0] = 0;
            neighbouringChunks[1] = CheckNeighbourLOD(1);
            neighbouringChunks[2] = CheckNeighbourLOD(2);
            neighbouringChunks[3] = 0;
        }
        if (path[^1] == '3')
        {
            neighbouringChunks[0] = 0;
            neighbouringChunks[1] = 0;
            neighbouringChunks[2] = CheckNeighbourLOD(2);
            neighbouringChunks[3] = CheckNeighbourLOD(3);
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
            if (pathPart == '0') return "1";
            else if (pathPart == '1') return "0";
            else if (pathPart == '2') return "3";
            else if (pathPart == '3') return "2";
        }
        else if ((dir % 2) == 0)
        {
            if (pathPart == '0') return "2";
            else if (pathPart == '1') return "3";
            else if (pathPart == '2') return "0";
            else if (pathPart == '3') return "1";
        }
        return "";
    }

    public bool HasSiblingTowards(int dir, char pathPart)
    {
        if (dir == 0)
        {
            if (pathPart == '2' || pathPart == '3') return true;
            else if (pathPart == '1' || pathPart == '4') return false;
        }
        else if (dir == 1)
        {
            if (pathPart == '1' || pathPart == '3') return true;
            else if (pathPart == '0' || pathPart == '2') return false;
        }
        else if (dir == 2)
        {
            if (pathPart == '0' || pathPart == '1') return true;
            else if (pathPart == '2' || pathPart == '3') return false;
        }
        else if (dir == 3)
        {
            if (pathPart == '0' || pathPart == '2') return true;
            else if (pathPart == '1' || pathPart == '3') return false;
        }
        return false;
    }
}
