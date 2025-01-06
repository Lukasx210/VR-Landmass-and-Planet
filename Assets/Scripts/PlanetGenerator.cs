﻿using UnityEngine;

public class Planet : MonoBehaviour
{
    // Public variables to control planet resolution and rendering options
    [Range(2, 256)]
    public int resolution = 10;                 // Resolution of the terrain (number of vertices per face)
    public bool autoUpdate = true;              // Whether the planet should auto-update when settings are changed
    public enum FaceRenderMask { All, Top, Bottom, Left, Right, Front, Back }; // Enum to control which faces to render
    public FaceRenderMask faceRenderMask;       // The selected face render mask

    public ShapeSettings shapeSettings;         // Shape settings for the planet's terrain
    public ColourSettings colourSettings;       // Colour settings for the planet's texture

    // These are used for displaying foldout options in the inspector
    [HideInInspector]
    public bool shapeSettingsFoldout;           
    [HideInInspector]
    public bool colourSettingsFoldout;

    // Internal objects to handle shape and colour generation
    ShapeGenerator shapeGenerator = new ShapeGenerator();
    ColourGenerator colourGenerator = new ColourGenerator();

    // MeshFilter array to store mesh filters for the planet's faces
    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;

    // Array to store terrain faces (one for each of the 6 faces of the planet)
    TerrainFace[] terrainFaces;

    // Start method to initiate planet generation
    void Start()
    {
        GeneratePlanet(); // Start generating the planet when the game starts
    }

    // Initialize the necessary components like mesh filters and terrain faces
    void Initialize()
    {
        // Update the shape and colour generators with the current settings
        shapeGenerator.UpdateSettings(shapeSettings);
        colourGenerator.UpdateSettings(colourSettings);

        // Ensure the meshFilters array is initialized
        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6]; // One MeshFilter per face (6 faces of the planet)
        }
        terrainFaces = new TerrainFace[6]; // Array to hold the terrain faces for each face of the planet

        // Directions for each face of the planet (6 faces: up, down, left, right, front, back)
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        // Loop through each face to initialize it
        for (int i = 0; i < 6; i++)
        {
            // If the MeshFilter for this face is not initialized, create a new one
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("mesh"); // Create a new GameObject for the face's mesh
                meshObj.transform.parent = transform; // Set this GameObject as a child of the planet

                meshObj.AddComponent<MeshRenderer>(); // Add a MeshRenderer to the GameObject
                meshFilters[i] = meshObj.AddComponent<MeshFilter>(); // Add a MeshFilter to the GameObject
                meshFilters[i].sharedMesh = new Mesh(); // Initialize the mesh
            }
            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = colourSettings.planetMaterial; // Assign the planet material

            // Initialize the terrain face for this face of the planet
            terrainFaces[i] = new TerrainFace(shapeGenerator, meshFilters[i].sharedMesh, resolution, directions[i]);

            // Decide whether to render this face based on the FaceRenderMask
            bool renderFace = faceRenderMask == FaceRenderMask.All || (int)faceRenderMask - 1 == i;
            meshFilters[i].gameObject.SetActive(renderFace); // Enable/Disable the face based on the selected mask
        }
    }

    // Generate the planet by initializing and then creating the mesh and textures
    public void GeneratePlanet()
    {
        Initialize();      // Initialize all the necessary components
        GenerateMesh();    // Generate the planet's mesh
        GenerateColours(); // Generate the planet's colours/texture
    }

    // Called when the shape settings are updated. It regenerates the mesh if autoUpdate is enabled
    public void OnShapeSettingsUpdated()
    {
        if (autoUpdate)
        {
            Initialize();    // Reinitialize the components
            GenerateMesh();  // Regenerate the mesh
        }
    }

    // Called when the colour settings are updated. It regenerates the colours if autoUpdate is enabled
    public void OnColourSettingsUpdated()
    {
        if (autoUpdate)
        {
            Initialize();    // Reinitialize the components
            GenerateColours(); // Regenerate the planet's colours
        }
    }

    // Generate the planet's mesh (terrain faces)
    void GenerateMesh()
    {
        for (int i = 0; i < 6; i++)
        {
            // Only generate the mesh for the active faces
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].MakeMesh(); // Create the mesh for each face of the planet
            }
        }

        // Update the elevation range for the colour generator based on the shape generator's results
        colourGenerator.UpdateElevation(shapeGenerator.elevationMinMax);
    }

    // Generate the colours/texture for the planet based on the colour settings and shape elevation
    void GenerateColours()
    {
        colourGenerator.UpdateColours(); // Update the colours based on the latest settings
        for (int i = 0; i < 6; i++)
        {
            // Only update the colours for the active faces
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].UpdateUVs(colourGenerator); // Update the UVs for each face
            }
        }
    }
}