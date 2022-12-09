using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class PlanetFace
{

    public volatile Mesh mesh;
    public Vector3 localUp;
    public Planet planetScript;
    public string dir = "";

    public Vector3 axisA;
    public Vector3 axisB;

    public List<Vector3> faceVertices = new();
    public Hashtable verticeSN = new();

    public Chunk baseChunk;
    public List<Chunk> visibleChunks = new();

    public List<Vector3> vertices = new();
    public Vector3[] normals;
    public List<float> verticesElevation = new();
    public List<int> triangles = new();
    public List<Color> colors = new();


    public List<float> verticesHeat = new();
    public List<float> verticesRain = new();

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
        Vector3 playerPosition = planetScript.playerObj.transform.position;
        visibleChunks = new List<Chunk>();

        baseChunk = new Chunk("", planetScript, this, null, localUp, 1, 0, null);
        baseChunk.GenerateAllChunks();
        baseChunk.GetSubChunks(playerPosition, visibleChunks, true);
        
        foreach (Chunk chunkPart in visibleChunks)
            triangles.AddRange(chunkPart.GetChunkData(verticeSN, true));

        for (int i = 0; i < faceVertices.Count; i++)
        {
            float vertexElevationValue = 1 + planetScript.noiseFilter.Evaluate(faceVertices[i]);
            vertices.Add(vertexElevationValue * planetScript.planetRadius * faceVertices[i]);
            verticesElevation.Add(vertexElevationValue);

            if (vertexElevationValue > planetScript.maxElevation) { planetScript.maxElevation = vertexElevationValue; }
            if (vertexElevationValue < planetScript.minElevation) { planetScript.minElevation = vertexElevationValue; }

            float heatHeightFalloff = vertexElevationValue > 0 ? 1f - vertexElevationValue : 0;
            float heatFalloff = (1f - Math.Abs(faceVertices[i].y)) - heatHeightFalloff;

            float vertexHeatValue = 1 + planetScript.heatFilter.Evaluate(faceVertices[i]);
            float vertexRainValue = 1 + planetScript.rainFilter.Evaluate(faceVertices[i]);


            /*float vertexHeatValue = (((1 + planetScript.heatFilter.Evaluate(faceVertices[i])) * 1000) * (Mathf.Epsilon - Mathf.Exp(Mathf.Abs(faceVertices[i].y))) / vertexElevationValue) -
                                    (((1 + planetScript.heatFilter.Evaluate(faceVertices[i])) * 1000 - vertexElevationValue) * (Mathf.Epsilon - Mathf.Exp(Mathf.Abs(faceVertices[i].y))));
            float vertexRainValue = vertexElevationValue / (1 + planetScript.rainFilter.Evaluate(faceVertices[i]) * (Mathf.Epsilon - Mathf.Exp(Mathf.Abs(Mathf.Abs(faceVertices[i].y) - .5f))));*/


            //float vertexHeatValue = ((1 + planetScript.heatFilter.Evaluate(faceVertices[i])) * 1000 - vertexElevationValue) * (Mathf.Epsilon - Mathf.Exp(Mathf.Abs(faceVertices[i].y)));
            //float vertexRainValue = vertexElevationValue / (1 + planetScript.rainFilter.Evaluate(faceVertices[i]) * (Mathf.Exp(1) - Mathf.Exp(Mathf.Abs(Mathf.Abs(faceVertices[i].y) - .5f))));

            verticesHeat.Add(vertexHeatValue);
            verticesRain.Add(vertexRainValue);

            if (vertexHeatValue > planetScript.maxHeat && vertexElevationValue > 0) { planetScript.maxHeat = vertexHeatValue; }
            if (vertexHeatValue < planetScript.minHeat && vertexElevationValue > 0) { planetScript.minHeat = vertexHeatValue; }

            if (vertexRainValue > planetScript.maxRain && vertexElevationValue > 0) { planetScript.maxRain = vertexRainValue; }
            if (vertexRainValue < planetScript.minRain && vertexElevationValue > 0) { planetScript.minRain = vertexRainValue; }
        }

        Vector3[] normals = new Vector3[vertices.Count];

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

            triangleNormal = SurfaceNormalFromIndices(vertices, vertexIndexA, vertexIndexB, vertexIndexC);
            normals[vertexIndexA] += triangleNormal;
            normals[vertexIndexB] += triangleNormal;
            normals[vertexIndexC] += triangleNormal;
        }

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i].Normalize();
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors = colors.ToArray();
        mesh.normals = normals;
    }

    public IEnumerator UpdateChunkMesh()
    {
        Vector3 playerPosition = planetScript.playerObj.transform.position;
        /*vertices.Clear();
        colors.Clear();
        verticesElevation.Clear();
        faceVertices.Clear();
        verticeSN.Clear();
        triangles.Clear();*/
        triangles.Clear();

        Thread thread = new Thread(() => triangles = Calculate(baseChunk, playerPosition, verticeSN));
        thread.Start();
        while (thread.IsAlive)
        {
            yield return null;
        }
        thread.Join();

        /*mesh.Clear();
        mesh.vertices = vertices.ToArray();*/
        mesh.triangles = triangles.ToArray();
        /*mesh.colors = colors.ToArray();
        mesh.normals = normals;*/
    }


    private Vector3 SurfaceNormalFromIndices(List<Vector3> vertices, int indexA, int indexB, int indexC)
    {
        Vector3 pointA = vertices[indexA];
        Vector3 pointB = vertices[indexB];
        Vector3 pointC = vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    List<int> Calculate(Chunk baseChunk, Vector3 playerPosition, Hashtable verticeSN)
    {
        List<Chunk> visibleChunks = new List<Chunk>();
        List<int> triangles = new();

        baseChunk.UpdateChunk(playerPosition);
        baseChunk.GetSubChunks(playerPosition, visibleChunks, false);

        foreach (Chunk chunkPart in visibleChunks)
        {
            triangles.AddRange(chunkPart.GetChunkData(verticeSN, false));
        }

        /*for (int i = 0; i < faceVertices.Count; i++)
        {
            float vertexElevationValue = (1 + planetScript.noiseFilter.Evaluate(faceVertices[i])) * planetScript.planetRadius;
            vertices.Add(vertexElevationValue * faceVertices[i]);
            verticesElevation.Add(vertexElevationValue);
        }

        Vector3[] normals = new Vector3[vertices.Count];

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

            triangleNormal = SurfaceNormalFromIndices(vertices ,vertexIndexA, vertexIndexB, vertexIndexC);
            normals[vertexIndexA] += triangleNormal;
            normals[vertexIndexB] += triangleNormal;
            normals[vertexIndexC] += triangleNormal;
        }

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i].Normalize();
        }

        foreach (var e in verticesElevation)
        {
            float height = Mathf.InverseLerp(planetScript.minElevation, planetScript.maxElevation, e);
            colors.Add(planetScript.gradient.Evaluate(height));
        }*/
        return triangles;
    }
}