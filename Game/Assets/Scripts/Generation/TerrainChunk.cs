using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A standard TerrainChunk that stores the heightmap, LODs and the GameObject
/// </summary>
public class TerrainChunk
{
    public event System.Action<TerrainChunk, bool> onVisibilityChanged;
    public Vector2Int coord;

    GameObject meshObject;
    Vector2 sampleCentre;
    Bounds bounds;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    LODClass[] detailLevels;
    public LODMesh[] lodMeshes;

    GenerateData gd;

    public BlockType[,,] heightMap = new BlockType[18, 255, 18];
    bool heightMapReceived;
    public int previousLODIndex = -1;
    float maxViewDst;

    Transform viewer;

    /// <summary>
    /// Creates a standard Terrain Chunk using standard values found in the TerrainGenerator.cs
    /// </summary>
    /// <param name="coord">The coordinates of the chunk</param>
    /// <param name="gd">The generate data that provides parameters to the Noise functions</param>
    /// <param name="detailLevels">The level of details parameters</param>
    /// <param name="parent">The transform of the parent to store our GameObject</param>
    /// <param name="viewer">The player's trasnform</param>
    /// <param name="material">The material to apply to the chunk object</param>
    public TerrainChunk(Vector2Int coord, GenerateData gd, LODClass[] detailLevels, Transform parent, Transform viewer, Material material)
    {
        this.coord = coord;
        this.detailLevels = detailLevels;
        this.gd = gd;
        this.viewer = viewer;

        sampleCentre = coord * 16;
        bounds = new Bounds(sampleCentre, Vector2.one);

        meshObject = new GameObject("Chunk" + coord);
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();
        meshRenderer.material = material;

        meshObject.transform.position = new Vector3(sampleCentre.x, 0, sampleCentre.y);
        meshObject.transform.parent = parent;
        SetVisible(false);

        lodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++)
        {
            lodMeshes[i] = new LODMesh(detailLevels[i].LOD);
            lodMeshes[i].updateCallback += UpdateTerrainChunk;
        }

