using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetFaceGrid
{
    public struct ChunkData
    {
        public Vector3 position;
        public Vector3[] vertices;
        public int[] triangles;
        public Vector3[] normals;

        public ChunkData(Vector3 position, Vector3[] vertices, int[] triangles, Vector3[] normals)
        {
            this.position = position;
            this.vertices = vertices;
            this.triangles = triangles;
            this.normals = normals;
        }
    }

    public ChunkData[] chunksOnFace;
    public List<ChunkData> visibleChunks;

    public volatile Mesh Mesh;
    public Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;

    public Vector3[] vertices;
    public int[] triangles;
    public Vector3[] normals;

    public int[] chunkPositions;
    public int gridDimensions = 0;

    public int offset = 0;
    public string dir = "";

    public PlanetFaceGrid(Mesh mesh, Vector3 localUp, PlanetGrid planetScript, string dir)
    {
    }
}