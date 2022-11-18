using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A lot of this code comes from Sebastian Lague, credit goes to him
public class NoiseFilter
{
    Noise noise = new Noise();

    public float strength = .2f;
    public int octaves = 10;
    public float baseRoughness = 1;
    public float roughness = 2f;
    public float persistance = .5f;
    public Vector3 center = new Vector3(0, 0, 0);

    public float Evaluate(Vector3 point)
    {
        float noiseValue = 0;
        float frequency = baseRoughness;
        float amplitude = 1;

        for (int i = 0; i < octaves; i++)
        {
            float v = noise.Evaluate(point * frequency + center);
            noiseValue += (v + 1) * .5f * amplitude;
            frequency *= roughness;
            amplitude *= persistance;
        }

        float elevation = 1 - Mathf.Abs(noiseValue);
        return elevation * strength;
    }
}