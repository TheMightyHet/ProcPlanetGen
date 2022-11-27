using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

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
    public int hashCounter = 0;

    public Chunk baseChunk;
    public List<Chunk> displayedChunk = new();

    public List<Vector3> vertices = new();
    public List<float> vertElevation = new();
    public List<int> triangles = new();
    public List<Color> colors = new();


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
        Vector3 playerPos = planetScript.playerObj.transform.position;
        displayedChunk = new List<Chunk>();

        baseChunk = new Chunk("0", planetScript, this, null, localUp, 1, 0, null);
        baseChunk.GenerateSubChunks(playerPos);
        baseChunk.GetSubChunks(playerPos);
        
        foreach (Chunk chunkPart in displayedChunk)
            triangles.AddRange(chunkPart.GetChunkData());

        for (int i = 0; i < faceVertices.Count; i++)
        {
            float elevation = (1 + planetScript.noiseFilter.Evaluate(faceVertices[i])) * planetScript.planetRadius;
            vertices.Add(elevation * faceVertices[i]);
            vertElevation.Add(elevation);

            if (elevation > planetScript.maxElevation) { planetScript.maxElevation = elevation; }
            if (elevation < planetScript.minElevation) { planetScript.minElevation = elevation; }
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
    }

    public async void UpdateChunkMesh()
    {
        Vector3 playerPos = planetScript.playerObj.transform.position;
        vertices.Clear();
        triangles.Clear();
        colors.Clear();
        vertElevation.Clear();
        faceVertices.Clear();
        verticeSN.Clear();
        triangles.Clear();
        hashCounter = 0;
        displayedChunk = new List<Chunk>();

        baseChunk.UpdateChunk(playerPos);
        baseChunk.GetSubChunks(playerPos);

        foreach (Chunk chunkPart in displayedChunk)
            triangles.AddRange(chunkPart.GetChunkData());

        for (int i = 0; i < faceVertices.Count; i++)
        {
            float elevation = (1 + planetScript.noiseFilter.Evaluate(faceVertices[i])) * planetScript.planetRadius;
            vertices.Add(elevation * faceVertices[i]);
            vertElevation.Add(elevation);

            if (elevation > planetScript.maxElevation) { planetScript.maxElevation = elevation; }
            if (elevation < planetScript.minElevation) { planetScript.minElevation = elevation; }
        }

        await Task.Yield();

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
    }
}