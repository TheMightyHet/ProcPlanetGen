using Codice.Client.BaseCommands.Changelist;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;

public class BiomeGenerator
{
    public enum BiomeType
    {
        Desert,
        Savanna,
        TropicalRainforest,
        Grassland,
        Woodland,
        SeasonalForest,
        TemperateRainforest,
        BorealForest,
        Tundra,
        Ice
    }

    BiomeType[,] BiomeTable = new BiomeType[6, 6] {   
          //FROSTY          //COLD              //CHILLY                //COOL                          //WARM                          //HOT
        { BiomeType.Ice,    BiomeType.Tundra,   BiomeType.Grassland,    BiomeType.Desert,               BiomeType.Desert,               BiomeType.Desert },             //DRYEST
        { BiomeType.Ice,    BiomeType.Tundra,   BiomeType.Grassland,    BiomeType.Desert,               BiomeType.Desert,               BiomeType.Desert },             //DRYER
        { BiomeType.Ice,    BiomeType.Tundra,   BiomeType.Woodland,     BiomeType.Woodland,             BiomeType.Savanna,              BiomeType.Savanna },            //DRY
        { BiomeType.Ice,    BiomeType.Tundra,   BiomeType.BorealForest, BiomeType.Woodland,             BiomeType.Savanna,              BiomeType.Savanna },            //WET
        { BiomeType.Ice,    BiomeType.Tundra,   BiomeType.BorealForest, BiomeType.SeasonalForest,       BiomeType.TropicalRainforest,   BiomeType.TropicalRainforest }, //WETTER
        { BiomeType.Ice,    BiomeType.Tundra,   BiomeType.BorealForest, BiomeType.TemperateRainforest,  BiomeType.TropicalRainforest,   BiomeType.TropicalRainforest }  //WETTEST
    };

    BiomeType[,] MinecraftBiomeTable = new BiomeType[5,5] {   
        { BiomeType.Ice,        BiomeType.Ice,          BiomeType.Ice,          BiomeType.Tundra,               BiomeType.Tundra                },  //Temperature = 0
        { BiomeType.Grassland,  BiomeType.Grassland,    BiomeType.Woodland,     BiomeType.Tundra,               BiomeType.BorealForest          },  //Temperature = 1
        { BiomeType.Grassland,  BiomeType.Grassland,    BiomeType.Woodland,     BiomeType.Woodland,             BiomeType.SeasonalForest        },  //Temperature = 2
        { BiomeType.Savanna,    BiomeType.Savanna,      BiomeType.Grassland,    BiomeType.TemperateRainforest,  BiomeType.TropicalRainforest    },  //Temperature = 3
        { BiomeType.Desert,     BiomeType.Desert,       BiomeType.BorealForest, BiomeType.Desert,               BiomeType.Desert                },  //Temperature = 4
          //Humidity = 0        //Humidity = 1          //Humidity = 2          //Humidity = 3                  //Humidity = 4
    };

    private static Color Ice = Color.white;
    private static Color Desert = new Color(237 / 255f, 220 / 255f, 116 / 255f, 1);
    private static Color Savanna = new Color(177 / 255f, 209 / 255f, 110 / 255f, 1);
    private static Color TropicalRainforest = new Color(66 / 255f, 123 / 255f, 25 / 255f, 1);
    private static Color Tundra = new Color(96 / 255f, 131 / 255f, 112 / 255f, 1);
    private static Color TemperateRainforest = new Color(29 / 255f, 73 / 255f, 40 / 255f, 1);
    private static Color Grassland = new Color(164 / 255f, 225 / 255f, 99 / 255f, 1);
    private static Color SeasonalForest = new Color(73 / 255f, 100 / 255f, 35 / 255f, 1);
    private static Color BorealForest = new Color(95 / 255f, 115 / 255f, 62 / 255f, 1);
    private static Color Woodland = new Color(139 / 255f, 175 / 255f, 90 / 255f, 1);

    public Color GetVertexBiome(float vertexElevation, float minElevation, float maxElevation, Vector3 vertex, float vertexHeatValue, float vertexRainValue, float minHeatValue, float maxHeatValue, float minRainValue, float maxRainValue)
    {
        if (vertexElevation < 1) return Desert;
        float scaledSeaLevel = (1 - minElevation) / (maxElevation - minElevation);
        float scaledVertexElevation = (vertexElevation - minElevation) / (maxElevation - minElevation);

        float scaledHeatValue = (vertexHeatValue - minHeatValue) / (maxHeatValue - minHeatValue);
        float scaledRainValue = (vertexRainValue - minRainValue) / (maxRainValue - minRainValue);

        int heatIndex = (int)(scaledHeatValue * 6) % 6;
        int rainIndex = (int)(scaledRainValue * 6) % 6;

        float heatHeightFalloff = scaledVertexElevation;
        float heatFalloff = 1f - ((Mathf.Pow(Mathf.Abs(vertex.y), 2f) + heatHeightFalloff) / 2);
        int heatFalloffIndex = (int)(heatFalloff * 6) % 6;
        int minecraftHeatFalloffIndex = (int)(heatFalloff * 5) % 5;

        float rainFalloff = 1 - (scaledRainValue * (1f - Mathf.Pow(heatFalloff, 1.5f)));
        int rainFalloffIndex = (int)(rainFalloff * 6) % 6;
        int minecraftRainFalloffIndex = (int)(rainFalloff * 5) % 5;



        BiomeType vertexBiomeType = BiomeTable[rainFalloffIndex, heatFalloffIndex];
        BiomeType minecraftBiomeType = MinecraftBiomeTable[minecraftHeatFalloffIndex, minecraftRainFalloffIndex];
        return GetColor(vertexBiomeType);
    }

    Color GetColor(BiomeType vertexBiomeType)
    {
        Color returnColor = new Color();
        switch (vertexBiomeType)
        {
            case BiomeType.Desert:
                returnColor = Desert;
                break;
            case BiomeType.Savanna:
                returnColor = Savanna;
                break;
            case BiomeType.TropicalRainforest:
                returnColor = TropicalRainforest;
                break;
            case BiomeType.Grassland:
                returnColor = Grassland;
                break;
            case BiomeType.Woodland:
                returnColor = Woodland;
                break;
            case BiomeType.SeasonalForest:
                returnColor = SeasonalForest;
                break;
            case BiomeType.TemperateRainforest:
                returnColor = TemperateRainforest;
                break;
            case BiomeType.BorealForest:
                returnColor = BorealForest;
                break;
            case BiomeType.Tundra:
                returnColor = Tundra;
                break;
            case BiomeType.Ice:
                returnColor = Ice;
                break;
            default:
                break;
        }
        return returnColor;
    }
}
