﻿using UnityEngine;

public class RidgidNoiseFilter : INoiseFilter
{
    NoiseSettings.RidgidNoiseSettings settings; // Holds the settings for rigid noise generation.
    Noise noise = new Noise(); // An instance of the Noise class used to generate noise values.

    // Constructor to initialize the filter with specific rigid noise settings.
    public RidgidNoiseFilter(NoiseSettings.RidgidNoiseSettings settings)
    {
        this.settings = settings;
    }

    // Evaluates the noise value at a given 3D point.
    public float Evaluate(Vector3 point)
    {
        float noiseValue = 0; // This will hold the final noise value.
        float frequency = settings.baseRoughness; // Controls the roughness of the noise at different layers.
        float amplitude = 1; // Controls the intensity of the noise.
        float weight = 1; // A weight factor used to reduce the influence of each layer.

        // Iterates through multiple layers to create complex noise.
        for (int i = 0; i < settings.numLayers; i++)
        {
            // Generate noise based on the current frequency and shift by a center value.
            float v = 1 - Mathf.Abs(noise.Evaluate(point * frequency + settings.centre));
            v *= v; // Squaring the value makes the noise more intense near 0, giving it a "rigid" feel.
            v *= weight; // Modulates the value by the weight (decaying each layer's influence).
            weight = Mathf.Clamp01(v * settings.weightMultiplier); // Modifies the weight for the next layer.

            // Accumulates the noise value with the amplitude at the current layer.
            noiseValue += v * amplitude;

            // Adjusts frequency and amplitude for the next layer.
            frequency *= settings.roughness; // Frequency increases with each layer.
            amplitude *= settings.persistence; // Amplitude decreases with each layer to add finer details.
        }

        // Subtracts the min value from the noise to allow for a customizable range.
        noiseValue = noiseValue - settings.minValue;
        
        // Scales the final result by the strength factor.
        return noiseValue * settings.strength;
    }
}