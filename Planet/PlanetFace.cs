using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetFace
{
    public volatile Mesh mesh;
    public Vector3 localUp;
    public Vector3 axisA;
    public Vector3 axisB;

    public Chunk baseChunk;
    public List<Chunk> displayedChunk = new();
    public Planet planetScript;

    public List<Vector3> vertices = new();
    public List<int> triangles = new();
    public List<Vector3> normals = new();

    public int offset = 0;
    public int set = 0;
    public string dir = "";

    public enum Corners { topLeft, topRight, botLeft, botRight, middle }
    public enum BorderPositions { topLeftCorner, topRightCorner, botLeftCorner, botRightCorner, topMidBorder, botMidBorder, leftMidBorder, rightMidBorder, middle, root}

    public PlanetFace(Mesh mesh, Vector3 localUp, Planet planetScript, string dir)
    {
        this.mesh = mesh;
        this.localUp = localUp;
        this.planetScript = planetScript;
        this.dir = dir;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    }

    public void CreateChunkMesh()
    {
        vertices.Clear();
        triangles.Clear();
        normals.Clear();
        displayedChunk = new List<Chunk>();

        baseChunk = new Chunk(null, "0", planetScript, this, null, localUp * planetScript.planetRadius, planetScript.planetRadius, 0, Corners.middle, BorderPositions.root, null);
        baseChunk.GenerateSubChunks();
        baseChunk.GetSubChunks();

        foreach (Chunk chunkPart in displayedChunk)
        {
            (Vector3[], Vector3[], int[]) chunkData = chunkPart.GetSubChunkData();
            vertices.AddRange(chunkData.Item1);
            normals.AddRange(chunkData.Item2);
            triangles.AddRange(chunkData.Item3);

            offset += chunkData.Item1.Length;
        }
        
        offset = 0;

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.triangles = triangles.ToArray();
    }

    public void UpdateChunkMesh()
    {
        vertices.Clear();
        triangles.Clear();
        normals.Clear();
        displayedChunk = new List<Chunk>();

        baseChunk.UpdateChunk();
        baseChunk.GetSubChunks();


        foreach (Chunk chunkPart in displayedChunk)
        {
            (Vector3[], Vector3[], int[]) chunkData;

            if (chunkPart.vertices == null)
                chunkData = chunkPart.GetSubChunkData();
            else if (chunkPart.vertices.Length == 0)
                chunkData = chunkPart.GetSubChunkData();
            else
                chunkData = (chunkPart.vertices, chunkPart.normals, chunkPart.GetTriangles());

            vertices.AddRange(chunkData.Item1);
            normals.AddRange(chunkData.Item2);
            triangles.AddRange(chunkData.Item3);
            offset += chunkData.Item1.Length;
        }
        
        offset = 0;

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.triangles = triangles.ToArray();
    }
}