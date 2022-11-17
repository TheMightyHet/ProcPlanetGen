using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public List<Color> colors = new();

    public int offset = 0;
    public int set = 0;
    public string dir = "";

    public List<Vector3> faceVertices = new();
    public Hashtable verticeSN = new();
    public int hashCounter = 0;

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
        colors.Clear(); 
        displayedChunk = new List<Chunk>();

        baseChunk = new Chunk(null, "0", planetScript, this, null, localUp * planetScript.planetRadius, planetScript.planetRadius, 0, Corners.middle, BorderPositions.root, null);
        baseChunk.GenerateSubChunks();
        baseChunk.GetSubChunks();

        foreach (Chunk chunkPart in displayedChunk)
        {
            (Vector3[], Vector3[], int[], Color[]) chunkData = chunkPart.GetSubChunkData();
            vertices.AddRange(chunkData.Item1);
            normals.AddRange(chunkData.Item2);
            triangles.AddRange(chunkData.Item3);
            colors.AddRange(chunkData.Item4);

            offset += chunkData.Item1.Length;
        }

        if (dir == "Forward")
        {
            List<Vector3> notInHashTableVertices = new List<Vector3>();

            foreach (var vert in vertices)
            {
                if (!verticeSN.ContainsKey(vert))
                {
                    notInHashTableVertices.Add(vert);
                }
            }


            Debug.Log(notInHashTableVertices.Count + ", " + verticeSN.Count);

            var asd = "HashTable: ";

            foreach (DictionaryEntry htEntry in verticeSN)
            {
                asd += "(" + htEntry.Key + ": " + htEntry.Value + "), ";
            }
            Debug.Log(asd);
        }

        
        offset = 0;

        mesh.Clear();
        mesh.vertices = faceVertices.ToArray();
        mesh.triangles = triangles.ToArray();
        //mesh.vertices = newVerts.ToArray();
        //mesh.triangles = tris;
        mesh.RecalculateNormals();
    }

    public void UpdateChunkMesh()
    {
        vertices.Clear();
        triangles.Clear();
        normals.Clear();
        colors.Clear();
        displayedChunk = new List<Chunk>();

        baseChunk.UpdateChunk();
        baseChunk.GetSubChunks();


        foreach (Chunk chunkPart in displayedChunk)
        {
            (Vector3[], Vector3[], int[], Color[]) chunkData;

            if (chunkPart.vertices == null)
                chunkData = chunkPart.GetSubChunkData();
            else if (chunkPart.vertices.Length == 0)
                chunkData = chunkPart.GetSubChunkData();
            else
                chunkData = (chunkPart.vertices, chunkPart.normals, chunkPart.triangles, chunkPart.colors);

            vertices.AddRange(chunkData.Item1);
            normals.AddRange(chunkData.Item2);
            triangles.AddRange(chunkData.Item3);
            colors.AddRange(chunkData.Item4);
            offset += chunkData.Item1.Length;
        }


        Vector3[] verts = vertices.ToArray();
        int[] tris = triangles.ToArray();

        List<Vector3> newVerts = new List<Vector3>();

        foreach (Vector3 vert in verts)
        {
            foreach (Vector3 newVert in newVerts)
                if (vert.Equals(newVert))
                    goto skipToNext;

            newVerts.Add(vert);

        skipToNext:;
        }

        for (int i = 0; i < tris.Length; ++i)
        {
            for (int j = 0; j < newVerts.Count; ++j)
            {
                if (newVerts[j].Equals(verts[tris[i]]))
                {
                    tris[i] = j;
                    break;
                }
            }
        }


        offset = 0;

        mesh.Clear();
        mesh.vertices = newVerts.ToArray();
        mesh.triangles = tris;
        mesh.RecalculateNormals();
    }
}