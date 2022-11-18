using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    public readonly float planetRadius = 1000;

    [SerializeField,HideInInspector] 
    MeshFilter[] meshFilters;
    PlanetFace[] planetFaces;

    public Transform playerObj;
    [HideInInspector] public float playerDistance;

    public NoiseFilter noiseFilter = new();

    public float[] distanceLOD; 

    public float minElevation = float.MaxValue;
    public float maxElevation = float.MinValue;
    public ColorSettings colorSettings;
    ColorGen colorGen = new();

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
        colorGen.UpdateSettings(colorSettings);
        distanceLOD = new float[] {
            Mathf.Infinity,
            planetRadius * 3f,
            planetRadius * 1.2f,
            planetRadius * .5f,
            planetRadius * .21f,
            planetRadius * .1f,
            planetRadius * .04f
        };


        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }

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
            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = colorSettings.planetMaterial;

            planetFaces[i] = new PlanetFace(meshFilters[i].sharedMesh, directions[i], this, directionNames[i]);
        }
    }

    void GenerateMesh()
    {
        foreach (PlanetFace face in planetFaces)
        {
            face.CreateChunkMesh();
        }
        colorGen.UpdateElevation(minElevation * planetRadius, maxElevation * planetRadius);
        GenerateColors();
    }
    void UpdateMesh()
    {
        foreach (PlanetFace face in planetFaces)
        {
            face.UpdateChunkMesh();
            //face.CreateChunkMesh();
        }
        colorGen.UpdateElevation(minElevation * planetRadius, maxElevation * planetRadius);
        GenerateColors();
    }

    void GenerateColors()
    {
        colorGen.UpdateColors();
    }
}