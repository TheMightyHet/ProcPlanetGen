using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorGen
{
    ColorSettings settings;
    Texture2D texture;
    const int textureResolution = 256;

    public void UpdateSettings(ColorSettings settings)
    {
        this.settings = settings;
        if (texture == null)
        {
            texture = new Texture2D(textureResolution, 1);
        }
    }

    public void UpdateElevation(float min, float max)
    {
        settings.planetMaterial.SetVector("_Borders", new Vector4(min, max));
    }

    public void UpdateColors()
    {
        Color[] colors = new Color[textureResolution];
        for (int i = 0; i < textureResolution; i++)
        {
            colors[i] = settings.gradient.Evaluate(i / (textureResolution - 1f));
        }
        texture.SetPixels(colors);
        texture.Apply();
        settings.planetMaterial.SetTexture("_PlanetTexture", texture);
    }
}