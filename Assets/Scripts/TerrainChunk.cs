using UnityEngine;

public class TerrainChunk {

    // Threshold for distance to generate a collider for the terrain chunk.
    const float colliderGenerationDistanceThreshold = 5;

    // Event triggered when the visibility of the terrain chunk changes.
    public event System.Action<TerrainChunk, bool> onVisibilityChanged;

    // Coordinates of the terrain chunk.
    public Vector2 coord;

    
    GameObject chunkObject;
    
    Vector2 sampleCentre;
    
    Bounds chunkBounds;

    // Mesh components to display and collide with terrain.
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    
    LODInfo[] detailLevels;
    LODMesh[] lodMeshes;
    // The LOD index that controls the generation of colliders.
    int colliderLODIndex;

    // HeightMap data for terrain elevation.
    HeightMap heightMap;
    // Flag to check if heightmap data has been received.
    bool heightMapReceived;
    // Previous LOD index used for optimization.
    int previousLODIndex = -1;
    // Flag to check if the collider has been set.
    bool hasSetCollider;
    // Maximum view distance for the terrain chunk.
    float maxViewDst;

    // Settings for heightmap and mesh.
    HeightMapSettings heightMapSettings;
    MeshSettings meshSettings;
    // Transform representing the viewer (e.g., the player).
    Transform viewer;

    // Constructor that sets up a terrain chunk with provided parameters.
    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material material) {
        this.coord = coord;
        this.detailLevels = detailLevels;
        this.colliderLODIndex = colliderLODIndex;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.viewer = viewer;

        // Calculate the sample center for the terrain chunk.
        sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
        // Calculate the position of the chunk in the world.
        Vector2 position = coord * meshSettings.meshWorldSize;
        chunkBounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

        // Create the terrain chunk GameObject and attach necessary components.
        chunkObject = new GameObject("Terrain Chunk");
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshRenderer.material = material;

        // Set the chunk position and parent.
        chunkObject.transform.position = new Vector3(position.x, 0, position.y);
        chunkObject.transform.parent = parent;
        // Initially set the chunk as not visible.
        SetVisible(false);

        // Initialize the LOD mesh data for each LOD level.
        lodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++) {
            lodMeshes[i] = new LODMesh(detailLevels[i].lod);
            lodMeshes[i].updateCallback += UpdateTerrainChunk;
            if (i == colliderLODIndex) {
                lodMeshes[i].updateCallback += UpdateCollisionMesh;
            }
        }

        // Set the maximum view distance based on the last detail level.
        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
    }

    // Requests the heightmap data asynchronously and triggers the update once it's received.
    public void Load() {
        ThreadedDataRequester.RequestData(() => HeightMapGenerator.CreateHeightMap(meshSettings.vertsPerLine, meshSettings.vertsPerLine, heightMapSettings, sampleCentre), OnHeightMapReceived);
    }

    // Called when the heightmap data has been received.
    void OnHeightMapReceived(object heightMapObject) {
        this.heightMap = (HeightMap)heightMapObject;
        heightMapReceived = true;
        // Once heightmap is received, update the terrain chunk.
        UpdateTerrainChunk();
    }

    // Property to get the viewer's position in the XZ plane.
    Vector2 viewerPosition {
        get {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }

    // Updates the terrain chunk based on the viewer's position and distance.
    public void UpdateTerrainChunk() {
        if (heightMapReceived) {
            // Calculate the distance between the viewer and the chunk's nearest edge.
            float viewerDstFromNearestEdge = Mathf.Sqrt(chunkBounds.SqrDistance(viewerPosition));

            // Track the previous visibility status to avoid redundant operations.
            bool wasVisible = IsVisible();
            bool visible = viewerDstFromNearestEdge <= maxViewDst;

            if (visible) {
                // Determine the appropriate LOD based on the distance to the viewer.
                int lodIndex = 0;
                for (int i = 0; i < detailLevels.Length - 1; i++) {
                    if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold) {
                        lodIndex = i + 1;
                    } else {
                        break;
                    }
                }

                // If the LOD has changed, update the mesh.
                if (lodIndex != previousLODIndex) {
                    LODMesh lodMesh = lodMeshes[lodIndex];
                    if (lodMesh.hasMesh) {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                    } else if (!lodMesh.hasRequestedMesh) {
                        // Request the mesh if it hasn't been requested yet.
                        lodMesh.RequestMesh(heightMap, meshSettings);
                    }
                }
            }

            // Update visibility if the chunk has moved in or out of the view.
            if (wasVisible != visible) {
                SetVisible(visible);
                // Trigger visibility change event if subscribed.
                onVisibilityChanged?.Invoke(this, visible);
            }
        }
    }

    // Updates the collider mesh based on the viewer's distance and the LOD.
    public void UpdateCollisionMesh() {
        if (!hasSetCollider) {
            // Calculate the distance between the viewer and the chunk's edges.
            float sqrDstFromViewerToEdge = chunkBounds.SqrDistance(viewerPosition);

            // Request the collision mesh if it's within range and hasn't been requested yet.
            if (sqrDstFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDstThreshold) {
                if (!lodMeshes[colliderLODIndex].hasRequestedMesh) {
                    lodMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
                }
            }

            // Set the mesh collider once it's within the generation distance threshold.
            if (sqrDstFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold) {
                if (lodMeshes[colliderLODIndex].hasMesh) {
                    meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                    hasSetCollider = true;
                }
            }
        }
    }

    // Sets the chunk's visibility status (active or inactive).
    public void SetVisible(bool visible) {
        chunkObject.SetActive(visible);
    }

    // Checks if the chunk is currently visible.
    public bool IsVisible() {
        return chunkObject.activeSelf;
    }
}

// LODMesh class handles the different LOD levels for terrain chunks.
class LODMesh {

    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    int lod;
    public event System.Action updateCallback;

    public LODMesh(int lod) {
        this.lod = lod;
    }

    // Callback to handle the mesh details once they are received.
    void OnMeshDetailsReceived(object MeshDetailsObject) {
        mesh = ((MeshDetails)MeshDetailsObject).MakeMesh();
        hasMesh = true;
        // Invoke the callback after the mesh is ready.
        updateCallback();
    }

    // Requests the terrain mesh for a specific LOD level.
    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings) {
        hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.CreateTerrainMesh(heightMap.values, meshSettings, lod), OnMeshDetailsReceived);
    }
}