        maxViewDst = detailLevels[detailLevels.Length - 1].LODDst;
        meshObject.layer = 8;
    }

    public void Load()
    {
        if (SavingManager.AdventureSave.Chunks.ContainsKey(coord))
        {
            heightMap = SavingManager.AdventureSave.Chunks[coord];
            OnHeightMapReceived((object)heightMap);
        }
        else
        {
            OnHeightMapReceived((object)gd.GenerateChunk(coord, TerrainGenerator.rand));
            //ThreadedManager.RequestData(() => gd.GenerateChunk(coord, TerrainGenerator.rand), OnHeightMapReceived);
        }
    }



    void OnHeightMapReceived(object heightMapObject)
    {
        this.heightMap = (BlockType[,,])heightMapObject;
        heightMapReceived = true;

        if (!SavingManager.AdventureSave.Chunks.ContainsKey(coord)) SavingManager.AdventureSave.Chunks.Add(coord, heightMap);
        UpdateTerrainChunk();
    }

    Vector2 playerPos
    {
        get
        {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }


    public void UpdateTerrainChunk()
    {
        if (heightMapReceived)
        {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(playerPos));

            bool wasVisible = IsVisible();
            bool visible = viewerDstFromNearestEdge <= maxViewDst;

            if (visible)
            {
                int lodIndex = 0;

                for (int i = 0; i < detailLevels.Length - 1; i++)
                {
                    if (viewerDstFromNearestEdge > detailLevels[i].LODDst)
                    {
                        lodIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                LODMesh lodMesh = lodMeshes[lodIndex];
                if (lodMesh.hasMesh)
                {
                    previousLODIndex = lodIndex;
                    meshFilter.mesh = lodMesh.mesh;
                    meshCollider.sharedMesh = lodMesh.mesh;
                }
                else if (!lodMesh.hasRequestedMesh)
                {
                    lodMesh.RequestMesh(heightMap, coord);
                }
            }

            if (wasVisible != visible)
            {

                SetVisible(visible);
                if (onVisibilityChanged != null)
                {
                    onVisibilityChanged(this, visible);
                }
            }
        }
    }

    public void SetVisible(bool visible)
    {
        meshObject.SetActive(visible);
    }

    public bool IsVisible()
    {
        return meshObject.activeSelf;
    }

}

public class LODMesh
{

    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    int lod;
    public event System.Action updateCallback;

    public LODMesh(int lod)
    {
        this.lod = lod;
    }

    void OnMeshDataReceived(object meshDataObject)
    {
        mesh = ((MeshData)meshDataObject).CreateMesh();
        hasMesh = true;

        updateCallback();
    }

    public void RequestMesh(BlockType[,,] heightMaps, Vector2Int coord)
    {
        hasRequestedMesh = true;
        ThreadedManager.RequestData(() => GenerateMesh(heightMaps, lod, coord), OnMeshDataReceived);
    }

    MeshData GenerateMesh(BlockType[,,] blocks, int l, Vector2Int coord)
    {
        MeshData mesh = new MeshData();

        for (int x = 1; x < 17; x += l)
        {
            for (int z = 1; z < 17; z += l)
            {
                for (int y = 0; y < 255; y++)
                {
                    if (blocks[x, y, z] != BlockType.Air)
                    {
                        Vector3 pos = new Vector3(x - 1, y, z - 1);
                        int numFaces = 0;

                        // Top
                        try { if (y < 254 && blocks[x, y + l, z] == BlockType.Air) { Add(pos, nv(0, l, 0), nv(0, l, l), nv(l, l, l), nv(l, l, 0), x, y, z); numFaces++; } }
                        catch { Add(pos, nv(0, l, 0), nv(0, l, l), nv(l, l, l), nv(l, l, 0), x, y, z); numFaces++; }

                        // Bottom
                        try { if (y > 0 && blocks[x, y - l, z] == BlockType.Air) { Add(pos, nv(0, 0, 0), nv(l, 0, 0), nv(l, 0, l), nv(0, 0, l), x, y, z); numFaces++; } }
                        catch { Add(pos, nv(0, 0, 0), nv(l, 0, 0), nv(l, 0, l), nv(0, 0, l), x, y, z); numFaces++; }

                        // Front
                        try { if (blocks[x, y, z - l] == BlockType.Air) { Add(pos, nv(0, 0, 0), nv(0, l, 0), nv(l, l, 0), nv(l, 0, 0), x, y, z); numFaces++; } }
                        catch { Add(pos, nv(0, 0, 0), nv(0, l, 0), nv(l, l, 0), nv(l, 0, 0), x, y, z); numFaces++; }

                        // Right
                        try { if (blocks[x + l, y, z] == BlockType.Air) { Add(pos, nv(l, 0, 0), nv(l, l, 0), nv(l, l, l), nv(l, 0, l), x, y, z); numFaces++; } }
                        catch { Add(pos, nv(l, 0, 0), nv(l, l, 0), nv(l, l, l), nv(l, 0, l), x, y, z); numFaces++; }

                        // Back
                        try { if (blocks[x, y, z + l] == BlockType.Air) { Add(pos, nv(l, 0, l), nv(l, l, l), nv(0, l, l), nv(0, 0, l), x, y, z); numFaces++; } }
                        catch { Add(pos, nv(l, 0, l), nv(l, l, l), nv(0, l, l), nv(0, 0, l), x, y, z); numFaces++; }

                        // Left
                        try { if (blocks[x - l, y, z] == BlockType.Air) { Add(pos, nv(0, 0, l), nv(0, l, l), nv(0, l, 0), nv(0, 0, 0), x, y, z); numFaces++; } }
                        catch { Add(pos, nv(0, 0, l), nv(0, l, l), nv(0, l, 0), nv(0, 0, 0), x, y, z); numFaces++; }


                        int tl = mesh.verts.Count - 4 * numFaces;
                        for (int i = 0; i < numFaces; i++) { mesh.tris.AddRange(new int[] { tl + i * 4, tl + i * 4 + 1, tl + i * 4 + 2, tl + i * 4, tl + i * 4 + 2, tl + i * 4 + 3 }); }
                    }
                }
            }
        }

        Vector3 nv(float x, float y, float z) { return new Vector3(x, y, z); }

        void Add(Vector3 blockPos, Vector3 one, Vector3 two, Vector3 three, Vector3 four, int x, int y, int z)
        {
            mesh.verts.Add(blockPos + one);
            mesh.verts.Add(blockPos + two);
            mesh.verts.Add(blockPos + three);
            mesh.verts.Add(blockPos + four);

            mesh.uvs.AddRange(Block.blocks[blocks[x, y, z]].GetUVs());
        }

        return mesh;
    }

}

public class MeshData
{
    public List<Vector3> verts = new List<Vector3>();
    public List<int> tris = new List<int>();
    public List<Vector2> uvs = new List<Vector2>();

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        return mesh;
    }
}