﻿using UnityEngine;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour {

    // Threshold distance to trigger chunk update when the viewer moves
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    // Public settings for LOD, mesh, height map, and texture configuration
    public int colliderLODIndex;
    public LODInfo[] detailLevels;
    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureSettings;

    // Reference to the viewer (e.g., the camera) and the material for the map
    public Transform viewer;
    public Material mapMaterial;

    // Current and old viewer positions (for checking movement)
    Vector2 viewerPosition;
    Vector2 viewerPositionOld;

    // Mesh world size and number of chunks visible within view distance
    float meshWorldSize;
    int chunksVisibleInViewDst;

    // Dictionary to store terrain chunks and a list for the currently visible chunks
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    void Start() {
        // Apply texture settings to the material and update mesh height information
        textureSettings.ApplyToMaterial(mapMaterial);
        textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        // Calculate the maximum visible distance and the number of chunks that fit within the view distance
        float maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        meshWorldSize = meshSettings.meshWorldSize;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshWorldSize);

        // Initialize the visible chunks based on the viewer's initial position
        UpdateVisibleChunks();
    }

    void Update() {
        // Update the viewer's current position (ignoring the y-axis)
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        // If the viewer's position has changed, update the collision meshes for the visible chunks
        if (viewerPosition != viewerPositionOld) {
            foreach (TerrainChunk chunk in visibleTerrainChunks) {
                chunk.UpdateCollisionMesh();
            }
        }

        // If the viewer has moved beyond the threshold, update the visible chunks
        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate) {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    // Updates the visible terrain chunks based on the current viewer position
    void UpdateVisibleChunks() {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();

        // Update each visible chunk in the current view
        for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--) {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        // Calculate the chunk coordinates around the viewer's current position
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

        // Loop through all chunk coordinates within the visible distance
        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++) {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++) {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                // Only update chunks that have not been processed yet
                if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord)) {
                    if (terrainChunkDictionary.ContainsKey(viewedChunkCoord)) {
                        // If the chunk already exists, update it
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    } else {
                        // If the chunk doesn't exist, create and load it
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, viewer, mapMaterial);
                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                        newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                        newChunk.Load();
                    }
                }
            }
        }
    }

    // Event handler to update the visibility list when a terrain chunk's visibility changes
    void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible) {
        if (isVisible) {
            // Add chunk to the visible list when it becomes visible
            visibleTerrainChunks.Add(chunk);
        } else {
            // Remove chunk from the visible list when it becomes invisible
            visibleTerrainChunks.Remove(chunk);
        }
    }
}

// Struct for storing information about Level of Detail (LOD) settings for terrain chunks
[System.Serializable]
public struct LODInfo {
    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int lod;
    public float visibleDstThreshold;

    // Property to return the squared visible distance threshold (used to avoid square root calculations)
    public float sqrVisibleDstThreshold {
        get {
            return visibleDstThreshold * visibleDstThreshold;
        }
    }
}