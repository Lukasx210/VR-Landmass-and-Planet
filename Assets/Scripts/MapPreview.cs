﻿using UnityEngine;

public class MapPreview : MonoBehaviour
{
    // References to different renderers and filters for the terrain preview
    public Renderer textureRender;    // Renders the texture (used for displaying noise or falloff map textures)
    public MeshFilter meshFilter;     // Mesh filter used to display terrain meshes
    public MeshRenderer meshRenderer; // Mesh renderer for rendering the terrain mesh

    // Enum to switch between different render modes (NoiseMap, Mesh, FalloffMap)
    public enum RenderMode { NoiseMap, Mesh, FalloffMap };
    public RenderMode renderMode; 
    
    public MeshSettings meshSettings;  // Mesh settings like LOD, number of vertices, etc.
    public HeightMapSettings heightMapSettings;  // Settings related to heightmap generation
    public TextureData textureData;  // Texture data for the terrain

    
    public Material terrainMaterial;

    // Settings for preview LOD (Level of Detail) and auto-update flag
    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int PreviewLOD;  // Preview LOD setting (to control mesh resolution)
    public bool enableautoUpdate;  // Flag to enable auto-update in editor

    // Method to render the terrain in the editor based on the selected render mode
    public void RenderMapInEditor()
    {
        // Apply texture and height adjustments to the material
        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        // Generate a heightmap based on the mesh settings and heightmap settings
        HeightMap heightMap = HeightMapGenerator.CreateHeightMap(meshSettings.vertsPerLine, meshSettings.vertsPerLine, heightMapSettings, Vector2.zero);

        // Switch based on render mode to render different aspects of the terrain
        if (renderMode == RenderMode.NoiseMap)
        {
            RenderTexture(TextureGenerator.TextureFromHeightMap(heightMap)); // Render the noise map as texture
        }
        else if (renderMode == RenderMode.Mesh)
        {
            // Generate and render the terrain mesh based on the heightmap values
            RenderMesh(MeshGenerator.CreateTerrainMesh(heightMap.values, meshSettings, PreviewLOD));
        }
        else if (renderMode == RenderMode.FalloffMap)
        {
            // Render the falloff map as a texture
            RenderTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(TerrainFalloff.CreateFalloffMap(meshSettings.vertsPerLine), 0, 1)));
        }
    }

    // Method to render a texture (such as the noise map or falloff map)
    public void RenderTexture(Texture2D texture)
    {
        // Assign the generated texture to the material and adjust scale
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10f;

        // Toggle the visibility of the texture renderer and mesh filter
        textureRender.gameObject.SetActive(true);
        meshFilter.gameObject.SetActive(false);
    }

    // Method to render a mesh (such as the terrain mesh)
    public void RenderMesh(MeshDetails meshDetails)
    {
        // Set the generated mesh to the mesh filter
        meshFilter.sharedMesh = meshDetails.MakeMesh();

        // Toggle visibility of the texture renderer and mesh filter
        textureRender.gameObject.SetActive(false);
        meshFilter.gameObject.SetActive(true);
    }

    // Called when height map or mesh values are updated
    void OnValuesUpdated()
    {
        // Only render map when not in play mode
        if (!Application.isPlaying)
        {
            RenderMapInEditor();
        }
    }

    // Called when texture values are updated
    void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial); // Apply texture updates to the material
    }

    // Called when any values are changed in the editor (such as mesh settings, height map settings, or texture data)
    void OnValidate()
    {
        // Register the callbacks to update the map or texture data when values are modified in the editor
        if (meshSettings != null)
        {
            meshSettings.OnValuesUpdated -= OnValuesUpdated;
            meshSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (heightMapSettings != null)
        {
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }
}