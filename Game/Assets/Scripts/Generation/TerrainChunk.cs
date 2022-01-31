/*

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    [HideInInspector] public Vector2Int Coord;
    [HideInInspector] public Vector2Int SampleCentre;

    [HideInInspector] public int lod = 1;

    [HideInInspector] public MeshFilter meshFilter;
    [HideInInspector] public MeshCollider meshCollider;

    public BlockType[,,] blocks = new BlockType[18, 255, 18];
    GenerateData gd;

    public void BuildMesh()
    {
        Mesh mesh = new Mesh();

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        
        for (int x = 1; x < 17; x += lod)
        {
            for (int z = 1; z < 17; z += lod)
            {
                for (int y = 0; y < 255; y++)
                {
                    if (blocks[x, y, z] != BlockType.Air)
                    {
                        Vector3 blockPos = new Vector3(x - 1, y, z - 1);
                        int numFaces = 0;

                        // Top
                        try
                        {
                            if (y < 254 && blocks[x, y + lod, z] == BlockType.Air)
                            {
                                AddToVerts(blockPos, nv(0, lod, 0), nv(0, lod, lod), nv(lod, lod, lod), nv(lod, lod, 0), x, y, z);
                                numFaces++;
                            }
                        }
                        catch
                        {
                            AddToVerts(blockPos, nv(0, lod, 0), nv(0, lod, lod), nv(lod, lod, lod), nv(lod, lod, 0), x, y, z);
                            numFaces++;
                        }

                        // Bottom
                        try
                        {
                            if (y > 0 && blocks[x, y - lod, z] == BlockType.Air)
                            {
                                AddToVerts(blockPos, nv(0, 0, 0), nv(lod, 0, 0), nv(lod, 0, lod), nv(0, 0, lod), x, y, z);
                                numFaces++;
                            }
                        }
                        catch
                        {
                            AddToVerts(blockPos, nv(0, 0, 0), nv(lod, 0, 0), nv(lod, 0, lod), nv(0, 0, lod), x, y, z);
                            numFaces++;
                        }

                        // Front
                        try
                        {
                            if (blocks[x, y, z - lod] == BlockType.Air)
                            {
                                AddToVerts(blockPos, nv(0, 0, 0), nv(0, lod, 0), nv(lod, lod, 0), nv(lod, 0, 0), x, y, z);
                                numFaces++;
                            }
                        }
                        catch
                        {
                            AddToVerts(blockPos, nv(0, 0, 0), nv(0, lod, 0), nv(lod, lod, 0), nv(lod, 0, 0), x, y, z);
                            numFaces++;
                        }

                        // Right
                        try
                        {
                            if (blocks[x + lod, y, z] == BlockType.Air)
                            {
                                AddToVerts(blockPos, nv(lod, 0, 0), nv(lod, lod, 0), nv(lod, lod, lod), nv(lod, 0, lod), x, y, z);
                                numFaces++;
                            }
                        }
                        catch
                        {
                            AddToVerts(blockPos, nv(lod, 0, 0), nv(lod, lod, 0), nv(lod, lod, lod), nv(lod, 0, lod), x, y, z);
                            numFaces++;
                        }

                        // Back
                        try
                        {
                            if (blocks[x, y, z + lod] == BlockType.Air)
                            {
                                AddToVerts(blockPos, nv(lod, 0, lod), nv(lod, lod, lod), nv(0, lod, lod), nv(0, 0, lod), x, y, z);
                                numFaces++;
                            }
                        }
                        catch
                        {
                            AddToVerts(blockPos, nv(lod, 0, lod), nv(lod, lod, lod), nv(0, lod, lod), nv(0, 0, lod), x, y, z);
                            numFaces++;
                        }

                        // Left
                        try
                        {
                            if (blocks[x - lod, y, z] == BlockType.Air)
                            {
                                AddToVerts(blockPos, nv(0, 0, lod), nv(0, lod, lod), nv(0, lod, 0), nv(0, 0, 0), x, y, z);
                                numFaces++;
                            }
                        }
                        catch
                        {
                            AddToVerts(blockPos, nv(0, 0, lod), nv(0, lod, lod), nv(0, lod, 0), nv(0, 0, 0), x, y, z);
                            numFaces++;
                        }


                        int tl = verts.Count - 4 * numFaces;
                        for (int i = 0; i < numFaces; i++)
                        {
                            tris.AddRange(new int[] { tl + i * 4, tl + i * 4 + 1, tl + i * 4 + 2, tl + i * 4, tl + i * 4 + 2, tl + i * 4 + 3 });
                        }
                    }
                }
            }
        }
        
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.name = transform.position.x + ", " + transform.position.z;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        
        Vector3 nv(float x, float y, float z) { return new Vector3(x, y, z); }

        void AddToVerts(Vector3 blockPos, Vector3 one, Vector3 two, Vector3 three, Vector3 four, int x, int y, int z)
        {
            verts.Add(blockPos + one);
            verts.Add(blockPos + two);
            verts.Add(blockPos + three);
            verts.Add(blockPos + four);

            uvs.AddRange(Block.blocks[blocks[x, y, z]].GetUVs());
        }
    }

    public void UpdateLod(int lod)
    {
        if (lod != this.lod)
        {
            this.lod = lod;
            BuildMesh();
        }
    }

    public void ResetBlocks() { blocks = new BlockType[18, 255, 18]; }

    public void SetupChunk(int lod, Vector2Int coord, GenerateData gd) 
    { 
        meshFilter = GetComponent<MeshFilter>(); 
        meshCollider = GetComponent<MeshCollider>(); 
        
        this.lod = lod;
        this.Coord = coord;
        this.SampleCentre = coord * 16;
        this.gd = gd;

        if (SavingManager.ActiveSave.Chunks.ContainsKey(coord))
        {
            blocks = SavingManager.ActiveSave.Chunks[coord];
            FinishChunkSetup();
        }
        else
        {
            ThreadedManager.RequestData(() => Noise.GenerateHeightMap(gd.Heightmaps, gd.HeightmapsNoise, SampleCentre, 0), OnHeightMapReceive);
        }
    }

    public void OnHeightMapReceive(object objectHeightmap)
    {
        float[,] heightmap = ((HeightmapData)objectHeightmap).Heightmap;

        for (int x = 0; x < 18; x++)
        {
            for (int z = 0; z < 18; z++)
            {
                float choose = Mathf.PerlinNoise((SampleCentre.x + x) / 10, (SampleCentre.y + z) / 10);

                for (int y = 0; y <= Mathf.CeilToInt(heightmap[x, z]); y++)
                {
                    if (gd.TestWorld)
                    {
                        if (x == 0 && z == 0) blocks[x, y, z] = BlockType.Dirt;
                        else { blocks[x, y, z] = BlockType.Spacer; }
                    }
                    else
                    {
                        if (choose < 0.2f) { blocks[x, y, z] = BlockType.Stone; }
                        else if (choose < 0.8f) { blocks[x, y, z] = BlockType.Grass; }
                        else { blocks[x, y, z] = BlockType.Dirt; }
                    }
                }
            }
        }

        float temperature = Noise.SingleNoise(gd.BiomeNoise, 0, Coord.x, Coord.y);
        int biome = 0;
        
        for (int i = 0; i < gd.Biomes.Length; i++)
        {
            if (temperature > gd.Biomes[i].MinTemperature && temperature <= gd.Biomes[i].MaxTemperature)
            {
                biome = i;
                break;
            }
        }

        if (!gd.TestWorld)
        {
            gd.GenerateStructures(SampleCentre, Coord, gd.Biomes[biome].Trees, heightmap, ref blocks);
            gd.GenerateStructures(SampleCentre, Coord, gd.Biomes[biome].Rocks, heightmap, ref blocks);
        }

        SavingManager.ActiveSave.Chunks.Add(Coord, blocks);
        FinishChunkSetup();
    }

    void FinishChunkSetup()
    {
        if (TerrainGenerator.ChunkChanges.ContainsKey(Coord))
        {
            List<StructureBlockClass> blocksToAdd = TerrainGenerator.ChunkChanges[Coord].BlocksToChange;
            for (int i = 0; i < blocksToAdd.Count; i++) blocks[blocksToAdd[i].Pos.x, blocksToAdd[i].Pos.y, blocksToAdd[i].Pos.z] = blocksToAdd[i].Block;
            TerrainGenerator.ChunkChanges.Remove(Coord);
        }

        transform.position = new Vector3(SampleCentre.x, 0, SampleCentre.y);
        transform.name = "Chunk " + Coord;

        BuildMesh();
    }
}
*/

