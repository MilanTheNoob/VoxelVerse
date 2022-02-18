using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The scriptable object responsible for all major terrain generation features
/// </summary>
[CreateAssetMenu()]
public class GenerateData : ScriptableObject
{
    [Header("The biomes relating to the structures, trees, etc")] public BiomeClass[] Biomes;
    [Header("The noise map that determines the biomes above ^")] public NoiseSettings BiomeNoise;

    [Space, Header("The heightmap biomes")] public BiomeNoiseClass[] Heightmaps;
    [Header("The noise map the determines the heightmap biomes above")] public NoiseSettings HeightmapsNoise;

    [Space, Header("Is this object used for the test world?")] public bool TestWorld;

    [HideInInspector] public NoiseSettings VaryingNoise = new NoiseSettings()
    { scale = 30, height = 1, octaves = 1, persistance = 0.6f, lacunarity = 2f };

    /// <summary>
    /// Returns a 3D heightmap of blocks using a random num generator and coordinates
    /// </summary>
    /// <param name="coord">The coordinates of the chunk</param>
    /// <param name="rand">The random number generator</param>
    /// <returns>The 3D map of blocks</returns>
    public BlockType[,,] GenerateChunk(Vector2Int coord, System.Random rand)
    {
        Vector2Int sampleCenter = coord * 16;

        float[,] map = Noise.GenerateHeightMap(Heightmaps, HeightmapsNoise, sampleCenter, 0).Heightmap;
        BlockType[,,] blocks = new BlockType[18, 255, 18];

        for (int x = 0; x < 18; x++)
        {
            for (int z = 0; z < 18; z++)
            {
                int height = Mathf.CeilToInt(map[x, z]);
                for (int y = 0; y <= height; y++) { blocks[x, y, z] = BlockType.Grass; }
            }
        }

        float temp = Noise.SingleNoise(BiomeNoise, 0, coord.x, coord.y);
        int biome = 0;

        for (int i = 0; i < Biomes.Length; i++)
            if (temp > Biomes[i].MinTemp && temp <= Biomes[i].MaxTemp) { biome = i; break; }

        if (!TestWorld)
        {
            Structures(coord, Biomes[biome].Trees, map, ref blocks, rand);
            Structures(coord, Biomes[biome].Rocks, map, ref blocks, rand);
        }

        if (TerrainGenerator.ChunkChanges.ContainsKey(coord))
        {
            List<StructureBlockClass> blocksA = TerrainGenerator.ChunkChanges[coord];
            for (int i = 0; i < blocksA.Count; i++) blocks[blocksA[i].Pos.x, blocksA[i].Pos.y, blocksA[i].Pos.z] = blocksA[i].Block;
        }

        return blocks;
    }

