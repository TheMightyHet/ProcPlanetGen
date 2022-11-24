using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    public Vector3[] normals;
    public List<Color> colors = new();

    public string dir = "";

    public List<float> vertElevation = new();

    public List<Vector3> faceVertices = new();
    public Hashtable verticeSN = new();
    public int hashCounter = 0;

    public PlanetFace(Mesh mesh, Vector3 localUp, Planet planetScript, string dir)
    {
        this.mesh = mesh;
        this.localUp = localUp;
        this.planetScript = planetScript;
        this.dir = dir;

        axisA = /*dir == "Back" ? new Vector3(0, 1, 0) :*/ new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = /*dir == "Back" ? new Vector3(-1, 0, 0) :*/ Vector3.Cross(localUp, axisA);

        Debug.Log(dir + ", " + localUp + ", " + axisA + ", " + axisB);

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    }

    public void CreateChunkMesh()
    {
        vertices.Clear();
        triangles.Clear();
        colors.Clear();
        vertElevation.Clear();
        displayedChunk = new List<Chunk>();

        baseChunk = new Chunk("0", planetScript, this, null, localUp, 1, 0, null);
        baseChunk.GenerateSubChunks();
        baseChunk.GetSubChunks();
        

        foreach (Chunk chunkPart in displayedChunk)
        {
            triangles.AddRange(chunkPart.GetChunkData());
        }

        for (int i = 0; i < faceVertices.Count; i++)
        {
            float elevation = (1 + planetScript.noiseFilter.Evaluate(faceVertices[i])) * planetScript.planetRadius;
            vertices.Add(elevation * faceVertices[i]);
            vertElevation.Add(elevation);

            if (elevation > planetScript.maxElevation) { planetScript.maxElevation = elevation; }
            if (elevation < planetScript.minElevation) { planetScript.minElevation = elevation; }
        }



        /*foreach (var e in vertElevation)
        {
            float height = Mathf.InverseLerp(planetScript.minElevation, planetScript.maxElevation, e);
            colors.Add(planetScript.gradient.Evaluate(height));
        }*/


        mesh.Clear();
        mesh.vertices = vertices.ToArray();//faceVertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
    }

    public void UpdateChunkMesh()
    {
        vertices.Clear();
        triangles.Clear();
        colors.Clear();
        vertElevation.Clear();

        faceVertices.Clear();
        verticeSN.Clear();
        triangles.Clear();
        hashCounter = 0;

        displayedChunk = new List<Chunk>();

        baseChunk.UpdateChunk();
        baseChunk.GetSubChunks();

        foreach (Chunk chunkPart in displayedChunk)
        {
            triangles.AddRange(chunkPart.GetChunkData());
        }

        for (int i = 0; i < faceVertices.Count; i++)
        {
            float elevation = (1 + planetScript.noiseFilter.Evaluate(faceVertices[i])) * planetScript.planetRadius;
            vertices.Add(elevation * faceVertices[i]);
            vertElevation.Add(elevation);

            if (elevation > planetScript.maxElevation) { planetScript.maxElevation = elevation; }
            if (elevation < planetScript.minElevation) { planetScript.minElevation = elevation; }
        }

        normals = new Vector3[vertices.Count];

        int triangleCount = triangles.Count / 3;

        int vertexIndexA;
        int vertexIndexB;
        int vertexIndexC;

        Vector3 triangleNormal; 
        
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            vertexIndexA = triangles[normalTriangleIndex];
            vertexIndexB = triangles[normalTriangleIndex + 1];
            vertexIndexC = triangles[normalTriangleIndex + 2];

            triangleNormal = GetSurfaceNormal(vertexIndexA, vertexIndexB, vertexIndexC);

            normals[vertexIndexA] += triangleNormal;
            normals[vertexIndexB] += triangleNormal;
            normals[vertexIndexC] += triangleNormal;
        }
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i].Normalize();
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();//faceVertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors = colors.ToArray();
        mesh.normals = normals;
        mesh.RecalculateNormals();
    }

    Vector3 GetSurfaceNormal(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = vertices[indexA];
        Vector3 pointB = vertices[indexB];
        Vector3 pointC = vertices[indexC];

        // Get an aproximation of the vertex normal using two other vertices that share the same triangle
        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }
}