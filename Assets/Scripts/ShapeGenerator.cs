﻿using UnityEngine;

public class ShapeGenerator {

    ShapeSettings settings;  // Stores the settings used to generate the shape of the planet.
    INoiseFilter[] noiseFilters;  // Array of noise filters applied to generate the elevation.
    public MinMax elevationMinMax;  // Keeps track of the minimum and maximum elevation values.

    // Updates the settings of the shape generator and sets up the noise filters.
    public void UpdateSettings(ShapeSettings settings)
    {
        this.settings = settings;
        noiseFilters = new INoiseFilter[settings.noiseLayers.Length];  // Initialize noise filters based on the number of noise layers.

        // Create noise filters based on the noise settings for each layer.
        for (int i = 0; i < noiseFilters.Length; i++)
        {
            noiseFilters[i] = NoiseFilterFactory.MakeNoiseFilter(settings.noiseLayers[i].noiseSettings);
        }

        elevationMinMax = new MinMax();  // Initialize the MinMax instance to store elevation values.
    }

    // Calculates the unscaled elevation at a point on the unit sphere (representing a point on the planet's surface).
    public float CalcUnscaledElevation(Vector3 pointOnUnitSphere)
    {
        float firstLayerValue = 0;
        float elevation = 0;

        // If there are noise filters, get the value for the first noise layer.
        if (noiseFilters.Length > 0)
        {
            firstLayerValue = noiseFilters[0].Evaluate(pointOnUnitSphere);  // Calculate the first layer's noise value.
            if (settings.noiseLayers[0].enabled)  // Check if the first layer is enabled.
            {
                elevation = firstLayerValue;  // Set the elevation to the value of the first noise layer.
            }
        }

        // Process the remaining noise layers (if any).
        for (int i = 1; i < noiseFilters.Length; i++)
        {
            if (settings.noiseLayers[i].enabled)  // Check if the current noise layer is enabled.
            {
                // If the layer uses the first layer as a mask, apply the first layer's value as a mask.
                float mask = (settings.noiseLayers[i].useFirstLayerAsMask) ? firstLayerValue : 1;
                elevation += noiseFilters[i].Evaluate(pointOnUnitSphere) * mask;  // Add the noise value of the current layer to the elevation.
            }
        }

        // Track the minimum and maximum elevation values encountered during the calculation.
        elevationMinMax.AddValue(elevation);
        return elevation;  // Return the final unscaled elevation value.
    }

    // Scales the unscaled elevation using the planet's radius.
    public float GetScaledElevation(float unscaledElevation)
    {
        float elevation = Mathf.Max(0, unscaledElevation);  // Ensure that the elevation is non-negative.
        elevation = settings.planetRadius * (1 + elevation);  // Scale the elevation based on the planet's radius.
        return elevation;  // Return the scaled elevation value.
    }
}