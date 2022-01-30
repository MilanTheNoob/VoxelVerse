using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu()]
public class GenerateData : ScriptableObject
{
    public BiomeClass[] Biomes;
    public float TemperatureScale;

    [Space]

    public bool TestWorld;
    public BiomeNoiseClass[] Heightmaps;
    public NoiseSettings BiomeNoise;

    public BlockType[,,] GenerateChunk(Vector2Int coord)
    {
        Vector2Int sampleCentre = coord * 16;

        float[,] heightmap = Noise.GenerateHeightMap(Heightmaps, BiomeNoise, new Vector2(sampleCentre.x, sampleCentre.y), 0);
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
        /*
        float temperature = Mathf.PerlinNoise(coord.x / TemperatureScale, coord.y / TemperatureScale);

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
            blocks = GenerateStructures(sampleCentre, coord, Biomes[biome].Trees, heightmap, blocks);
            blocks = GenerateStructures(sampleCentre, coord, Biomes[biome].Rocks, heightmap, blocks);
        }

        if (TerrainGenerator.ChunkChanges.ContainsKey(coord))
        {
            List<StructureBlockClass> blocksToAdd = TerrainGenerator.ChunkChanges[coord].BlocksToChange;
            for (int i = 0; i < blocksToAdd.Count; i++) blocks[blocksToAdd[i].Pos.x, blocksToAdd[i].Pos.y, blocksToAdd[i].Pos.z] = blocksToAdd[i].Block;
        }
        */
        return blocks;
    }

    public BlockType[,,] GenerateStructures(Vector2Int sampleCenter, Vector2Int coord, List<StructureClass> structures, float[,] heightmap, BlockType[,,] blocks)
    {
        List<Vector2Int> affectedChunks = new List<Vector2Int>();

        for (int i = 0; i < structures.Count; i++)
        {
            if (Random.Range(0, 100) < structures[i].chance)
            {
                for (int j = 0; j < Random.Range(structures[i].MinAmount, structures[i].MaxAmount); j++)
                {
                    Vector3Int startPos = new Vector3Int(Random.Range(0, 15), 0, Random.Range(0, 15));
                    
                    startPos.y = (int)heightmap[startPos.x, startPos.z] + 1;
                    startPos = TerrainGenerator.GetGlobalPos(startPos, coord);

                    int variant = Random.Range(0, structures[i].Variants.Count);
                    for (int m = 0; m < structures[i].Variants[variant].Blocks.Count; m++)
                    {
                        Vector3Int blockPos = new Vector3Int(startPos.x + structures[i].Variants[variant].Blocks[m].Pos.x, startPos.y + 
                            structures[i].Variants[variant].Blocks[m].Pos.y, startPos.z + structures[i].Variants[variant].Blocks[m].Pos.z);
                        Vector2Int blockChunk = new Vector2Int(Mathf.FloorToInt(blockPos.x / 16f), Mathf.FloorToInt(blockPos.z / 16f));

                        Vector3Int chunkBlockPos = new Vector3Int(blockPos.x - (blockChunk.x * 16), blockPos.y, blockPos.z - (blockChunk.y * 16));

                        if (TerrainGenerator.chunks.ContainsKey(blockChunk))
                        {
                            TerrainGenerator.chunks[blockChunk].blocks[chunkBlockPos.x + 1, chunkBlockPos.y, chunkBlockPos.z + 1] = structures[i].Variants[variant].Blocks[m].Block;
                            if (!affectedChunks.Contains(blockChunk)) affectedChunks.Add(blockChunk);
                        }
                        else if (blockChunk == coord)
                        {
                            blocks[chunkBlockPos.x + 1, chunkBlockPos.y, chunkBlockPos.z + 1] = structures[i].Variants[variant].Blocks[m].Block;
                        }
                        else
                        {
                            if (!TerrainGenerator.ChunkChanges.ContainsKey(blockChunk)) { TerrainGenerator.ChunkChanges.Add(blockChunk, new ChunkChangesClass()); }

                            TerrainGenerator.ChunkChanges[blockChunk].BlocksToChange.Add(new StructureBlockClass()
                            { Block = structures[i].Variants[variant].Blocks[m].Block, Pos = new Vector3Int(chunkBlockPos.x + 1, chunkBlockPos.y, chunkBlockPos.z + 1) });
                        }

                    }
                }
            }
        }

        for (int i = 0; i < affectedChunks.Count; i++) { TerrainGenerator.chunks[affectedChunks[i]].BuildMesh(); }

        return blocks;
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