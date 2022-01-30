using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    #region Singleton
    public static TerrainGenerator instance;
    void Awake() { instance = this; }
    #endregion

    public GameObject terrainChunk;
    public Transform player;

    [Space]

    public GenerateData generateData;
    public LODClass[] ChunkLODs;
    public float TimeBetweenChunkGen;

    public static Dictionary<Vector2Int, TerrainChunk> chunks = new Dictionary<Vector2Int, TerrainChunk>();
    public static Dictionary<Vector2Int, ChunkChangesClass> ChunkChanges = new Dictionary<Vector2Int, ChunkChangesClass>();
    public static List<TerrainChunk> PooledChunks = new List<TerrainChunk>();

    static GameObject pooledChunksHolder;
    static GameObject chunksHolder;

    Vector2Int oldChunk = new Vector2Int(0, 0);

    void Start()
    {
        pooledChunksHolder = new GameObject("Pooled Chunks");
        pooledChunksHolder.transform.position = Vector3.zero;
        pooledChunksHolder.transform.parent = gameObject.transform;

        chunksHolder = new GameObject("Active Chunks");
        chunksHolder.transform.position = Vector3.zero;
        chunksHolder.transform.parent = gameObject.transform;

        StartCoroutine(ILoadChunks());
    }
    IEnumerator ILoadChunks() { yield return new WaitForSeconds(0.1f); FirstChunkUpdate(); }

    void BuildChunk(Vector2Int coord, int lod, bool tween = false)
    {
        Vector2Int sampleCentre = new Vector2Int(coord.x * 16, coord.y * 16);
        TerrainChunk chunk;

        if (PooledChunks.Count > 0)
        {
            chunk = PooledChunks[0];
            PooledChunks.RemoveAt(0);

            chunk.gameObject.SetActive(true);
        }
        else
        {
            chunk = Instantiate(terrainChunk, new Vector3(sampleCentre.x, 0, sampleCentre.y), Quaternion.identity).GetComponent<TerrainChunk>();
        }

        chunk.SetupChunk(lod);

        chunk.transform.position = new Vector3(sampleCentre.x, 0, sampleCentre.y);
        chunk.transform.parent = chunksHolder.transform;
        chunk.transform.name = "Chunk - " + sampleCentre.x + ", " + sampleCentre.y;

        if (SavingManager.ActiveSave.Chunks.ContainsKey(coord))
        {
            chunk.blocks = SavingManager.ActiveSave.Chunks[coord];
        }
        else
        {
            chunk.blocks = generateData.GenerateChunk(coord);
            SavingManager.ActiveSave.Chunks.Add(coord, chunk.blocks);
        }

        chunk.Coord = coord;
        chunk.BuildMesh();

        chunks.Add(coord, chunk);
    }

    public void FirstChunkUpdate()
    {
        oldChunk = new Vector2Int(Mathf.FloorToInt(player.position.x / 16), Mathf.FloorToInt(player.position.z / 16));

        List<Vector2Int> prevChunks = chunks.Keys.ToList();
        Dictionary<Vector2Int, int> chunksToGen = new Dictionary<Vector2Int, int>();

        for (int i = ChunkLODs.Length - 1; i >= 0; i--)
        {
            for (int x = oldChunk.x - ChunkLODs[i].LODDst; x < oldChunk.x + ChunkLODs[i].LODDst; x++)
            {
                for (int y = oldChunk.y - ChunkLODs[i].LODDst; y < oldChunk.y + ChunkLODs[i].LODDst; y++)
                {
                    Vector2Int chunkIter = new Vector2Int(x, y);
                    if (prevChunks.Contains(chunkIter)) prevChunks.Remove(chunkIter);

                    if (chunksToGen.ContainsKey(chunkIter)) chunksToGen[chunkIter] = ChunkLODs[i].LOD;
                    else chunksToGen.Add(chunkIter, ChunkLODs[i].LOD);
                }
            }
        }

        for (int i = 0; i < prevChunks.Count; i++)
        {
            chunks[prevChunks[i]].gameObject.SetActive(false);
        }

        StartCoroutine(IToGenerate(chunksToGen));
    }

    void FixedUpdate()
    {
        Vector2Int newChunk = new Vector2Int(Mathf.FloorToInt(player.position.x / 16), Mathf.FloorToInt(player.position.z / 16));

        if (oldChunk.x != newChunk.x || oldChunk.y != newChunk.y)
        {
            oldChunk = newChunk;
            UpdateChunks();
        }
    }

    public void UpdateChunks()
    {
        List<Vector2Int> prevChunks = chunks.Keys.ToList();
        Dictionary<Vector2Int, int> chunksToGen = new Dictionary<Vector2Int, int>();

        for (int i = ChunkLODs.Length - 1; i >= 0; i--)
        {
            for (int x = oldChunk.x - ChunkLODs[i].LODDst; x < oldChunk.x + ChunkLODs[i].LODDst; x++)
            {
                for (int y = oldChunk.y - ChunkLODs[i].LODDst; y < oldChunk.y + ChunkLODs[i].LODDst; y++)
                {
                    Vector2Int chunkIter = new Vector2Int(x, y);
                    if (prevChunks.Contains(chunkIter)) prevChunks.Remove(chunkIter);

                    if (chunksToGen.ContainsKey(chunkIter)) chunksToGen[chunkIter] = ChunkLODs[i].LOD;
                    else chunksToGen.Add(chunkIter, ChunkLODs[i].LOD);
                }
            }
        }

        for (int i = 0; i < prevChunks.Count; i++)
        {
            chunks[prevChunks[i]].gameObject.SetActive(false);

            PooledChunks.Add(chunks[prevChunks[i]]);
            chunks.Remove(prevChunks[i]);
        }

        StartCoroutine(IToGenerate(chunksToGen));
    }

    IEnumerator IToGenerate(Dictionary<Vector2Int, int> toGenerate)
    {
        while (toGenerate.Count > 0)
        {
            Vector2Int newChunk = toGenerate.ElementAt(0).Key;

            if (!chunks.ContainsKey(newChunk)) BuildChunk(newChunk, toGenerate[newChunk]);
            else
            {
                if (chunks[newChunk].lod != toGenerate[newChunk]) { chunks[newChunk].lod = toGenerate[newChunk]; chunks[newChunk].BuildMesh(); }
                chunks[newChunk].gameObject.SetActive(true);
            }

            toGenerate.Remove(newChunk);
            yield return new WaitForSeconds(TimeBetweenChunkGen);
        }
    }

    public static Vector3Int GetLocalPosToChunk(Vector3Int pos, Vector2Int chunkCoord) { return new Vector3Int(pos.x - (chunkCoord.x * 16), pos.y, pos.z - (chunkCoord.y * 16)); }
    public static Vector3Int GetGlobalPos(Vector3Int pos, Vector2Int chunkCoord) { return new Vector3Int(pos.x + (chunkCoord.x * 16), pos.y, pos.z + (chunkCoord.y * 16)); }
}

public class ChunkChangesClass { public List<StructureBlockClass> BlocksToChange = new List<StructureBlockClass>(); }
[System.Serializable]
public class LODClass { public int LODDst;[Range(1, 16)] public int LOD; }
