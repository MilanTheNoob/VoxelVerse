using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Deals with everything related to generating & managing terrain chunks
/// </summary>
public class TerrainGenerator : MonoBehaviour
{
    #region Instance
    public static TerrainGenerator Instance;
    void Awake() { Instance = this; }
    #endregion

    public static Dictionary<Vector2Int, List<StructureBlockClass>> ChunkChanges = new Dictionary<Vector2Int, List<StructureBlockClass>>();
    public static bool CanGenerate = false;

    public LODClass[] detailLevels;
    public GenerateData gd;

    public Transform viewer;
    public Material mapMaterial;

    public static Vector2Int playerPos;
    public static Vector2Int playerPosOld;

    public static Dictionary<Vector2, TerrainChunk> Chunks = new Dictionary<Vector2, TerrainChunk>();
    public static List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    public static System.Random rand = new System.Random();
    public static int chunksVisibleInViewDst = 0;

    void Start() { chunksVisibleInViewDst = Mathf.RoundToInt(detailLevels[detailLevels.Length - 1].LODDst / 16); UpdateVisibleChunks(); }

    void FixedUpdate()
    {
        playerPos = new Vector2Int(Mathf.FloorToInt(viewer.position.x / 16), Mathf.FloorToInt(viewer.position.z / 16));
        if ((playerPosOld.x != playerPos.x || playerPosOld.y != playerPos.y) && CanGenerate) { playerPosOld = playerPos; UpdateVisibleChunks(); }
    }

    /// <summary>
    /// Updates all chunks in visible range, adds any new ones that need to be added and hides ones out of range
    /// </summary>
    public static void UpdateVisibleChunks()
    {
        if (chunksVisibleInViewDst == 0) chunksVisibleInViewDst = Mathf.RoundToInt(Instance.detailLevels[Instance.detailLevels.Length - 1].LODDst / 16);

        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--)
        {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(playerPos.x / 16);
        int currentChunkCoordY = Mathf.RoundToInt(playerPos.y / 16);

        for (int yOffset = playerPos.y - chunksVisibleInViewDst; yOffset <= playerPos.y + chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = playerPos.x - chunksVisibleInViewDst; xOffset <= playerPos.x + chunksVisibleInViewDst; xOffset++)
            {
                Vector2Int viewedChunkCoord = new Vector2Int(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                
                if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                {
                    if (Chunks.ContainsKey(viewedChunkCoord))
                    {
                        Chunks[viewedChunkCoord].UpdateTerrainChunk();
                    }
                    else
                    {
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, Instance.gd, Instance.detailLevels, Instance.transform, Instance.viewer, Instance.mapMaterial);
                        Chunks.Add(viewedChunkCoord, newChunk);
                        newChunk.onVisibilityChanged += Instance.OnTerrainChunkVisibilityChanged;
                        newChunk.Load();
                    }
                }

            }
        }
    }

    void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
    {
        if (isVisible) visibleTerrainChunks.Add(chunk);
        else visibleTerrainChunks.Remove(chunk);
    }

    public static Vector3Int GetLocalPosToChunk(Vector3Int pos, Vector2Int chunkCoord) { return new Vector3Int(pos.x - (chunkCoord.x * 16), pos.y, pos.z - (chunkCoord.y * 16)); }
    public static Vector3Int GetGlobalPos(Vector3Int pos, Vector2Int chunkCoord) { return new Vector3Int(pos.x + (chunkCoord.x * 16), pos.y, pos.z + (chunkCoord.y * 16)); }
}

[System.Serializable]
public class LODClass { public int LODDst;[Range(1, 16)] public int LOD; }