using UnityEngine;
using System.Linq;

// Create a custom scriptable object for managing texture data, including layer configurations for materials
[CreateAssetMenu()]
public class TextureData : UpdatableData {

    // Constants defining texture size and format
    const int textureSize = 512;
    const TextureFormat textureFormat = TextureFormat.RGB565;

    // Array of layers, each representing a texture layer with its properties
    public Layer[] layers;

    // Variables to store saved mesh height values
    float savedMinHeight;
    float savedMaxHeight;

    // Applies the texture data to a material, setting the appropriate properties
    public void ApplyToMaterial(Material material) {

        // Set the number of layers in the material
        material.SetInt("layerCount", layers.Length);

        // Set arrays of properties for each layer to the material
        material.SetColorArray("baseColours", layers.Select(x => x.tint).ToArray());
        material.SetFloatArray("baseStartHeights", layers.Select(x => x.startHeight).ToArray());
        material.SetFloatArray("baseBlends", layers.Select(x => x.blendStrength).ToArray());
        material.SetFloatArray("baseColourStrength", layers.Select(x => x.tintStrength).ToArray());
        material.SetFloatArray("baseTextureScales", layers.Select(x => x.textureScale).ToArray());

        // Generate a texture array from the layer textures and set it to the material
        Texture2DArray texturesArray = GenerateTextureArray(layers.Select(x => x.texture).ToArray());
        material.SetTexture("baseTextures", texturesArray);

        // Update the mesh height values for the material
        UpdateMeshHeights(material, savedMinHeight, savedMaxHeight);
    }

    // Updates the minimum and maximum heights for the material
    public void UpdateMeshHeights(Material material, float minHeight, float maxHeight) {
        savedMinHeight = minHeight;
        savedMaxHeight = maxHeight;

        // Set the height values to the material
        material.SetFloat("minHeight", minHeight);
        material.SetFloat("maxHeight", maxHeight);
    }

    // Generates a Texture2DArray from an array of textures
    Texture2DArray GenerateTextureArray(Texture2D[] textures) {
        // Create a new Texture2DArray with the specified size and format
        Texture2DArray textureArray = new Texture2DArray(textureSize, textureSize, textures.Length, textureFormat, true);

        // Copy pixels from each texture into the array
        for (int i = 0; i < textures.Length; i++) {
            textureArray.SetPixels(textures[i].GetPixels(), i);
        }

        // Apply the changes to the texture array
        textureArray.Apply();
        return textureArray;
    }

    // Serializable class representing a single texture layer with its properties
    [System.Serializable]
    public class Layer {
        public Texture2D texture;      // The texture for the layer
        public Color tint;             // The tint color for the layer
        [Range(0, 1)] public float tintStrength;  // The strength of the tint
        [Range(0, 1)] public float startHeight;  // The height at which the layer starts
        [Range(0, 1)] public float blendStrength; // The blend strength of the layer
        public float textureScale;     // The scaling of the texture
    }
}
