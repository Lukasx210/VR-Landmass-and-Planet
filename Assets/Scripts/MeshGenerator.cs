﻿using UnityEngine;

public static class MeshGenerator {

    // Method to generate a terrain mesh from a heightmap
    public static MeshDetails CreateTerrainMesh(float[,] heightMap, MeshSettings meshSettings, int levelOfDetail) {

        // Calculate the skip increment based on the level of detail
        int skipIncrement = (levelOfDetail == 0)?1:levelOfDetail * 2;
        int vertsPerLine = meshSettings.vertsPerLine;

        // Define the starting point for the terrain
        Vector2 topLeft = new Vector2(-1, 1) * meshSettings.meshWorldSize / 2f;

        // Initialize mesh details
        MeshDetails meshDetails = new MeshDetails(vertsPerLine, skipIncrement, meshSettings.useFlatShading);

        int[,] vertexIndicesMap = new int[vertsPerLine, vertsPerLine];
        int meshVertexIndex = 0;
        int outOfMeshVertexIndex = -1;

        // Loop through each vertex and assign it a position or mark it as out of mesh
        for (int y = 0; y < vertsPerLine; y++) {
            for (int x = 0; x < vertsPerLine; x++) {
                bool isOutOfMeshVertex = y == 0 || y == vertsPerLine - 1 || x == 0 || x == vertsPerLine - 1;
                bool isSkippedVertex = x > 2 && x < vertsPerLine - 3 && y > 2 && y < vertsPerLine - 3 && ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0);
                if (isOutOfMeshVertex) {
                    vertexIndicesMap[x, y] = outOfMeshVertexIndex;
                    outOfMeshVertexIndex--;
                } else if (!isSkippedVertex) {
                    vertexIndicesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        // Loop again to calculate the positions of vertices, including edge connection vertices
        for (int y = 0; y < vertsPerLine; y++) {
            for (int x = 0; x < vertsPerLine; x++) {
                bool isSkippedVertex = x > 2 && x < vertsPerLine - 3 && y > 2 && y < vertsPerLine - 3 && ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0);

                if (!isSkippedVertex) {
                    // Determine vertex types and position calculations
                    bool isOutOfMeshVertex = y == 0 || y == vertsPerLine - 1 || x == 0 || x == vertsPerLine - 1;
                    bool isMeshEdgeVertex = (y == 1 || y == vertsPerLine - 2 || x == 1 || x == vertsPerLine - 2) && !isOutOfMeshVertex;
                    bool isMainVertex = (x - 2) % skipIncrement == 0 && (y - 2) % skipIncrement == 0 && !isOutOfMeshVertex && !isMeshEdgeVertex;
                    bool isEdgeConnectionVertex = (y == 2 || y == vertsPerLine - 3 || x == 2 || x == vertsPerLine - 3) && !isOutOfMeshVertex && !isMeshEdgeVertex && !isMainVertex;

                    // Calculate the vertex position based on the heightmap and the 2D position
                    int vertexIndex = vertexIndicesMap[x, y];
                    Vector2 percent = new Vector2(x - 1, y - 1) / (vertsPerLine - 3);
                    Vector2 vertexPosition2D = topLeft + new Vector2(percent.x, -percent.y) * meshSettings.meshWorldSize;
                    float height = heightMap[x, y];

                    // Handle edge connection vertices, calculating heights based on neighboring vertices
                    if (isEdgeConnectionVertex) {
                        bool isVertical = x == 2 || x == vertsPerLine - 3;
                        int dstToMainVertexA = ((isVertical)?y - 2:x-2) % skipIncrement;
                        int dstToMainVertexB = skipIncrement - dstToMainVertexA;
                        float dstPercentFromAToB = dstToMainVertexA / (float)skipIncrement;

                        float heightMainVertexA = heightMap[(isVertical) ? x : x - dstToMainVertexA, (isVertical) ? y - dstToMainVertexA : y];
                        float heightMainVertexB = heightMap[(isVertical) ? x : x + dstToMainVertexB, (isVertical) ? y + dstToMainVertexB : y];

                        height = heightMainVertexA * (1 - dstPercentFromAToB) + heightMainVertexB * dstPercentFromAToB;
                    }

                    // Add the vertex to the mesh details
                    meshDetails.AddVertex(new Vector3(vertexPosition2D.x, height, vertexPosition2D.y), percent, vertexIndex);

                    // Determine if triangles should be created for this vertex
                    bool makeTriangle = x < vertsPerLine - 1 && y < vertsPerLine - 1 && (!isEdgeConnectionVertex || (x != 2 && y != 2));

                    if (makeTriangle) {
                        int currentIncrement = (isMainVertex && x != vertsPerLine - 3 && y != vertsPerLine - 3) ? skipIncrement : 1;

                        // Create two triangles for each quadrilateral in the mesh
                        int a = vertexIndicesMap[x, y];
                        int b = vertexIndicesMap[x + currentIncrement, y];
                        int c = vertexIndicesMap[x, y + currentIncrement];
                        int d = vertexIndicesMap[x + currentIncrement, y + currentIncrement];
                        meshDetails.AddTriangle(a, d, c);
                        meshDetails.AddTriangle(d, a, b);
                    }
                }
            }
        }

        // Process the mesh details (bake normals or apply flat shading)
        meshDetails.ProcessMesh();

        // Return the final mesh details
        return meshDetails;
    }
}

// MeshDetails class stores vertex, triangle, and normal data
public class MeshDetails {
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;
    Vector3[] bakedNormals;