    public void Structures(Vector2Int coord, List<StructureClass> structs, float[,] heightmap, ref BlockType[,,] blocks, System.Random random)
    {
        ThreadedManager.ExecuteOnMainThread(() =>
            { 
            });
        Dictionary<Vector2Int, List<StructureBlockClass>> existingChanges = new Dictionary<Vector2Int, List<StructureBlockClass>>();
        Dictionary<Vector2Int, List<StructureBlockClass>> newChanges = new Dictionary<Vector2Int, List<StructureBlockClass>>();

        for (int i = 0; i < structs.Count; i++)
        {
            if (random.Next(0, 100) < structs[i].chance)
            {
                for (int j = 0; j < random.Next(structs[i].MinAmount, structs[i].MaxAmount); j++)
                {
                    Vector3Int start = new Vector3Int(random.Next(0, 15), 0, random.Next(0, 15));

                    start.y = (int)heightmap[start.x, start.z] + 1;
                    //start = TerrainGenerator.GetGlobalPos(start, coord);

                    int x = random.Next(0, structs[i].Variants.Count);
                    for (int m = 0; m < structs[i].Variants[x].Blocks.Count; m++)
                    {
                        Vector3Int bPos = structs[i].Variants[x].Blocks[m].Pos;
                        Vector3Int pos = new Vector3Int(start.x + bPos.x + 1, start.y + bPos.y, start.z + bPos.z + 1);

                        Vector2Int blockChunk = new Vector2Int(Mathf.FloorToInt((pos.x - 1) / 16f), Mathf.FloorToInt((pos.z - 1) / 16f));
                        Vector2Int sideChunk = blockChunk + coord;

                        Vector3Int localPos = new Vector3Int(pos.x - blockChunk.x * 16, pos.y, pos.z - blockChunk.y * 16);

                        if (blockChunk == Vector2Int.zero) blocks[pos.x, pos.y, pos.z] = structs[i].Variants[x].Blocks[m].Block;
                        else if (TerrainGenerator.Chunks.ContainsKey(sideChunk))
                        {

                            if (TerrainGenerator.Chunks[sideChunk].lodMeshes[0].hasMesh)
                            {
                                if (!existingChanges.ContainsKey(sideChunk)) { existingChanges.Add(sideChunk, new List<StructureBlockClass>()); }
                                existingChanges[sideChunk].Add(new StructureBlockClass() { Block = structs[i].Variants[x].Blocks[m].Block, Pos = localPos });
                            }
                            else
                            {
                                if (!newChanges.ContainsKey(sideChunk)) { newChanges.Add(sideChunk, new List<StructureBlockClass>()); }
                                newChanges[sideChunk].Add(new StructureBlockClass() { Block = structs[i].Variants[x].Blocks[m].Block, Pos = localPos });
                            }
                        }
                        else
                        {
                            if (!newChanges.ContainsKey(sideChunk)) { newChanges.Add(sideChunk, new List<StructureBlockClass>()); }
                            newChanges[sideChunk].Add(new StructureBlockClass() { Block = structs[i].Variants[x].Blocks[m].Block, Pos = localPos });
                        }
                    }
                }
            }

        }

            for (int i = 0; i < newChanges.Count; i++)
            {
                Vector2Int loc = newChanges.ElementAt(i).Key;

                if (!TerrainGenerator.ChunkChanges.ContainsKey(loc)) { TerrainGenerator.ChunkChanges.Add(loc, new List<StructureBlockClass>()); }
                TerrainGenerator.ChunkChanges[loc].AddRange(newChanges[loc]);
            }

            for (int i = 0; i < existingChanges.Count; i++)
            {
                Vector2Int loc = existingChanges.ElementAt(i).Key;
                TerrainChunk chunk = TerrainGenerator.Chunks[loc];

                for (int j = 0; j < existingChanges[loc].Count; j++)
                {
                    Vector3Int blockPos = existingChanges[loc][j].Pos;
                    TerrainGenerator.Chunks[loc].heightMap[blockPos.x, blockPos.y, blockPos.z] = existingChanges[loc][j].Block;
                }

                chunk.lodMeshes[chunk.previousLODIndex].RequestMesh(chunk.heightMap, chunk.coord);
            }
    }
}

[System.Serializable]
public class BiomeClass
{
    public string Name;

    [Space]

    public List<StructureClass> Rocks;
    public List<StructureClass> Trees;
    public List<StructureClass> Foliage;

    [Space]

    public List<StructureClass> Structures;

    [Space]

    public float MinTemp;
    public float MaxTemp;
}

[System.Serializable]
public class StructureClass
{
    public string Name;

    [Space]

    public int MinAmount;
    public int MaxAmount;

    [Space, Tooltip("Out of 100")]
    public int chance = 100;

    [Space]

    public List<StructureObj> Variants;
}

[System.Serializable]
public class StructureBlockClass
{
    public BlockType Block;
    public Vector3Int Pos;
}

[System.Serializable]
public class BiomeNoiseClass
{
    public NoiseSettings[] Noise;
}