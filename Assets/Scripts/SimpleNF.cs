﻿using UnityEngine;

public class SimpleNoiseFilter : INoiseFilter {

    NoiseSettings.SimpleNoiseSettings settings;  // Stores the settings used for the noise filter.
    Noise noise = new Noise();  // Instance of the Noise class, which likely handles the generation of the noise itself.

    // Constructor that accepts noise settings.
    public SimpleNoiseFilter(NoiseSettings.SimpleNoiseSettings settings)
    {
        this.settings = settings;  // Initialize the settings from the passed-in value.
    }

    // Evaluates the noise at a specific point (usually a point on the planet's surface).
    public float Evaluate(Vector3 point)
    {
        float noiseValue = 0;  // This will store the final calculated noise value at the point.
        float frequency = settings.baseRoughness;  // The base frequency for the noise, which controls the roughness of the terrain.
        float amplitude = 1;  // The amplitude (height) of the noise, starting at 1.

        // Loop through each layer of noise to add more detail to the elevation.
        for (int i = 0; i < settings.numLayers; i++)
        {
            float v = noise.Evaluate(point * frequency + settings.centre);  // Get the noise value for the current layer.
            noiseValue += (v + 1) * .5f * amplitude;  // Normalize the noise value to the range [0, 1] and apply the amplitude.
            frequency *= settings.roughness;  // Increase the frequency for the next layer, making the noise finer.
            amplitude *= settings.persistence;  // Decrease the amplitude for the next layer, making the noise less influential as layers increase.
        }

        noiseValue = noiseValue - settings.minValue;  // Subtract the minimum value to shift the noise range.
        return noiseValue * settings.strength;  // Apply the strength setting to adjust the final noise value.
    }
}