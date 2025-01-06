﻿using UnityEngine;

public class ColourGenerator
{
    // Instance of ColourSettings containing configuration data
    ColourSettings settings;

    Texture2D texture;

    const int textureResolution = 50;

    // Noise filter for biome generation based on noise
    INoiseFilter biomeNoiseFilter;

    // Calculates the biome index based on the point on the unit sphere
    // This function computes how much of the point falls into each biome based on the height and noise.
    public float BiomePercentFromPoint(Vector3 pointOnUnitSphere)
    {
        // Normalize the Y position of the point to [0, 1]
        float heightPercent = (pointOnUnitSphere.y + 1) / 2f;

        // Adjust the heightPercent with biome noise, factoring in offset and strength
        heightPercent += (biomeNoiseFilter.Evaluate(pointOnUnitSphere) - settings.biomeColourSettings.noiseOffset) * settings.biomeColourSettings.noiseStrength;

        // Initialize biome index
        float biomeIndex = 0;

        // Number of biomes available in the settings
        int numBiomes = settings.biomeColourSettings.biome.Length;

        // Blend range for smooth transitions between biomes
        float blendRange = settings.biomeColourSettings.blendAmount / 2f + .001f;

        // Loop through each biome to determine the biome index
        for (int i = 0; i < numBiomes; i++)
        {
            // Calculate the distance from the start height of the current biome
            float dst = heightPercent - settings.biomeColourSettings.biome[i].startHeight;

            // Calculate the weight based on the distance to the biome's start height
            float weight = Mathf.InverseLerp(-blendRange, blendRange, dst);

            // Adjust biome index with the calculated weight
            biomeIndex *= (1 - weight);
            biomeIndex += i * weight;
        }

        // Return the normalized biome index
        return biomeIndex / Mathf.Max(1, numBiomes - 1);
    }

    // Updates the settings for the colour generator, creating a new texture if necessary
    public void UpdateSettings(ColourSettings settings)
    {
        this.settings = settings;

        // Create a new texture if it does not exist or its height does not match the number of biomes
        if (texture == null || texture.height != settings.biomeColourSettings.biome.Length)
        {
            texture = new Texture2D(textureResolution * 2, settings.biomeColourSettings.biome.Length, TextureFormat.RGBA32, false);
        }

        // Create a new biome noise filter based on the settings
        biomeNoiseFilter = NoiseFilterFactory.MakeNoiseFilter(settings.biomeColourSettings.noise);
    }

    // Updates the colors for the texture based on biome settings and applies it to the material
    public void UpdateColours()
    {
        // Create an array to store colors for each pixel in the texture
        Color[] colours = new Color[texture.width * texture.height];
        int colourIndex = 0;

        // Loop through each biome to apply its gradient and tint
        foreach (var biome in settings.biomeColourSettings.biome)
        {
            // Generate colors for the texture based on biome gradient and tint
            for (int i = 0; i < textureResolution * 2; i++)
            {
                Color gradientCol;

                // Determine whether the current pixel corresponds to the ocean or biome
                if (i < textureResolution)
                {
                    // Ocean color based on gradient
                    gradientCol = settings.oceanColour.Evaluate(i / (textureResolution - 1f));
                }
                else
                {
                    // Biome color based on gradient
                    gradientCol = biome.gradient.Evaluate((i - textureResolution) / (textureResolution - 1f));
                }

                // Apply biome tint to the color
                Color tintCol = biome.tint;
                colours[colourIndex] = gradientCol * (1 - biome.tintPercent) + tintCol * biome.tintPercent;
                colourIndex++;
            }
        }

        // Apply the generated colors to the texture
        texture.SetPixels(colours);
        texture.Apply();

        // Set the texture to the planet's material
        settings.planetMaterial.SetTexture("_texture", texture);
    }

    // Updates the elevation range for the material based on provided min and max heights
    public void UpdateElevation(MinMax elevationMinMax)
    {
        // Set the elevation range to the material's shader
        settings.planetMaterial.SetVector("_elevationMinMax", new Vector4(elevationMinMax.Min, elevationMinMax.Max));
    }
}