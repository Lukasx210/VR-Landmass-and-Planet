﻿using UnityEngine;

public static class TextureGenerator {

    // Converts a color map (array of Color) into a Texture2D of specified width and height
    public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height) {
        // Create a new texture with the specified width and height
        Texture2D texture = new Texture2D(width, height);
        
        // Set the texture's filter mode to point for a pixelated look
        texture.filterMode = FilterMode.Point;
        
        // Set the texture's wrap mode to clamp to avoid tiling at the edges
        texture.wrapMode = TextureWrapMode.Clamp;
        
        // Set the pixels of the texture from the provided color map
        texture.SetPixels(colourMap);
        
        // Apply the changes to the texture
        texture.Apply();
        
        // Return the generated texture
        return texture;
    }

    // Converts a height map into a texture where each height value is mapped to a grayscale color
    public static Texture2D TextureFromHeightMap(HeightMap heightMap) {
        int texWidth = heightMap.values.GetLength(0); // Get the width of the height map
        int texHeight = heightMap.values.GetLength(1); // Get the height of the height map

        // Create an array to store the colors corresponding to the height values
        Color[] colourMap = new Color[texWidth * texHeight];
        
        // Loop through all height values and map them to a grayscale color
        for (int y = 0; y < texHeight; y++) {
            for (int x = 0; x < texWidth; x++) {
                // Linearly interpolate between black and white based on the height value
                colourMap[y * texWidth + x] = Color.Lerp(Color.black, Color.white, Mathf.InverseLerp(heightMap.minValue, heightMap.maxValue, heightMap.values[x, y]));
            }
        }

        // Generate the texture from the color map and return it
        return TextureFromColourMap(colourMap, texWidth, texHeight);
    }
}