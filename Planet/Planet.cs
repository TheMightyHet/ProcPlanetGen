using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    public readonly float planetRadius = 100;

    public float strength = .1f;
    public int octaves = 10;
    public float baseRoughness = 1f;
    public float roughness = 2f;
    public float persistance = .5f;
    public Vector3 center = new Vector3(0, 0, 0);

    public Transform playerObj;
    [HideInInspector] public float playerDistance;

    public NoiseFilter noiseFilter;
    public float[] distanceLOD;
    [HideInInspector] 
    MeshFilter[] meshFilters;
    PlanetFace[] planetFaces;

    public float minElevation = float.MaxValue;
    public float maxElevation = float.MinValue;
    public Gradient gradient;

    private void Awake()
    {
        playerObj = GameObject.FindGameObjectWithTag("Player").transform;
        playerDistance = Vector3.Distance(transform.position, playerObj.transform.position);
    }
    private void Start()
    {
        Initialize();
        GenerateMesh();
    }

    float elapsedTime;
    readonly float timeLimit = .5f;
    private void Update()
    {
        playerDistance = Vector3.Distance(transform.position, playerObj.transform.position); 
        
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= timeLimit)
        {
            elapsedTime = 0;
            UpdateMesh();
        }
    }

    void Initialize()
    {
        noiseFilter = new NoiseFilter(strength, octaves, baseRoughness, roughness, persistance, center);
        distanceLOD = new float[] {
            Mathf.Infinity,
            planetRadius * 10f,
            planetRadius * 1.16f,
            planetRadius * .62f,
            planetRadius * .36f,
            planetRadius * .22f,
            planetRadius * .15f
        };

        if (meshFilters == null || meshFilters.Length == 0)
            meshFilters = new MeshFilter[6];

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
            }
            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = this.GetComponent<MeshRenderer>().material;

            planetFaces[i] = new PlanetFace(meshFilters[i].sharedMesh, directions[i], this, directionNames[i]);
        }
    }

    void GenerateMesh()
    {
        minElevation = float.MaxValue;
        maxElevation = float.MinValue;  

        foreach (PlanetFace face in planetFaces)
            face.CreateChunkMesh();
        ColorGen();
    }
    void UpdateMesh()
    {
        minElevation = float.MaxValue;
        maxElevation = float.MinValue; 

        foreach (PlanetFace face in planetFaces)
            face.UpdateChunkMesh();
        ColorGen();
    }

    void ColorGen()
    {
        foreach (PlanetFace face in planetFaces)
        {
            List<Color> colors = new();
            foreach (var e in face.vertElevation)
            {
                float height = Mathf.InverseLerp(minElevation, maxElevation, e);
                colors.Add(gradient.Evaluate(height));
            }
            face.mesh.colors = colors.ToArray();
        }
    }
}