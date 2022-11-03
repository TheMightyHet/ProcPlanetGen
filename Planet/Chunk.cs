using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlanetFace;

public class Chunk
{
    public string path;
    public Planet planetScript;
    public PlanetFace planetFace;
    public NoiseFilter noiseFilter = new();
    public Chunk parentChunk;

    public List<int> topVertices = new() { 6, 13, 20, 27, 34, 41, 48 };
    public List<int> botVertices = new() { 0, 7, 14, 21, 28, 35, 42 };
    public List<int> leftVertices = new() { 0, 1, 2, 3, 4, 5, 6 };
    public List<int> rightVertices = new() { 42, 43, 44, 45, 46, 47, 48 };

    public int chunkBaseResolution = 8;

    public Vector3 chunkPosition;
    public Chunk[] subChunks;

    public float chunkRadius;
    public int chunkLODLevel;

    public Vector3[] vertices;
    public int[] triangles;
    public Vector3[] normals;
    public Color[] colors;

    public int triangleOffset = 0;
    public int borderOffset = 0;

    public Corners corner;
    public BorderPositions borderPosition;
    public BorderPositions topLeftBordPos = PlanetFace.BorderPositions.middle, topRightBordPos = PlanetFace.BorderPositions.middle, botLeftBordPos = PlanetFace.BorderPositions.middle, botRightBordPos = PlanetFace.BorderPositions.middle;
    public int[] neighbours;
    public Chunk(Chunk parentChunk, string path, Planet planetScript, PlanetFace planetFace, Chunk[] subChunks,
                Vector3 chunkPosition, float chunkRadius, int chunkLODLevel, PlanetFace.Corners corner, PlanetFace.BorderPositions borderPosition, int[] neighbours)
    {
        this.parentChunk = parentChunk;
        this.path = path;
        this.planetScript = planetScript;
        this.planetFace = planetFace;
        this.subChunks = subChunks;
        this.chunkPosition = chunkPosition;
        this.chunkRadius = chunkRadius;
        this.chunkLODLevel = chunkLODLevel;
        this.corner = corner;
        this.borderPosition = borderPosition;
        this.neighbours = neighbours;
    }

