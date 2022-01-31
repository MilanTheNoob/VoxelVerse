using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu()]
public class GenerateData : ScriptableObject
{
    public BiomeClass[] Biomes;
    public NoiseSettings BiomeNoise;

    [Space]

    public bool TestWorld;
    public BiomeNoiseClass[] Heightmaps;
    public NoiseSettings HeightmapsNoise;

    public BlockType[,,] GenerateChunk(Vector2Int coord, System.Random rand)
    {
        Vector2Int sampleCentre = coord * 16;

        float[,] heightmap = Noise.GenerateHeightMap(Heightmaps, HeightmapsNoise, new Vector2(sampleCentre.x, sampleCentre.y), 0).Heightmap;
        BlockType[,,] blocks = new BlockType[18, 255, 18];

        for (int x = 0; x < 18; x++) for (int z = 0; z < 18; z++) for (int y = 0; y <= Mathf.CeilToInt(heightmap[x, z]); y++)
                {
                    if (TestWorld)
                    {
                        if (x == 0 && z == 0) blocks[x, y, z] = BlockType.Dirt;
                        else { blocks[x, y, z] = BlockType.Spacer; }
                    }
                    else
                    {
                        float choose = Mathf.PerlinNoise((sampleCentre.x + x) / 10, (sampleCentre.y + z) / 10);

                        if (choose < 0.2f) { blocks[x, y, z] = BlockType.Stone; }
                        else if (choose < 0.8f) { blocks[x, y, z] = BlockType.Grass; }
                        else { blocks[x, y, z] = BlockType.Dirt; }
                    }
                }

        float temperature = Noise.SingleNoise(BiomeNoise, 0, coord.x, coord.y);

        int biome = 0;
        for (int i = 0; i < Biomes.Length; i++)
        {
            if (temperature > Biomes[i].MinTemperature && temperature <= Biomes[i].MaxTemperature)
            {
                biome = i;
                break;
            }
        }

        if (!TestWorld)
        {
            GenerateStructures(sampleCentre, coord, Biomes[biome].Trees, heightmap, ref blocks, rand);
            GenerateStructures(sampleCentre, coord, Biomes[biome].Rocks, heightmap, ref blocks, rand);
        }

        return blocks;
    }

    public void GenerateStructures(Vector2Int sampleCenter, Vector2Int coord, List<StructureClass> structures, float[,] heightmap, ref BlockType[,,] blocks, System.Random random)
    {
        List<Vector2Int> affectedChunks = new List<Vector2Int>();

        for (int i = 0; i < structures.Count; i++)
        {
            if (random.Next(0, 100) < structures[i].chance)
            {
                for (int j = 0; j < random.Next(structures[i].MinAmount, structures[i].MaxAmount); j++)
                {
                    Vector3Int startPos = new Vector3Int(random.Next(0, 15), 0, random.Next(0, 15));
                    
                    startPos.y = (int)heightmap[startPos.x, startPos.z] + 1;
                    startPos = TerrainGenerator.GetGlobalPos(startPos, coord);

                    int variant = random.Next(0, structures[i].Variants.Count);
                    for (int m = 0; m < structures[i].Variants[variant].Blocks.Count; m++)
                    {
                        Vector3Int blockPos = new Vector3Int(startPos.x + structures[i].Variants[variant].Blocks[m].Pos.x, startPos.y + 
                            structures[i].Variants[variant].Blocks[m].Pos.y, startPos.z + structures[i].Variants[variant].Blocks[m].Pos.z);
                        Vector2Int blockChunk = new Vector2Int(Mathf.FloorToInt(blockPos.x / 16f), Mathf.FloorToInt(blockPos.z / 16f));

                        Vector3Int chunkBlockPos = new Vector3Int(blockPos.x - (blockChunk.x * 16), blockPos.y, blockPos.z - (blockChunk.y * 16));

                        if (blockChunk == coord)
                        {
                            blocks[chunkBlockPos.x + 1, chunkBlockPos.y, chunkBlockPos.z + 1] = structures[i].Variants[variant].Blocks[m].Block;
                        }
                        else if (TerrainGenerator.terrainChunkDictionary.ContainsKey(blockChunk))
                        {
                            if (TerrainGenerator.terrainChunkDictionary[blockChunk].lodMeshes[0].hasMesh)
                            {
                                //TerrainGenerator.terrainChunkDictionary[blockChunk].heightMap[chunkBlockPos.x + 1, chunkBlockPos.y, chunkBlockPos.z + 1] = structures[i].Variants[variant].Blocks[m].Block;
                                if (!affectedChunks.Contains(blockChunk)) affectedChunks.Add(blockChunk);
                            }
                            else
                            {
                                if (!TerrainGenerator.ChunkChanges.ContainsKey(blockChunk)) { TerrainGenerator.ChunkChanges.Add(blockChunk, new ChunkChangesClass()); }

                                try
                                {
                                    TerrainGenerator.ChunkChanges[blockChunk].BlocksToChange.Add(new StructureBlockClass()
                                    { Block = structures[i].Variants[variant].Blocks[m].Block, Pos = new Vector3Int(chunkBlockPos.x + 1, chunkBlockPos.y, chunkBlockPos.z + 1) });
                                }
                                catch { }
                            }
                        }
                        else
                        {
                            if (!TerrainGenerator.ChunkChanges.ContainsKey(blockChunk)) { TerrainGenerator.ChunkChanges.Add(blockChunk, new ChunkChangesClass()); }

                            try
                            {
                                TerrainGenerator.ChunkChanges[blockChunk].BlocksToChange.Add(new StructureBlockClass()
                                { Block = structures[i].Variants[variant].Blocks[m].Block, Pos = new Vector3Int(chunkBlockPos.x + 1, chunkBlockPos.y, chunkBlockPos.z + 1) });
                            } catch { }
                        }

                    }
                }
            }
        }

        for (int i = 0; i < affectedChunks.Count; i++) { TerrainGenerator.terrainChunkDictionary[affectedChunks[i]].lodMeshes[0].RequestMesh
                (TerrainGenerator.terrainChunkDictionary[affectedChunks[i]].heightMap, TerrainGenerator.terrainChunkDictionary[affectedChunks[i]].coord); }
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

    public float MinTemperature;
    public float MaxTemperature;
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