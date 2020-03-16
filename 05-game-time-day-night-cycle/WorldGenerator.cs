using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour {

    public int WorldSizeInChunks = 10;

    public GameObject loadingScreen;

    Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

    void Start() {

        Generate();

    }

    void Generate () {

        loadingScreen.SetActive(true);

        for (int x = 0; x < WorldSizeInChunks; x++) {
            for (int z = 0; z < WorldSizeInChunks; z++) {

                Vector3Int chunkPos = new Vector3Int(x * GameData.ChunkWidth, 0, z * GameData.ChunkWidth);
                chunks.Add(chunkPos, new Chunk(chunkPos));
                chunks[chunkPos].chunkObject.transform.SetParent(World.Instance.transform);

            }
        }

        loadingScreen.SetActive(false);
        Debug.Log(string.Format("{0} x {0} world generated.", (WorldSizeInChunks * GameData.ChunkWidth)));

    }

    public Chunk GetChunkFromVector3 (Vector3 pos) {

        int x = (int)pos.x;
        int y = (int)pos.y;
        int z = (int)pos.z;

        return chunks[new Vector3Int(x, y, z)];

    }

}
