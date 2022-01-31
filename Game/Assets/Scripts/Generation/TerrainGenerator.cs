using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour
{
    public static Dictionary<Vector2Int, ChunkChangesClass> ChunkChanges = new Dictionary<Vector2Int, ChunkChangesClass>();

    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public LODClass[] detailLevels;
    public GenerateData gd;

    public Transform viewer;
    public Material mapMaterial;

    Vector2Int viewerPosition;
    Vector2Int viewerPositionOld;

    float meshWorldSize;
    int chunksVisibleInViewDst;

    public static Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    public static List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    public static System.Random rand = new System.Random();

    void Start()
    {
        chunksVisibleInViewDst = Mathf.RoundToInt(detailLevels[detailLevels.Length - 1].LODDst / 16);
        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2Int(Mathf.FloorToInt(viewer.position.x / 16), Mathf.FloorToInt(viewer.position.z / 16));

        if (viewerPositionOld.x != viewerPosition.x || viewerPositionOld.y != viewerPosition.y)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks()
    {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--)
        {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / 16);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / 16);

        for (int yOffset = viewerPosition.y - chunksVisibleInViewDst; yOffset <= viewerPosition.y + chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = viewerPosition.x - chunksVisibleInViewDst; xOffset <= viewerPosition.x + chunksVisibleInViewDst; xOffset++)
            {
                Vector2Int viewedChunkCoord = new Vector2Int(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                {
                    if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    }
                    else
                    {
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, gd, detailLevels, transform, viewer, mapMaterial);
                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                        newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                        newChunk.Load();
                    }
                }

            }
        }
    }

    void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
    {
        if (isVisible)
        {
            visibleTerrainChunks.Add(chunk);
        }
        else
        {
            visibleTerrainChunks.Remove(chunk);
        }
    }

    public static Vector3Int GetLocalPosToChunk(Vector3Int pos, Vector2Int chunkCoord) { return new Vector3Int(pos.x - (chunkCoord.x * 16), pos.y, pos.z - (chunkCoord.y * 16)); }
    public static Vector3Int GetGlobalPos(Vector3Int pos, Vector2Int chunkCoord) { return new Vector3Int(pos.x + (chunkCoord.x * 16), pos.y, pos.z + (chunkCoord.y * 16)); }
}

public class ChunkChangesClass { public List<StructureBlockClass> BlocksToChange = new List<StructureBlockClass>(); }
[System.Serializable]
public class LODClass { public int LODDst;[Range(1, 16)] public int LOD; }

/*
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

    public static GameObject chunksHolder;

    Vector2Int oldChunk = new Vector2Int(0, 0);

    void Start()
    {
        chunksHolder = new GameObject("Active Chunks");
        chunksHolder.transform.position = Vector3.zero;
        chunksHolder.transform.parent = gameObject.transform;

        StartCoroutine(ILoadChunks());
    }
    IEnumerator ILoadChunks() { yield return new WaitForSeconds(0.1f); FirstChunkUpdate(); }

    void BuildChunk(Vector2Int coord, int lod, bool tween = false)
    {
        if (chunks.ContainsKey(coord)) return;
        TerrainChunk chunk = Instantiate(terrainChunk, Vector3.zero, Quaternion.identity).GetComponent<TerrainChunk>();
        chunks.Add(coord, chunk);

        chunk.SetupChunk(lod, coord, generateData);
    }

    public void FirstChunkUpdate()
    {
        oldChunk = new Vector2Int(Mathf.FloorToInt(player.position.x / 16), Mathf.FloorToInt(player.position.z / 16));

        Dictionary<Vector2Int, int> chunksToGen = new Dictionary<Vector2Int, int>();

        for (int i = 0; i < ChunkLODs.Length; i++)
        {
            for (int x = oldChunk.x - ChunkLODs[i].LODDst; x < oldChunk.x + ChunkLODs[i].LODDst; x++)
            {
                for (int y = oldChunk.y - ChunkLODs[i].LODDst; y < oldChunk.y + ChunkLODs[i].LODDst; y++)
                {
                    Vector2Int chunkIter = new Vector2Int(x, y);
                    if (!chunksToGen.ContainsKey(chunkIter)) chunksToGen.Add(chunkIter, ChunkLODs[i].LOD);
                }
            }
        }

        for (int i = 0; i < chunksToGen.Count; i++) BuildChunk(chunksToGen.ElementAt(i).Key, chunksToGen.ElementAt(i).Value);
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
                    if (!chunksToGen.ContainsKey(chunkIter) && !prevChunks.Contains(chunkIter) && !chunks.ContainsKey(chunkIter)) chunksToGen.Add(chunkIter, ChunkLODs[i].LOD);
                    if (prevChunks.Contains(chunkIter)) { chunks[chunkIter].UpdateLod(ChunkLODs[i].LOD); prevChunks.Remove(chunkIter); }
                }
            }
        }

        for (int i = 0; i < prevChunks.Count; i++)
        {
            Destroy(chunks[prevChunks[i]].gameObject);
            chunks.Remove(prevChunks[i]);
        }

        for (int i = 0; i < chunksToGen.Count; i++) BuildChunk(chunksToGen.ElementAt(i).Key, chunksToGen.ElementAt(i).Value);
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
*/