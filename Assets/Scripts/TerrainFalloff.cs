﻿using UnityEngine;

public static class TerrainFalloff {

    // Generates a falloff map of the given dimension (e.g., height map fading effect)
    // The map smooths out the terrain's transition from the center to the edges
    public static float[,] CreateFalloffMap(int dimension) {
        float[,] falloffMap = new float[dimension, dimension];

        // Loop through each cell in the falloff map
        for (int row = 0; row < dimension; row++) {
            for (int col = 0; col < dimension; col++) {
                // Normalize the coordinates to the range [-1, 1] (relative to the center of the map)
                float horizontal = row / (float)dimension * 2 - 1;
                float vertical = col / (float)dimension * 2 - 1;

                // Determine the maximum distance from the center between the horizontal and vertical axis
                float maxAxis = Mathf.Max(Mathf.Abs(horizontal), Mathf.Abs(vertical));
                // Calculate the falloff value based on the distance
                falloffMap[row, col] = CalculateFalloff(maxAxis);
            }
        }

        // Invert the falloff map values to create the smooth, outward fading effect
        for (int row = 0; row < dimension; row++) {
            for (int col = 0; col < dimension; col++) {
                falloffMap[row, col] = 1f - falloffMap[row, col]; // Invert the values
            }
        }

        return falloffMap;
    }

    // Calculate the falloff effect based on a given input distance (or normalized value)
    // This function applies a smoothing curve to the distance, creating a smooth transition
    static float CalculateFalloff(float input) {
        float alpha = 3f; // Controls the curve steepness for the falloff
        float beta = 2.2f; // Controls the transition behavior (a factor that adjusts the sharpness)

        // Use the falloff equation to return the appropriate smoothing value
        return Mathf.Pow(input, alpha) / (Mathf.Pow(input, alpha) + Mathf.Pow(beta - beta * input, alpha));
    }
}