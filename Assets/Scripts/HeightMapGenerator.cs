using UnityEngine;

public static class HeightMapGenerator
{
    // Generates a height map based on Perlin noise, falloff, and an adjustable height curve
    public static HeightMap CreateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCentre)
    {
        // Generate a 2D noise map using the NoiseGenerator class
        float[,] values = NoiseGenerator.CreateNoiseMap(width, height, settings.noiseConfig, sampleCentre);

        // Create a thread-safe height curve (animation curve) based on the provided settings
        AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.keys);

        // Variables to track the minimum and maximum height values
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        // Check if falloff effect is enabled and create a falloff map if necessary
        if (settings.useFalloff)
        {
            // Generate the falloff map which influences the height map based on distance
            float[,] falloffMap = TerrainFalloff.CreateFalloffMap(width);

            // Apply the falloff map to the noise values (multiplying them)
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    values[i, j] *= falloffMap[i, j]; // Apply falloff effect to height values
                }
            }
        }

        // Apply the height curve and height multiplier to each value in the noise map
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Apply the height curve and the multiplier to adjust the final height value
                values[i, j] *= heightCurve_threadsafe.Evaluate(values[i, j]) * settings.heightMultiplier;

                // Update the minimum and maximum values encountered
                if (values[i, j] > maxValue)
                {
                    maxValue = values[i, j];
                }
                if (values[i, j] < minValue)
                {
                    minValue = values[i, j];
                }
            }
        }

        // Return the generated height map along with the min and max height values
        return new HeightMap(values, minValue, maxValue);
    }
}

// Struct to hold the generated height map data, including the height values and min/max values
public struct HeightMap
{
    public readonly float[,] values; // 2D array of height values
    public readonly float minValue;  // Minimum height value in the map
    public readonly float maxValue;  // Maximum height value in the map

    // Constructor to initialize the height map with values and min/max height information
    public HeightMap(float[,] values, float minValue, float maxValue)
    {
        this.values = values;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}