using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
    const float colliderGenerationDistanceThreshold = 5;
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

    public TerrainChunk(Vector2Int coord, GenerateData gd, LODClass[] detailLevels, Transform parent, Transform viewer, Material material)
    {
        this.coord = coord;
        this.detailLevels = detailLevels;
        this.gd = gd;
        this.viewer = viewer;

        sampleCentre = coord * 16;
        bounds = new Bounds(sampleCentre, Vector2.one);


        meshObject = new GameObject("Terrain Chunk");
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
        if (SavingManager.ActiveSave.Chunks.ContainsKey(coord))
        {
            heightMap = SavingManager.ActiveSave.Chunks[coord];
            OnHeightMapReceived((object)heightMap);
        }
        else
        {
            ThreadedManager.RequestData(() => gd.GenerateChunk(coord, TerrainGenerator.rand), OnHeightMapReceived);
        }
    }



    void OnHeightMapReceived(object heightMapObject)
    {
        this.heightMap = (BlockType[,,])heightMapObject;
        heightMapReceived = true;

        if (!SavingManager.ActiveSave.Chunks.ContainsKey(coord)) SavingManager.ActiveSave.Chunks.Add(coord, heightMap);
        UpdateTerrainChunk();
    }

    Vector2 viewerPosition
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
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

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

    MeshData GenerateMesh(BlockType[,,] blocks, int lod, Vector2Int coord)
    {
        MeshData mesh = new MeshData();

        if (TerrainGenerator.ChunkChanges.ContainsKey(coord))
        {
            try
            {
                List<StructureBlockClass> blocksToAdd = TerrainGenerator.ChunkChanges[coord].BlocksToChange;
                for (int i = 0; i < blocksToAdd.Count; i++) blocks[blocksToAdd[i].Pos.x, blocksToAdd[i].Pos.y, blocksToAdd[i].Pos.z] = blocksToAdd[i].Block;
            } catch { }
        }

        for (int x = 1; x < 17; x += lod)
        {
            for (int z = 1; z < 17; z += lod)
            {
                for (int y = 0; y < 255; y++)
                {
                    if (blocks[x, y, z] != BlockType.Air)
                    {
                        Vector3 blockPos = new Vector3(x - 1, y, z - 1);
                        int numFaces = 0;

                        // Top
                        try
                        {
                            if (y < 254 && blocks[x, y + lod, z] == BlockType.Air)
                            {
                                AddToVerts(blockPos, nv(0, lod, 0), nv(0, lod, lod), nv(lod, lod, lod), nv(lod, lod, 0), x, y, z);
                                numFaces++;
                            }
                        }
                        catch
                        {
                            AddToVerts(blockPos, nv(0, lod, 0), nv(0, lod, lod), nv(lod, lod, lod), nv(lod, lod, 0), x, y, z);
                            numFaces++;
                        }

                        // Bottom
                        try
                        {
                            if (y > 0 && blocks[x, y - lod, z] == BlockType.Air)
                            {
                                AddToVerts(blockPos, nv(0, 0, 0), nv(lod, 0, 0), nv(lod, 0, lod), nv(0, 0, lod), x, y, z);
                                numFaces++;
                            }
                        }
                        catch
                        {
                            AddToVerts(blockPos, nv(0, 0, 0), nv(lod, 0, 0), nv(lod, 0, lod), nv(0, 0, lod), x, y, z);
                            numFaces++;
                        }

                        // Front
                        try
                        {
                            if (blocks[x, y, z - lod] == BlockType.Air)
                            {
                                AddToVerts(blockPos, nv(0, 0, 0), nv(0, lod, 0), nv(lod, lod, 0), nv(lod, 0, 0), x, y, z);
                                numFaces++;
                            }
                        }
                        catch
                        {
                            AddToVerts(blockPos, nv(0, 0, 0), nv(0, lod, 0), nv(lod, lod, 0), nv(lod, 0, 0), x, y, z);
                            numFaces++;
                        }

                        // Right
                        try
                        {
                            if (blocks[x + lod, y, z] == BlockType.Air)
                            {
                                AddToVerts(blockPos, nv(lod, 0, 0), nv(lod, lod, 0), nv(lod, lod, lod), nv(lod, 0, lod), x, y, z);
                                numFaces++;
                            }
                        }
                        catch
                        {
                            AddToVerts(blockPos, nv(lod, 0, 0), nv(lod, lod, 0), nv(lod, lod, lod), nv(lod, 0, lod), x, y, z);
                            numFaces++;
                        }

                        // Back
                        try
                        {
                            if (blocks[x, y, z + lod] == BlockType.Air)
                            {
                                AddToVerts(blockPos, nv(lod, 0, lod), nv(lod, lod, lod), nv(0, lod, lod), nv(0, 0, lod), x, y, z);
                                numFaces++;
                            }
                        }
                        catch
                        {
                            AddToVerts(blockPos, nv(lod, 0, lod), nv(lod, lod, lod), nv(0, lod, lod), nv(0, 0, lod), x, y, z);
                            numFaces++;
                        }

                        // Left
                        try
                        {
                            if (blocks[x - lod, y, z] == BlockType.Air)
                            {
                                AddToVerts(blockPos, nv(0, 0, lod), nv(0, lod, lod), nv(0, lod, 0), nv(0, 0, 0), x, y, z);
                                numFaces++;
                            }
                        }
                        catch
                        {
                            AddToVerts(blockPos, nv(0, 0, lod), nv(0, lod, lod), nv(0, lod, 0), nv(0, 0, 0), x, y, z);
                            numFaces++;
                        }


                        int tl = mesh.verts.Count - 4 * numFaces;
                        for (int i = 0; i < numFaces; i++)
                        {
                            mesh.tris.AddRange(new int[] { tl + i * 4, tl + i * 4 + 1, tl + i * 4 + 2, tl + i * 4, tl + i * 4 + 2, tl + i * 4 + 3 });
                        }
                    }
                }
            }
        }

        Vector3 nv(float x, float y, float z) { return new Vector3(x, y, z); }

        void AddToVerts(Vector3 blockPos, Vector3 one, Vector3 two, Vector3 three, Vector3 four, int x, int y, int z)
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