    Vector3[] outOfMeshVertices;
    int[] outOfMeshTriangles;

    int triangleIndex;
    int outOfMeshTriangleIndex;

    bool useFlatShading;

    // Constructor to initialize mesh details
    public MeshDetails(int vertsPerLine, int skipIncrement, bool useFlatShading) {
        this.useFlatShading = useFlatShading;

        // Calculate sizes for various mesh components
        int numMeshEdgeVertices = (vertsPerLine - 2) * 4 - 4;
        int numEdgeConnectionVertices = (skipIncrement - 1) * (vertsPerLine - 5) / skipIncrement * 4;
        int numMainVerticesPerLine = (vertsPerLine - 5) / skipIncrement + 1;
        int numMainVertices = numMainVerticesPerLine * numMainVerticesPerLine;

        // Initialize arrays for vertices, uvs, triangles, and out-of-mesh vertices
        vertices = new Vector3[numMeshEdgeVertices + numEdgeConnectionVertices + numMainVertices];
        uvs = new Vector2[vertices.Length];

        // Calculate the total number of triangles
        int numMeshEdgeTriangles = 8 * (vertsPerLine - 4);
        int numMainTriangles = (numMainVerticesPerLine - 1) * (numMainVerticesPerLine - 1) * 2;
        triangles = new int[(numMeshEdgeTriangles + numMainTriangles) * 3];

        outOfMeshVertices = new Vector3[vertsPerLine * 4 - 4];
        outOfMeshTriangles = new int[24 * (vertsPerLine-2)];
    }

    // Adds a vertex to the mesh
    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex) {
        if (vertexIndex < 0) {
            outOfMeshVertices[-vertexIndex - 1] = vertexPosition;
        } else {
            vertices[vertexIndex] = vertexPosition;
            uvs[vertexIndex] = uv;
        }
    }

    // Adds a triangle to the mesh
    public void AddTriangle(int a, int b, int c) {
        if (a < 0 || b < 0 || c < 0) {
            outOfMeshTriangles[outOfMeshTriangleIndex] = a;
            outOfMeshTriangles[outOfMeshTriangleIndex + 1] = b;
            outOfMeshTriangles[outOfMeshTriangleIndex + 2] = c;
            outOfMeshTriangleIndex += 3;
        } else {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }
    }

    // Calculates normals for the mesh based on triangle data
    Vector3[] CalculateNormals() {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;

        // Loop through all triangles and calculate the normal for each vertex
        for (int i = 0; i < triangleCount; i++) {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        // Calculate normals for out-of-mesh triangles
        int borderTriangleCount = outOfMeshTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++) {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = outOfMeshTriangles[normalTriangleIndex];
            int vertexIndexB = outOfMeshTriangles[normalTriangleIndex + 1];
            int vertexIndexC = outOfMeshTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if (vertexIndexA >= 0) vertexNormals[vertexIndexA] += triangleNormal;
            if (vertexIndexB >= 0) vertexNormals[vertexIndexB] += triangleNormal;
            if (vertexIndexC >= 0) vertexNormals[vertexIndexC] += triangleNormal;
        }

        // Normalize all normals
        for (int i = 0; i < vertexNormals.Length; i++) {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    // Calculates the normal of a surface from three vertices
    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC) {
        Vector3 pointA = (indexA < 0) ? outOfMeshVertices[-indexA - 1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? outOfMeshVertices[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? outOfMeshVertices[-indexC - 1] : vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    // Processes the mesh (apply either flat shading or baked normals)
    public void ProcessMesh() {
        if (useFlatShading) {
            FlatShading();
        } else {
            BakeNormals();
        }
    }

    // Bakes normals into the mesh
    void BakeNormals() {
        bakedNormals = CalculateNormals();
    }

    // Applies flat shading to the mesh
    void FlatShading() {
        Vector3[] flatShadedVertices = new Vector3[triangles.Length];
        Vector2[] flatShadedUvs = new Vector2[triangles.Length];

        // Assign each triangle a unique vertex and UV
        for (int i = 0; i < triangles.Length; i++) {
            flatShadedVertices[i] = vertices[triangles[i]];
            flatShadedUvs[i] = uvs[triangles[i]];
            triangles[i] = i; // Ensure each triangle has its own unique index
        }

        // Replace the original vertices and uvs with the flat-shaded data
        vertices = flatShadedVertices;
        uvs = flatShadedUvs;
    }

    // Converts the mesh details into a Unity Mesh object
    public Mesh MakeMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        if (useFlatShading) {
            mesh.RecalculateNormals();
        } else {
            mesh.normals = bakedNormals;
        }

        return mesh;
    }
}