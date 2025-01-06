﻿using UnityEngine;

public static class NoiseGenerator
{
    // Enum to define whether the noise should be normalized locally or globally
    public enum NormalizeMode { Local, Global };

    // Creates a 2D noise map using Perlin noise with octaves and other configurations
    public static float[,] CreateNoiseMap(int width, int height, NoiseConfig config, Vector2 center)
    {
        // Initialize the noise map
        float[,] map = new float[width, height];

        // Seed for random number generation
        System.Random random = new System.Random(config.seed);

        // Array to store the offsets for each octave
        Vector2[] offsets = new Vector2[config.octaves];

        // Variables to track the maximum possible height and amplitude for noise calculation
        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        // Loop through each octave and calculate the total maximum possible height
        for (int i = 0; i < config.octaves; i++)
        {
            // Generate a random offset for each octave based on seed and center position
            float offsetX = random.Next(-100000, 100000) + config.offset.x + center.x;
            float offsetY = random.Next(-100000, 100000) - config.offset.y - center.y;
            offsets[i] = new Vector2(offsetX, offsetY);

            // Increase the max possible height for each octave's contribution to the total height
            maxPossibleHeight += amplitude;
            amplitude *= config.persistence;
        }

        // Initialize variables for tracking the minimum and maximum noise heights
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        // Half width and height used to center the noise map
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        // Loop through every pixel in the map to generate Perlin noise values
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Reset amplitude and frequency for each pixel
                amplitude = 1;
                frequency = 1;
                float currentHeight = 0;

                // Accumulate noise from all octaves
                for (int i = 0; i < config.octaves; i++)
                {
                    // Calculate the Perlin noise sample positions based on scale and frequency
                    float sampleX = (x - halfWidth + offsets[i].x) / config.scale * frequency;
                    float sampleY = (y - halfHeight + offsets[i].y) / config.scale * frequency;

                    // Generate Perlin noise value and scale it to be in the range [-1, 1]
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                    // Add the noise value to the current height, weighted by amplitude
                    currentHeight += perlinValue * amplitude;

                    // Adjust amplitude and frequency for the next octave
                    amplitude *= config.persistence;
                    frequency *= config.lacunarity;
                }

                // Track the minimum and maximum noise heights for normalization
                if (currentHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = currentHeight;
                }
                if (currentHeight < minNoiseHeight)
                {
                    minNoiseHeight = currentHeight;
                }

                // Store the generated height in the map
                map[x, y] = currentHeight;

                // Normalize the noise values based on the global mode
                if (config.normalizeMode == NormalizeMode.Global)
                {
                    float normalizedHeight = (map[x, y] + 1) / (maxPossibleHeight / 0.9f);
                    map[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        // Normalize the map values locally (each map independently) if specified
        if (config.normalizeMode == NormalizeMode.Local)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, map[x, y]);
                }
            }
        }

        // Return the generated noise map
        return map;
    }
}

// Configuration class for the noise generation settings
[System.Serializable]
public class NoiseConfig
{
    // Normalization mode for the generated noise map (Local or Global)
    public NoiseGenerator.NormalizeMode normalizeMode;

    // Scale for the noise (larger scale = less detail)
    public float scale = 50;

    // Number of octaves to use for multi-layered noise generation
    public int octaves = 6;

    // Persistence of each octave (controls how much influence each octave has)
    [Range(0, 1)]
    public float persistence = .6f;

    // Lacunarity (frequency multiplier between octaves)
    public float lacunarity = 2;

    // Random seed for noise generation (helps in creating repeatable results)
    public int seed;

    // Offset for shifting the noise pattern
    public Vector2 offset;

    // Ensures that the values are valid and within the expected ranges
    public void EnsureValidValues()
    {
        scale = Mathf.Max(scale, 0.01f); // Prevent a scale of 0 or negative
        octaves = Mathf.Max(octaves, 1); // Ensure at least one octave
        lacunarity = Mathf.Max(lacunarity, 1); // Lacunarity should be greater than or equal to 1
        persistence = Mathf.Clamp01(persistence); // Clamp persistence between 0 and 1
    }
}