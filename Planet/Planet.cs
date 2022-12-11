using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Planet : MonoBehaviour
{
    public readonly float planetRadius = 1000;

    public NoiseFilter noiseFilter = new NoiseFilter(.1f, 10, 1f, 2f, .5f, new Vector3(0, 0, 0));

    BiomeGenerator biomeGenerator = new BiomeGenerator();
    public float minHeat = float.MaxValue;
    public float maxHeat = float.MinValue;
    public NoiseFilter heatFilter = new NoiseFilter(.1f, 5, 1f, 2f, .3f, new Vector3(90, 0, 60));

    public float minRain = float.MaxValue;
    public float maxRain = float.MinValue;
    public NoiseFilter rainFilter = new NoiseFilter(.1f, 5, 1f, 2f, .1f, new Vector3(60, 0, 90));

    public Transform playerObj;
    [HideInInspector] public float playerDistance;
    [HideInInspector] public Vector3 playerPosition;

    public float[] distanceLOD;
    [HideInInspector] 
    MeshFilter[] meshFilters;
    MeshCollider[] meshColliders;
    PlanetFace[] planetFaces;

    public float minElevation = float.MaxValue;
    public float maxElevation = float.MinValue;
    public Gradient gradient;

    [SerializeField] private GameObject[] vegetationPrefabs;
    Hashtable vegetation = new();

    public bool[] faceUpdated = new bool[] { false, false, false, false, false, false };
    public int faceUpdateIndexer = 0;

    private void Awake()
    {
        playerObj = GameObject.FindGameObjectWithTag("Player").transform;
        playerPosition = playerObj.transform.position;
        playerDistance = Vector3.Distance(transform.position, playerObj.transform.position);
    }
    private void Start()
    {
        Initialize();
        GenerateMesh();
    }

    float elapsedTime;
    float elapsedTimeVegetation;
    readonly float timeLimit = 3f;
    readonly float timeLimitVegetation = 1f;
    private void Update()
    {
        playerPosition = playerObj.transform.position;
        playerDistance = Vector3.Distance(transform.position, playerObj.transform.position);

        elapsedTimeVegetation += Time.deltaTime;
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= timeLimit)
        {
            elapsedTime = 0;
            UpdateMesh();
            GenerateVegetation();
        }

        if (elapsedTimeVegetation >= timeLimitVegetation)
        {
            elapsedTimeVegetation = 0;
        }
    }

    private void OnDestroy()
    {
        DestroyVegetation();
    }

    void Initialize()
    {
        
        distanceLOD = new float[] {
            Mathf.Infinity,
            planetRadius * 10f,
            planetRadius * 1.16f,
            planetRadius * .62f,
            planetRadius * .36f,
            planetRadius * .22f,
            planetRadius * .15f
            /*planetRadius * 2f,
            planetRadius * .82f,
            planetRadius * .54f,
            planetRadius * .4f,
            planetRadius * .3f*/
        };

        if (meshFilters == null || meshFilters.Length == 0)
            meshFilters = new MeshFilter[6];

        if (meshColliders == null || meshColliders.Length == 0)
            meshColliders = new MeshCollider[6];

        planetFaces = new PlanetFace[6];
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.up, Vector3.down, Vector3.right, Vector3.left};
        string[] directionNames = { "Forward", "Back", "Up", "Down", "Right", "Left" };

        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new(directionNames[i] + "_face")
                {
                    tag = "PlanetFace"
                };
                meshObj.transform.parent = transform;
                meshObj.AddComponent<MeshRenderer>();

                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
                meshColliders[i] = meshObj.AddComponent(typeof(MeshCollider)) as MeshCollider;
            }
            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = this.GetComponent<MeshRenderer>().material;

            planetFaces[i] = new PlanetFace(meshFilters[i].sharedMesh, directions[i], this, directionNames[i]);
        }
    }

    void GenerateMesh()
    {
        int meshColliderInd = 0;
        foreach (PlanetFace face in planetFaces)
        {
            face.CreateChunkMesh();
            meshColliders[meshColliderInd++].sharedMesh = face.mesh;
        }
        GenerateColors();
        GenerateVegetation();
    }
    void UpdateMesh()
    {
        foreach (PlanetFace face in planetFaces)
            StartCoroutine(face.UpdateChunkMesh());
    }

    void GenerateColors()
    {
        foreach (PlanetFace face in planetFaces)
        {
            
            List<Color> colors = new();
            for (int i = 0; i < face.verticesElevation.Count; i++)
            {
                Color vertexColor = biomeGenerator.GetVertexBiome(face.verticesElevation[i], minElevation, maxElevation, face.faceVertices[i], face.verticesHeat[i], face.verticesRain[i], minHeat, maxHeat, minRain, maxRain);
                colors.Add(vertexColor);
            }
            face.mesh.colors = colors.ToArray();
            face.colors = colors;
        }
    }

    void GenerateVegetation()
    {
        DestroyVegetation();
        foreach (PlanetFace face in planetFaces)
        {
            int vegetationCounter = 0;
            for (int i = 0; i < face.vertices.Count(); ++i) 
            {
                if (Vector3.Distance(playerPosition, face.vertices[i]) < 100f)
                    if (!vegetation.ContainsKey(face.vertices[i]) && face.verticesElevation[i] > 1.0005f)
                    {
                        GameObject plant = Instantiate(vegetationPrefabs[UnityEngine.Random.Range(0, vegetationPrefabs.Length - 1)], transform);
                        plant.name = "Plant_" + vegetationCounter++;
                        plant.transform.position = face.vertices[i] - face.vertices[i].normalized;
                        plant.transform.up = face.faceVertices[i].normalized;
                        plant.transform.localScale = Vector3.one * UnityEngine.Random.Range(.75f, 1.25f);
                        vegetation.Add(face.vertices[i], plant);
                    }
            }
        }
    }

    void DestroyVegetation()
    {
        List<Vector3> toBeDestroyed = new List<Vector3>();
        foreach (DictionaryEntry plant in vegetation)
        {
            if (Vector3.Distance(playerPosition, (Vector3)plant.Key) > 100f)
            {
                toBeDestroyed.Add((Vector3)plant.Key);
            }
        }
        foreach (var key in toBeDestroyed)
        {
            Destroy((GameObject)vegetation[key]);
            vegetation.Remove(key);
        }
    }
}