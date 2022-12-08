using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseFilter
{
    Noise noise;

    public float strength;
    public int octaves;
    public float baseRoughness;
    public float roughness;
    public float persistance;
    public Vector3 center;

    public NoiseFilter(float strength, int octaves, float baseRoughness, float roughness, float persistance, Vector3 center)
    {
        noise = new();

        this.strength = strength;
        this.octaves = octaves;
        this.baseRoughness = baseRoughness;
        this.roughness = roughness;
        this.persistance = persistance;
        this.center = center;
    }

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