    public void GenerateSubChunks()
    {
        if (chunkLODLevel <= planetScript.distanceLOD.Length - 1 && chunkLODLevel >= 0)
        {
            float distToPlayerObj = Vector3.Distance(chunkPosition.normalized * planetScript.planetRadius + planetScript.transform.position, planetScript.playerObj.position);

            if (distToPlayerObj <= planetScript.distanceLOD[chunkLODLevel])
            {
                subChunks = new Chunk[4];

                GetChunkBorderPosition();

                Vector3 topLeft  = chunkPosition + (.5f * chunkRadius * planetFace.axisA) - (.5f * chunkRadius * planetFace.axisB);
                Vector3 topRight = chunkPosition + (.5f * chunkRadius * planetFace.axisA) + (.5f * chunkRadius * planetFace.axisB);
                Vector3 botLeft  = chunkPosition - (.5f * chunkRadius * planetFace.axisA) - (.5f * chunkRadius * planetFace.axisB);
                Vector3 botRight = chunkPosition - (.5f * chunkRadius * planetFace.axisA) + (.5f * chunkRadius * planetFace.axisB);

                subChunks[0] = new Chunk(this, path + "0", planetScript, planetFace, new Chunk[0], topLeft,  chunkRadius * .5f, chunkLODLevel + 1, Corners.topLeft,  topLeftBordPos,  new int[4]);
                subChunks[1] = new Chunk(this, path + "1", planetScript, planetFace, new Chunk[0], topRight, chunkRadius * .5f, chunkLODLevel + 1, Corners.topRight, topRightBordPos, new int[4]);
                subChunks[2] = new Chunk(this, path + "2", planetScript, planetFace, new Chunk[0], botLeft,  chunkRadius * .5f, chunkLODLevel + 1, Corners.botLeft,  botLeftBordPos,  new int[4]);
                subChunks[3] = new Chunk(this, path + "3", planetScript, planetFace, new Chunk[0], botRight, chunkRadius * .5f, chunkLODLevel + 1, Corners.botRight, botRightBordPos, new int[4]);

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

    internal (Vector3[], Vector3[], int[], Color[]) GetSubChunkData()
    {
        //if (planetFace.dir == "Right")
        //{

        /*Vector3 rotationMatrixAttrib = new Vector3(0, 0, 0);
        if (planetFace.dir == "Forward") rotationMatrixAttrib = new Vector3(0, 0, 180);
        else if (planetFace.dir == "Back") rotationMatrixAttrib = new Vector3(0, 180, 0);
        else if (planetFace.dir == "Right") rotationMatrixAttrib = new Vector3(0, 90, 270);
        else if (planetFace.dir == "Left") rotationMatrixAttrib = new Vector3(0, 270, 270);
        else if (planetFace.dir == "Up") rotationMatrixAttrib = new Vector3(270, 0, 90);
        else if (planetFace.dir == "Down") rotationMatrixAttrib = new Vector3(90, 0, 270);
        Vector3 scaleMatrixAttrib = new Vector3(chunkRadius, chunkRadius, 1);

        Matrix4x4 transformMatrix = Matrix4x4.TRS(chunkPosition, Quaternion.Euler(rotationMatrixAttrib), scaleMatrixAttrib);

        int templateIndex = 0;
        CheckNeighbourChunkLODsSmaller();
        if (neighbours[0] == 0 && neighbours[1] == 0 && neighbours[2] == 0 && neighbours[3] == 0) templateIndex = 0;

        else if (neighbours[0] == 1 && neighbours[1] == 0 && neighbours[2] == 0 && neighbours[3] == 0) templateIndex = 1;
        else if (neighbours[0] == 0 && neighbours[1] == 0 && neighbours[2] == 1 && neighbours[3] == 0) templateIndex = 2;
        else if (neighbours[0] == 0 && neighbours[1] == 1 && neighbours[2] == 0 && neighbours[3] == 0) templateIndex = 3;
        else if (neighbours[0] == 0 && neighbours[1] == 0 && neighbours[2] == 0 && neighbours[3] == 1) templateIndex = 4;

        else if (neighbours[0] == 1 && neighbours[1] == 1 && neighbours[2] == 0 && neighbours[3] == 0) templateIndex = 5;
        else if (neighbours[0] == 1 && neighbours[1] == 0 && neighbours[2] == 0 && neighbours[3] == 1) templateIndex = 6;
        else if (neighbours[0] == 0 && neighbours[1] == 1 && neighbours[2] == 1 && neighbours[3] == 0) templateIndex = 7;
        else if (neighbours[0] == 0 && neighbours[1] == 0 && neighbours[2] == 1 && neighbours[3] == 1) templateIndex = 8;

        vertices = new Vector3[ChunkTemplate.templateVertices[templateIndex].Length];
        normals = new Vector3[vertices.Length];

        for (int i = 0; i < vertices.Length; ++i)
        {
            Vector3 pointPosOnCube = transformMatrix.MultiplyPoint(ChunkTemplate.templateVertices[0][i]);
            Vector3 pointPosOnSphere = pointPosOnCube.normalized;
            vertices[i] = pointPosOnSphere * planetScript.planetRadius;
            normals[i] = pointPosOnSphere;
        }
        triangles = ChunkTemplate.templateTriangles[templateIndex];*/


        triangleOffset = 0;
        borderOffset = 0;

        CheckNeighbourChunkLODsSmaller();

        int topVerticesCount = neighbours[0] == 1 ? chunkBaseResolution / 2 + 1 : chunkBaseResolution + 1;
        int topTrianglesCount = neighbours[0] == 1 ? (chunkBaseResolution / 2) * 3 + (chunkBaseResolution / 2 - 1) * 6 : (chunkBaseResolution / 2) * 6 + (chunkBaseResolution / 2 - 1) * 6;

        int leftVerticesCount = neighbours[1] == 1 ? chunkBaseResolution / 2 + 1 : chunkBaseResolution + 1;
        int leftTrianglesCount = neighbours[1] == 1 ? (chunkBaseResolution / 2) * 3 + (chunkBaseResolution / 2 - 1) * 6 : (chunkBaseResolution / 2) * 6 + (chunkBaseResolution / 2 - 1) * 6;

        int botVerticesCount = neighbours[2] == 1 ? chunkBaseResolution / 2 + 1 : chunkBaseResolution + 1;
        int botTrianglesCount = neighbours[2] == 1 ? (chunkBaseResolution / 2) * 3 + (chunkBaseResolution / 2 - 1) * 6 : (chunkBaseResolution / 2) * 6 + (chunkBaseResolution / 2 - 1) * 6;

        int righttVerticesCount = neighbours[3] == 1 ? chunkBaseResolution / 2 + 1 : chunkBaseResolution + 1;
        int rightTrianglesCount = neighbours[3] == 1 ? (chunkBaseResolution / 2) * 3 + (chunkBaseResolution / 2 - 1) * 6 : (chunkBaseResolution / 2) * 6 + (chunkBaseResolution / 2 - 1) * 6;

        vertices = new Vector3[(chunkBaseResolution - 1) * (chunkBaseResolution - 1) + topVerticesCount + botVerticesCount + leftVerticesCount + righttVerticesCount];
        triangles = new int[(chunkBaseResolution - 2) * (chunkBaseResolution - 2) * 6 + topTrianglesCount + botTrianglesCount + leftTrianglesCount + rightTrianglesCount];
        normals = new Vector3[vertices.Length];
        colors = new Color[vertices.Length];

        CalculateChunkMiddle();

        if (neighbours[0] == 1) CalculateChunkBorderEdgeFan(topVertices, chunkBaseResolution, 0); else CalculateChunkBorder(topVertices, chunkBaseResolution, 0);
        if (neighbours[1] == 1) CalculateChunkBorderEdgeFan(leftVertices, 0, 1); else CalculateChunkBorder(leftVertices, 0, 1);
        if (neighbours[2] == 1) CalculateChunkBorderEdgeFan(botVertices, 0, 2); else CalculateChunkBorder(botVertices, 0, 2);
        if (neighbours[3] == 1) CalculateChunkBorderEdgeFan(rightVertices, chunkBaseResolution, 3); else CalculateChunkBorder(rightVertices, chunkBaseResolution, 3);


        for (int i = 0; i < vertices.Length; ++i)
            colors[i] = Color.Lerp(Color.red, Color.green, UnityEngine.Random.value);//noiseFilter.Evaluate(vertices[i].normalized * planetScript.planetRadius));


        /*int triangleCount = triangles.Length / 3;

        Debug.Log(triangleCount + ", " + vertices.Length + ", " + triangles.Length);

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
        }*/



        return (vertices, normals, triangles, colors); //GetTriangles());
    }

    public void CalculateChunkMiddle()
    {

        for (int y = 0; y < chunkBaseResolution - 1; y++)
        {
            for (int x = 0; x < chunkBaseResolution - 1; x++)
            {
                int i = x + y * (chunkBaseResolution - 1);
                Vector2 percent = new Vector2(x + 1, y + 1) / chunkBaseResolution;
                Vector3 pointPosOnCube = chunkPosition + ((percent.x - .5f) * 2 * planetFace.axisA + (percent.y - .5f) * 2 * planetFace.axisB) * chunkRadius;

                Vector3 pointPosOnSphere = pointPosOnCube.normalized;
                float elevation = planetScript.noiseFilter.Evaluate(pointPosOnSphere);
                vertices[i] = pointPosOnSphere * (1 + elevation) * planetScript.planetRadius; //pointPosOnSphere * planetScript.planetRadius + pointPosOnSphere * noiseFilter.Evaluate(pointPosOnSphere) * 50;
                normals[i] = pointPosOnSphere;

                if (x != chunkBaseResolution - 2 && y != chunkBaseResolution - 2)
                {
                    triangles[triangleOffset] = i + planetFace.offset;
                    triangles[triangleOffset + 1] = i + (chunkBaseResolution - 1) + 1 + planetFace.offset;
                    triangles[triangleOffset + 2] = i + (chunkBaseResolution - 1) + planetFace.offset;

                    triangles[triangleOffset + 3] = i + planetFace.offset;
                    triangles[triangleOffset + 4] = i + 1 + planetFace.offset;
                    triangles[triangleOffset + 5] = i + (chunkBaseResolution - 1) + 1 + planetFace.offset;

                    triangleOffset += 6;
                }
            }
        }
    }

    public void CalculateChunkBorderEdgeFan(List<int> borderVertices, int border, int sideWays)
    {
        // BorderWithEdgeFan
        for (int y = 0; y < chunkBaseResolution / 2 + 1; y++)
        {
            int i = (chunkBaseResolution - 1) * (chunkBaseResolution - 1) + y + borderOffset;
            Vector2 percent = sideWays % 2 == 0 ? new Vector2(border, y * 2) / chunkBaseResolution : new Vector2(y * 2, border) / chunkBaseResolution;

            Vector3 pointPosOnCube = chunkPosition + ((percent.x - .5f) * 2 * planetFace.axisA + (percent.y - .5f) * 2 * planetFace.axisB) * chunkRadius;

            Vector3 pointPosOnSphere = pointPosOnCube.normalized;
            float elevation = planetScript.noiseFilter.Evaluate(pointPosOnSphere);
            vertices[i] = pointPosOnSphere * (1 + elevation) * planetScript.planetRadius; //pointPosOnSphere * planetScript.planetRadius + pointPosOnSphere * noiseFilter.Evaluate(pointPosOnSphere) * 50;
            normals[i] = pointPosOnSphere;

            if (sideWays == 0 || sideWays == 1)
                DrawEdgeFanTopAnd(borderVertices, i, y);
            else if (sideWays == 2 || sideWays == 3)
            {
                DrawEdgeFanBotAnd(borderVertices, i, y); 
            }
        }
        borderOffset += chunkBaseResolution / 2 + 1;
    }

    public void DrawEdgeFanTopAnd(List<int> borderVertices, int i, int y)
    {

        if (y != chunkBaseResolution / 2)
        {
            triangles[triangleOffset] = i + planetFace.offset;
            triangles[triangleOffset + 1] = i + 1 + planetFace.offset;
            triangles[triangleOffset + 2] = borderVertices[y * 2] + planetFace.offset;

            triangleOffset += 3;
        }

        if (y > 0 && y < chunkBaseResolution / 2)
        {
            triangles[triangleOffset] = borderVertices[y * 2 - 2] + planetFace.offset;
            triangles[triangleOffset + 1] = i + planetFace.offset;
            triangles[triangleOffset + 2] = borderVertices[y * 2 - 1] + planetFace.offset;

            triangles[triangleOffset + 3] = borderVertices[y * 2 - 1] + planetFace.offset;
            triangles[triangleOffset + 4] = i + planetFace.offset;
            triangles[triangleOffset + 5] = borderVertices[y * 2] + planetFace.offset;

            triangleOffset += 6;
        }
    }

    public void DrawEdgeFanBotAnd(List<int> borderVertices, int i, int y)
    {

        if (y != chunkBaseResolution / 2)
        {
            triangles[triangleOffset] = i + planetFace.offset;
            triangles[triangleOffset + 1] = borderVertices[y * 2] + planetFace.offset;
            triangles[triangleOffset + 2] = i + 1 + planetFace.offset;

            triangleOffset += 3;
        }

        if (y > 0 && y < chunkBaseResolution / 2)
        {
            triangles[triangleOffset] = i + planetFace.offset; 
            triangles[triangleOffset + 1] = borderVertices[y * 2 - 2] + planetFace.offset;
            triangles[triangleOffset + 2] = borderVertices[y * 2 - 1] + planetFace.offset;

            triangles[triangleOffset + 3] = i + planetFace.offset;
            triangles[triangleOffset + 4] = borderVertices[y * 2 - 1] + planetFace.offset;
            triangles[triangleOffset + 5] = borderVertices[y * 2] + planetFace.offset;

            triangleOffset += 6;
        }
    }

    public void CalculateChunkBorder(List<int> borderVertices, int border, int sideWays)
    { 
        // BorderWithoutEdgeFans
        for (int y = 0; y < chunkBaseResolution + 1; y++)
        {
            int i = (chunkBaseResolution - 1) * (chunkBaseResolution - 1) + y + borderOffset;
            Vector2 percent = sideWays % 2 == 0 ? new Vector2(border, y) / chunkBaseResolution : new Vector2(y, border) / chunkBaseResolution;
            Vector3 pointPosOnCube = chunkPosition + ((percent.x - .5f) * 2 * planetFace.axisA + (percent.y - .5f) * 2 * planetFace.axisB) * chunkRadius;

            Vector3 pointPosOnSphere = pointPosOnCube.normalized;
            float elevation = planetScript.noiseFilter.Evaluate(pointPosOnSphere);
            vertices[i] = pointPosOnSphere * (1 + elevation) * planetScript.planetRadius; //pointPosOnSphere * planetScript.planetRadius + pointPosOnSphere * noiseFilter.Evaluate(pointPosOnSphere) * 50;
            normals[i] = pointPosOnSphere;

            if (sideWays == 0) // || sideWays == 1)
                DrawSimpleBorderTopAnd(borderVertices, i, y);
            else 
            if (sideWays == 1)
                DrawSimpleBorderLeft(borderVertices, i, y);
            else 
            if (sideWays == 2) // || sideWays == 3)
                DrawSimpleBorderBotAnd(borderVertices, i, y);
            else
            if (sideWays == 3)
                DrawSimpleBorderRight(borderVertices, i, y);
        }
        borderOffset += chunkBaseResolution + 1;
    }

    public void DrawSimpleBorderTopAnd(List<int> borderVertices, int i, int y)
    { 

        if (y > 0 && y < chunkBaseResolution - 1)
        {
            triangles[triangleOffset] = borderVertices[y - 1] + planetFace.offset;
            triangles[triangleOffset + 1] = i + planetFace.offset;
            triangles[triangleOffset + 2] = i + 1 + planetFace.offset;

            triangles[triangleOffset + 3] = borderVertices[y - 1] + planetFace.offset;
            triangles[triangleOffset + 4] = i + 1 + planetFace.offset;
            triangles[triangleOffset + 5] = borderVertices[y] + planetFace.offset;

            triangleOffset += 6;
        }
        if (y == 0)
        {
            triangles[triangleOffset] = borderVertices[0] + planetFace.offset;
            triangles[triangleOffset + 1] = i + planetFace.offset;
            triangles[triangleOffset + 2] = i + 1 + planetFace.offset;

            triangleOffset += 3;
        }
        if (y == chunkBaseResolution - 1)
        {
            triangles[triangleOffset] = borderVertices[^1] + planetFace.offset;
            triangles[triangleOffset + 1] = i + planetFace.offset;
            triangles[triangleOffset + 2] = i + 1 + planetFace.offset;

            triangleOffset += 3;
        }
    }

    public void DrawSimpleBorderLeft(List<int> borderVertices, int i, int y)
    {

        if (y > 0 && y < chunkBaseResolution - 1)
        {
            triangles[triangleOffset] = i + planetFace.offset;
            triangles[triangleOffset + 1] = borderVertices[y] + planetFace.offset;
            triangles[triangleOffset + 2] = borderVertices[y - 1] + planetFace.offset;

            triangles[triangleOffset + 3] = i + planetFace.offset;
            triangles[triangleOffset + 4] = i + 1 + planetFace.offset;
            triangles[triangleOffset + 5] = borderVertices[y] + planetFace.offset;

            triangleOffset += 6;
        }
        if (y == 0)
        {
            triangles[triangleOffset] = borderVertices[0] + planetFace.offset;
            triangles[triangleOffset + 1] = i + planetFace.offset;
            triangles[triangleOffset + 2] = i + 1 + planetFace.offset;

            triangleOffset += 3;
        }
        if (y == chunkBaseResolution - 1)
        {
            triangles[triangleOffset] = borderVertices[^1] + planetFace.offset;
            triangles[triangleOffset + 1] = i + planetFace.offset;
            triangles[triangleOffset + 2] = i + 1 + planetFace.offset;

            triangleOffset += 3;
        }
    }

    public void DrawSimpleBorderBotAnd(List<int> borderVertices, int i, int y)
    {

        if (y > 0 && y < chunkBaseResolution - 1)
        {
            triangles[triangleOffset] = i + planetFace.offset;
            triangles[triangleOffset + 1] = borderVertices[y - 1] + planetFace.offset;
            triangles[triangleOffset + 2] = borderVertices[y] + planetFace.offset; 

            triangles[triangleOffset + 3] = i + planetFace.offset;
            triangles[triangleOffset + 4] = borderVertices[y] + planetFace.offset; 
            triangles[triangleOffset + 5] = i + 1 + planetFace.offset; 

            triangleOffset += 6;
        }
        if (y == 0)
        {
            triangles[triangleOffset] = i + planetFace.offset;
            triangles[triangleOffset + 1] = borderVertices[0] + planetFace.offset;
            triangles[triangleOffset + 2] = i + 1 + planetFace.offset;

            triangleOffset += 3;
        }
        if (y == chunkBaseResolution - 1)
        {
            triangles[triangleOffset] = i + planetFace.offset; 
            triangles[triangleOffset + 1] = borderVertices[^1] + planetFace.offset;
            triangles[triangleOffset + 2] = i + 1 + planetFace.offset;

            triangleOffset += 3;
        }
    }

    public void DrawSimpleBorderRight(List<int> borderVertices, int i, int y)
    {

        if (y > 0 && y < chunkBaseResolution - 1)
        {
            triangles[triangleOffset] = i + planetFace.offset;
            triangles[triangleOffset + 1] = borderVertices[y - 1] + planetFace.offset;
            triangles[triangleOffset + 2] = i + 1 + planetFace.offset;

            triangles[triangleOffset + 3] = borderVertices[y - 1] + planetFace.offset;
            triangles[triangleOffset + 4] = borderVertices[y] + planetFace.offset;
            triangles[triangleOffset + 5] = i + 1 + planetFace.offset;

            triangleOffset += 6;
        }
        if (y == 0)
        {
            triangles[triangleOffset] = i + planetFace.offset;
            triangles[triangleOffset + 1] = borderVertices[0] + planetFace.offset;
            triangles[triangleOffset + 2] = i + 1 + planetFace.offset;

            triangleOffset += 3;
        }
        if (y == chunkBaseResolution - 1)
        {
            triangles[triangleOffset] = i + planetFace.offset;
            triangles[triangleOffset + 1] = borderVertices[^1] + planetFace.offset;
            triangles[triangleOffset + 2] = i + 1 + planetFace.offset;

            triangleOffset += 3;
        }
    }

    internal int[] GetTriangles()
    {
        int[] returnTriangles = new int[triangles.Length];

        for (int i = 0; i < returnTriangles.Length; ++i)
            returnTriangles[i] = triangles[i] + planetFace.offset;

        return returnTriangles;
    }

    public Vector3 GetSurfaceNormal(int a, int b, int c)
    {
        Vector3 pA = vertices[a];
        Vector3 pB = vertices[b];
        Vector3 pC = vertices[c];
        
        Vector3 sideAB = pB - pA;
        Vector3 sideAC = pC - pB;

        return Vector3.Cross(sideAB, sideAC).normalized;
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
            if (Vector3.Angle(chunkPosition, planetScript.playerObj.position) < 70)
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
                    neighbours[0] = CheckIfNeighbourLODSmallerPathV(0); // Top  //borderPosition != BorderPositions.topLeftCorner && borderPosition != BorderPositions.topRightCorner && borderPosition != BorderPositions.topMidBorder ? CheckIfNeighbourLODSmaller(0) : 0; // <- 0 if on faceBorder
                    neighbours[1] = CheckIfNeighbourLODSmallerPathV(1); // Left //borderPosition != BorderPositions.topLeftCorner && borderPosition != BorderPositions.botLeftCorner && borderPosition != BorderPositions.leftMidBorder ? CheckIfNeighbourLODSmaller(1) : 0; // <- 0 if on faceBorder
                    neighbours[2] = 0; // Bot
                    neighbours[3] = 0; // Right
                    break;
                }
            case '1': // Corners.topRight:
                {
                    neighbours[0] = CheckIfNeighbourLODSmallerPathV(0); // Top //borderPosition != BorderPositions.topLeftCorner && borderPosition != BorderPositions.topRightCorner && borderPosition != BorderPositions.topMidBorder ? CheckIfNeighbourLODSmaller(0) : 0; // <- 0 if on faceBorder
                    neighbours[1] = 0; // Left
                    neighbours[2] = 0; // Bot
                    neighbours[3] = CheckIfNeighbourLODSmallerPathV(3); // Right //borderPosition != BorderPositions.topRightCorner && borderPosition != BorderPositions.botRightCorner && borderPosition != BorderPositions.rightMidBorder ? CheckIfNeighbourLODSmaller(3) : 0; // <- 0 if on faceBorder
                    break;
                }
            case '2': // Corners.botLeft:
                {
                    neighbours[0] = 0; // Top
                    neighbours[1] = CheckIfNeighbourLODSmallerPathV(1); // Left //borderPosition != BorderPositions.topLeftCorner && borderPosition != BorderPositions.botLeftCorner && borderPosition != BorderPositions.leftMidBorder ? CheckIfNeighbourLODSmaller(1) : 0; // <- 0 if on faceBorder
                    neighbours[2] = CheckIfNeighbourLODSmallerPathV(2); // Bot  //borderPosition != BorderPositions.botLeftCorner && borderPosition != BorderPositions.botRightCorner && borderPosition != BorderPositions.botMidBorder ? CheckIfNeighbourLODSmaller(2) : 0; // <- 0 if faceBorder
                    neighbours[3] = 0; // Right
                    break;
                }
            case '3': // Corners.botRight:
                {
                    neighbours[0] = 0; // Top
                    neighbours[1] = 0; // Left
                    neighbours[2] = CheckIfNeighbourLODSmallerPathV(2); // Bot //borderPosition != BorderPositions.botLeftCorner && borderPosition != BorderPositions.botRightCorner && borderPosition != BorderPositions.botMidBorder ? CheckIfNeighbourLODSmaller(2) : 0; // <- 0 if faceBorder
                    neighbours[3] = CheckIfNeighbourLODSmallerPathV(3); // Right //borderPosition != BorderPositions.topRightCorner && borderPosition != BorderPositions.botRightCorner && borderPosition != BorderPositions.rightMidBorder ? CheckIfNeighbourLODSmaller(3) : 0; // <- 0 if on faceBorder

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






    //Not used - only to show at consultation

    public void GetChunkBorderPosition()
    {
        switch (borderPosition)
        {
            case BorderPositions.root:
                {
                    topLeftBordPos = BorderPositions.topLeftCorner;
                    topRightBordPos = BorderPositions.topRightCorner;
                    botLeftBordPos = BorderPositions.botLeftCorner;
                    botRightBordPos = BorderPositions.botRightCorner;
                    break;
                }
            case BorderPositions.topLeftCorner:
                {
                    topLeftBordPos = BorderPositions.topLeftCorner;
                    topRightBordPos = BorderPositions.topMidBorder;
                    botLeftBordPos = BorderPositions.leftMidBorder;
                    break;
                }
            case BorderPositions.topRightCorner:
                {
                    topLeftBordPos = BorderPositions.topMidBorder;
                    topRightBordPos = BorderPositions.topRightCorner;
                    botRightBordPos = BorderPositions.rightMidBorder;
                    break;
                }
            case BorderPositions.botLeftCorner:
                {
                    topLeftBordPos = BorderPositions.leftMidBorder;
                    botLeftBordPos = BorderPositions.botLeftCorner;
                    botRightBordPos = BorderPositions.botMidBorder;
                    break;
                }
            case BorderPositions.botRightCorner:
                {
                    topRightBordPos = BorderPositions.rightMidBorder;
                    botLeftBordPos = BorderPositions.botMidBorder;
                    botRightBordPos = BorderPositions.botRightCorner;
                    break;
                }
            case BorderPositions.topMidBorder:
                {
                    topLeftBordPos = BorderPositions.topMidBorder;
                    topRightBordPos = BorderPositions.topMidBorder;
                    break;
                }
            case BorderPositions.leftMidBorder:
                {
                    topLeftBordPos = BorderPositions.leftMidBorder;
                    botLeftBordPos = BorderPositions.leftMidBorder;
                    break;
                }
            case BorderPositions.rightMidBorder:
                {
                    topRightBordPos = BorderPositions.rightMidBorder;
                    botRightBordPos = BorderPositions.rightMidBorder;
                    break;
                }
            case BorderPositions.botMidBorder:
                {
                    botLeftBordPos = BorderPositions.botMidBorder;
                    botRightBordPos = BorderPositions.botMidBorder;
                    break;
                }
            default:
                break;
        }
    }

    public int CheckIfNeighbourLODSmaller(int dir)
    {
        List<Corners> neighbourPath = InvertedPath(dir, new List<Corners>());
        Chunk neighbour = planetFace.baseChunk;
        neighbourPath.RemoveAt(0);

        while (neighbourPath.Count > 0)
        {
            if (neighbour.subChunks.Length > 0)
            {
                if (neighbourPath[0] == Corners.topLeft) neighbour = neighbour.subChunks[0];
                else if (neighbourPath[0] == Corners.topRight) neighbour = neighbour.subChunks[1];
                else if (neighbourPath[0] == Corners.botLeft) neighbour = neighbour.subChunks[2];
                else if (neighbourPath[0] == Corners.botRight) neighbour = neighbour.subChunks[3];
                neighbourPath.RemoveAt(0);
            }
            else
                break;
        }
        if (neighbour.chunkLODLevel < chunkLODLevel) return 1;
        return 0;
    }
    public List<Corners> InvertedPath(int dir, List<Corners> cornerPath)
    {
        List<Corners> tmpCornerPath = cornerPath;
        Corners tmpCorner;

        if (corner != Corners.middle) // Stops at baseChunk
            if (HasSiblingTowards(dir))
            {
                tmpCorner = InvertDirection(dir, corner);
                tmpCornerPath.Insert(0, tmpCorner);

                parentChunk.InvertedPath(-1, tmpCornerPath); // In HasSiblingTowards: -1 % 2 == -1 => Adds parentChunk base corner instead of the inverted one
            }
            else
            {
                tmpCorner = InvertDirection(dir, corner);
                tmpCornerPath.Insert(0, tmpCorner);

                parentChunk.InvertedPath(dir, tmpCornerPath); // Call on parentChunk
            }
        else
            tmpCornerPath.Insert(0, Corners.middle);

        return tmpCornerPath; // Return path with inversions
    }
    public Corners InvertDirection(int dir, Corners corner)
    {

        if (dir % 2 == 1) // HorizontalSide
        {
            if (corner == Corners.topLeft) return Corners.topRight; // topLeft  -> topRight
            else if (corner == Corners.topRight) return Corners.topLeft;  // topRight -> topLeft
            else if (corner == Corners.botLeft) return Corners.botRight; // botLeft  -> botRight
            else if (corner == Corners.botRight) return Corners.botLeft;  // botRight -> botLeft
        }
        else
        if (dir % 2 == 0) // VerticalSide
        {
            if (corner == Corners.topLeft) return Corners.botLeft;  // topLeft -> botLeft
            else if (corner == Corners.topRight) return Corners.botRight; // topRight -> botRight
            else if (corner == Corners.botLeft) return Corners.topLeft;  // botLeft -> topLeft
            else if (corner == Corners.botRight) return Corners.topRight; // botRight -> topRight
        }
        return corner;
    }
    public bool HasSiblingTowards(int dir) // True if has sibling towards a direction (Top = 0, Left = 1, Bot = 2, Right = 3)
    {
        if (dir == 0) // TopSide
        {
            if (corner == Corners.botLeft || corner == Corners.botRight) // BotSides
                return true;
            else if (corner == Corners.topLeft || corner == Corners.topRight) // TopSides
                return false;
        }
        else if (dir == 1) // LeftSide
        {
            if (corner == Corners.topRight || corner == Corners.botRight) // RightSides
                return true;
            else if (corner == Corners.topLeft || corner == Corners.botLeft) // LeftSides
                return false;
        }
        else if (dir == 2) // BotSide
        {
            if (corner == Corners.topLeft || corner == Corners.topRight) // TopSides
                return true;
            else if (corner == Corners.botLeft || corner == Corners.botRight) // BotSides
                return false;
        }
        else if (dir == 3) // RightSide
        {
            if (corner == Corners.topLeft || corner == Corners.botLeft) // LeftSides
                return true;
            else if (corner == Corners.topRight || corner == Corners.botRight) // RightSides
                return false;
        }
        return false;
    }
}
