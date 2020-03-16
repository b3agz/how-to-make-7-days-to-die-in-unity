using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {

    public GameObject chunkObject;
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    MeshRenderer meshRenderer;

    Vector3Int chunkPosition;

    float[,,] terrainMap;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    int width { get { return GameData.ChunkWidth; } }
    int height { get { return GameData.ChunkHeight; } }
    float terrainSurface { get { return GameData.terrainSurface; } }

    public Chunk (Vector3Int _position) {

        chunkObject = new GameObject();
        chunkObject.name = string.Format("Chunk {0}, {1}", _position.x, _position.z);
        chunkPosition = _position;
        chunkObject.transform.position = chunkPosition;

        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.material = Resources.Load<Material>("Materials/Terrain");
        chunkObject.transform.tag = "Terrain";
        terrainMap = new float[width + 1, height + 1, width + 1];
        PopulateTerrainMap();
        CreateMeshData();

    }

    void CreateMeshData() {

        ClearMeshData();

        // Loop through each "cube" in our terrain.
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < width; z++) {

                    // Pass the value into our MarchCube function.
                    MarchCube(new Vector3Int(x, y, z));

                }
            }
        }

        BuildMesh();

    }

    void PopulateTerrainMap () {

        // The data points for terrain are stored at the corners of our "cubes", so the terrainMap needs to be 1 larger
        // than the width/height of our mesh.
        for (int x = 0; x < width + 1; x++) {
            for (int z = 0; z < width + 1; z++) {
                for (int y = 0; y < height + 1; y++) {

                    float thisHeight;

                    // Get a terrain height using regular old Perlin noise.
                    thisHeight = GameData.GetTerrainHeight(x + chunkPosition.x, z + chunkPosition.z);

                    // Set the value of this point in the terrainMap.
                    terrainMap[x, y, z] = (float)y - thisHeight;

                }
            }
        }
    }

    void MarchCube (Vector3Int position) {

        // Sample terrain values at each corner of the cube.
        float[] cube = new float[8];
        for (int i = 0; i < 8; i++) {

            cube[i] = SampleTerrain(position + GameData.CornerTable[i]);

        }

        // Get the configuration index of this cube.
        int configIndex = GetCubeConfiguration(cube);

        // If the configuration of this cube is 0 or 255 (completely inside the terrain or completely outside of it) we don't need to do anything.
        if (configIndex == 0 || configIndex == 255)
            return;

        // Loop through the triangles. There are never more than 5 triangles to a cube and only three vertices to a triangle.
        int edgeIndex = 0;
        for(int i = 0; i < 5; i++) {
            for(int p = 0; p < 3; p++) {

                // Get the current indice. We increment triangleIndex through each loop.
                int indice = GameData.TriangleTable[configIndex, edgeIndex];

                // If the current edgeIndex is -1, there are no more indices and we can exit the function.
                if (indice == -1)
                    return;

                // Get the vertices for the start and end of this edge.
                Vector3 vert1 = position + GameData.CornerTable[GameData.EdgeIndexes[indice, 0]];
                Vector3 vert2 = position + GameData.CornerTable[GameData.EdgeIndexes[indice, 1]];

                Vector3 vertPosition;

                // Get the terrain values at either end of our current edge from the cube array created above.
                float vert1Sample = cube[GameData.EdgeIndexes[indice, 0]];
                float vert2Sample = cube[GameData.EdgeIndexes[indice, 1]];

                // Calculate the difference between the terrain values.
                float difference = vert2Sample - vert1Sample;

                // If the difference is 0, then the terrain passes through the middle.
                if (difference == 0)
                    difference = terrainSurface;
                else
                    difference = (terrainSurface - vert1Sample) / difference;

                // Calculate the point along the edge that passes through.
                vertPosition = vert1 + ((vert2 - vert1) * difference);

                // Add to our vertices and triangles list and incremement the edgeIndex.
                triangles.Add(VertForIndice(vertPosition));

                edgeIndex++;

            }
        }
    }

    int GetCubeConfiguration (float[] cube) {

        // Starting with a configuration of zero, loop through each point in the cube and check if it is below the terrain surface.
        int configurationIndex = 0;
        for (int i = 0; i < 8; i++) {

            // If it is, use bit-magic to the set the corresponding bit to 1. So if only the 3rd point in the cube was below
            // the surface, the bit would look like 00100000, which represents the integer value 32.
            if (cube[i] > terrainSurface)
                configurationIndex |= 1 << i;

        }

        return configurationIndex;

    }

    public void PlaceTerrain (Vector3 pos) {

        Vector3Int v3Int = new Vector3Int(Mathf.CeilToInt(pos.x), Mathf.CeilToInt(pos.y), Mathf.CeilToInt(pos.z));
        v3Int -= chunkPosition;
        terrainMap[v3Int.x, v3Int.y, v3Int.z] = 0f;
        CreateMeshData();

    }

    public void RemoveTerrain (Vector3 pos) {

        Vector3Int v3Int = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
        v3Int -= chunkPosition;
        terrainMap[v3Int.x, v3Int.y, v3Int.z] = 1f;
        CreateMeshData();

    }

    float SampleTerrain (Vector3Int point) {

        return terrainMap[point.x, point.y, point.z];

    }

    int VertForIndice (Vector3 vert) {

        // Loop through all the vertices currently in the vertices list.
        for (int i = 0; i < vertices.Count; i++) {

            // If we find a vert that matches ours, then simply return this index.
            if (vertices[i] == vert)
                return i;

        }

        // If we didn't find a match, add this vert to the list and return last index.
        vertices.Add(vert);
        return vertices.Count - 1;

    }

    void ClearMeshData () {

        vertices.Clear();
        triangles.Clear();

    }

    void BuildMesh () {

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

    }

}
