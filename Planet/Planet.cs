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

    public float strength = .1f;
    public int octaves = 10;
    public float baseRoughness = 1f;
    public float roughness = 2f;
    public float persistance = .5f;
    public Vector3 center = new Vector3(0, 0, 0);

    public NoiseFilter noiseFilter;

    public float[] distanceLOD;
    public float minElevation = float.MaxValue;
    public float maxElevation = float.MinValue;
    public ColorSettings colorSettings;
    ColorGen colorGen = new();
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
        colorGen.UpdateSettings(colorSettings);
        distanceLOD = new float[] {
            Mathf.Infinity,
            planetRadius * 10f,
            planetRadius * 7f,
            planetRadius * .4f,
            planetRadius * .25f,
            planetRadius * .18f,
            planetRadius * .1f
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
            //meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = colorSettings.planetMaterial;
            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = this.GetComponent<MeshRenderer>().material;

            planetFaces[i] = new PlanetFace(meshFilters[i].sharedMesh, directions[i], this, directionNames[i]);
        }
    }

    void GenerateMesh()
    {
        minElevation = float.MaxValue;
        maxElevation = float.MinValue;  
        noiseFilter = new NoiseFilter(strength, octaves, baseRoughness, roughness, persistance, center);

        foreach (PlanetFace face in planetFaces)
        {
            face.CreateChunkMesh();
        }
        /*colorGen.UpdateElevation(minElevation, maxElevation);
        GenerateColors();*/
        ColorGen();
    }
    void UpdateMesh()
    {
        minElevation = float.MaxValue;
        maxElevation = float.MinValue; 
        noiseFilter = new NoiseFilter(strength, octaves, baseRoughness, roughness, persistance, center);

        foreach (PlanetFace face in planetFaces)
        {
            face.UpdateChunkMesh();
            //face.CreateChunkMesh();
        }
        /*colorGen.UpdateElevation(minElevation, maxElevation);
        GenerateColors();*/
        ColorGen();
    }

    void GenerateColors()
    {
        colorGen.UpdateColors();
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

    private void OnDrawGizmos()
    { }
}