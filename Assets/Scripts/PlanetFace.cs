﻿using UnityEngine;

public class TerrainFace
{
    Mesh mesh;                    // The mesh object to store terrain data.
    ShapeGenerator shapeGenerator; // The shape generator responsible for terrain generation.
    int resolution;               // The resolution of the terrain (number of vertices).
    Vector3 axisA;                // Axis vector used for shaping the terrain.
    Vector3 axisB;                // Axis vector used for shaping the terrain.
    Vector3 localUp;              // Local up direction of the terrain face.

    // Constructor to initialize the terrain face.
    public TerrainFace(ShapeGenerator shapeGenerator, Mesh mesh, int resolution, Vector3 localUp)
    {
        this.mesh = mesh;
        this.shapeGenerator = shapeGenerator;
        this.resolution = resolution;

        // Define axis vectors to help generate terrain points.
        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);

        this.localUp = localUp;
    }

    // Method to generate the mesh for the terrain face.
    public void MakeMesh()
    {
        // Arrays to store mesh vertices, triangles, and UV coordinates.
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6]; // 6 indices per triangle
        int triIndex = 0;
        Vector2[] uv = (mesh.uv.Length == vertices.Length) ? mesh.uv : new Vector2[vertices.Length];

        // Loop through each vertex to calculate its position and texture coordinates.
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution; // Calculate the index in the vertex array
                Vector2 percent = new Vector2(x, y) / (resolution - 1); // Normalize the vertex position

                // Calculate the point on the unit cube before mapping it to the sphere.
                Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;

                // Calculate the elevation based on the terrain shape.
                float unscaledElevation = shapeGenerator.CalcUnscaledElevation(pointOnUnitSphere);

                // Apply the elevation scaling to the vertex position.
                vertices[i] = pointOnUnitSphere * shapeGenerator.GetScaledElevation(unscaledElevation);
                
                // Set the UV y-coordinate to represent the unscaled elevation for texturing.
                uv[i].y = unscaledElevation;

                // Generate the triangles for the terrain mesh.
                if (x != resolution - 1 && y != resolution - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + resolution + 1;
                    triangles[triIndex + 2] = i + resolution;

                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + resolution + 1;
                    triIndex += 6;
                }
            }
        }

        // Clear the existing mesh data and apply the newly generated vertices, triangles, and UVs.
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals(); // Recalculate normals for proper shading.
        mesh.uv = uv; // Apply the texture coordinates.
    }

    // Method to update UV coordinates for texturing based on biome information.
    public void UpdateUVs(ColourGenerator colourGenerator)
    {
        Vector2[] uv = mesh.uv;

        // Loop through each vertex and calculate the biome-based UV coordinate.
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution; // Calculate the index in the UV array
                Vector2 percent = new Vector2(x, y) / (resolution - 1); // Normalize the vertex position
                Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;

                // Get the biome type at the current point and set it as the x-coordinate of the UV.
                uv[i].x = colourGenerator.BiomePercentFromPoint(pointOnUnitSphere);
            }
        }
        
        // Update the mesh's UV coordinates with the calculated values.
        mesh.uv = uv;
    }
}