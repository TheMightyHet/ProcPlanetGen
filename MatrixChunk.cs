using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatrixChunk
{
    public struct ChunkData
    { 
        public Vector3 Position;
        public int LOD;

        public ChunkData(Vector3 Position, int LOD)
        { 
            this.Position = Position;
            this.LOD = LOD;
        }
    }

    public ChunkData[][] subChunks;
    public int gridDimensions = 0;

    //public Planet planetScript;
    public volatile Mesh mesh;
    public Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;

    public Vector3[] vertices;
    public int[] triangles;
    public Vector3[] normals;

    public List<Vector3> verticesColl = new List<Vector3>();
    public List<int> trianglesColl = new List<int>();
    public List<Vector3> normalsColl = new List<Vector3>();
    public List<ChunkData> visibleChunks = new List<ChunkData>();

    public int offset = 0;
    public string dir = "";

    public MatrixChunk(Mesh mesh, Vector3 localUp, Planet planetScript, string dir)
    {
        this.mesh = mesh;
        this.localUp = localUp;
        //this.planetScript = planetScript;
        this.dir = dir;
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
        gridDimensions = (int)Math.Pow(2, planetScript.distanceLOD.Length) * (int)(planetScript.planetRadius / planetScript.distanceLOD.Length / 20);
        subChunks = new ChunkData[gridDimensions][];
    }

    public void CreateChunkMesh()
    {
        mesh.Clear();
        GenerateSubChunks();
        GetVisibleChunks();
        DrawCunks();
    }

    public void GenerateSubChunks()
    {
        for (int y = 0; y < gridDimensions; ++y)
        {
            ChunkData[] chunkLine = new ChunkData[gridDimensions];
            for (int x = 0; x < gridDimensions; ++x)
            {
                Vector2 percent = new Vector2(x, y) / gridDimensions;
                Vector3 pos = localUp + (percent.x - .5f + (1f / gridDimensions / 2)) * axisA * 2 + (percent.y - .5f + (1f / gridDimensions / 2)) * axisB * 2;
              //  float distToPlayer = Vector3.Distance(pos.normalized * planetScript.planetRadius + planetScript.transform.position, planetScript.playerObj.position);

                int lod = 0;
                //for (int i = 0; i < planetScript.distanceLOD.Length; ++i)
                  //  if (distToPlayer < planetScript.distanceLOD[i])
                   //     lod = i;

                chunkLine[x] = new ChunkData(pos, lod);
            }
            subChunks[y] = chunkLine;
        }
    }

    public void DrawCunks()
    {
        verticesColl.Clear();
        trianglesColl.Clear();
        normalsColl.Clear();
        foreach (ChunkData chunk in visibleChunks)
        {
            int subChunkGrid = (int)Math.Pow(2, chunk.LOD);
            vertices = new Vector3[(subChunkGrid + 1) * (subChunkGrid + 1)];
            triangles = new int[6 * subChunkGrid * subChunkGrid];
            normals = new Vector3[vertices.Length];
            int triangeOffset = 0;

            if(chunk.LOD > 1)
            for (int y = 0; y < subChunkGrid + 1; ++y)
            {
                for (int x = 0; x < subChunkGrid + 1; ++x)
                {
                    int i = x + y * (subChunkGrid + 1);
                    Vector2 percent = new Vector2(x, y) / (subChunkGrid);
                    Vector3 pointOnCube = chunk.Position + (percent.x - .5f) * axisA / (gridDimensions / 2) + (percent.y - .5f) * axisB / (gridDimensions / 2);

                    normals[i] = pointOnCube.normalized;
                   // vertices[i] = pointOnCube.normalized * planetScript.planetRadius;

                    if (x != subChunkGrid && y != subChunkGrid)
                    {
                        triangles[triangeOffset] = i + offset;
                        triangles[triangeOffset + 1] = i + (subChunkGrid + 1) + 1 + offset;
                        triangles[triangeOffset + 2] = i + (subChunkGrid + 1) + offset;

                        triangles[triangeOffset + 3] = i + offset;
                        triangles[triangeOffset + 4] = i + 1 + offset;
                        triangles[triangeOffset + 5] = i + (subChunkGrid + 1) + 1 + offset;

                        triangeOffset += 6;
                    }
                }
            }
            offset += (subChunkGrid + 1) * (subChunkGrid + 1); 
            verticesColl.AddRange(vertices);
            trianglesColl.AddRange(triangles);
            normalsColl.AddRange(normals);
        }
        offset = 0;

        mesh.Clear();
        mesh.vertices = verticesColl.ToArray();
        mesh.triangles = trianglesColl.ToArray();
        mesh.RecalculateNormals();
    }

    public void GetVisibleChunks()
    {
        visibleChunks.Clear();
        foreach (ChunkData[] chunkArray in subChunks)
            foreach (ChunkData chunk in chunkArray)
            {
              //  Vector3 playerDirection = (chunk.Position - planetScript.playerObj.position).normalized;
               // if (Vector3.Angle(chunk.Position, playerDirection) > 120)
                {
                    visibleChunks.Add(chunk);
                }
            }
    }
}