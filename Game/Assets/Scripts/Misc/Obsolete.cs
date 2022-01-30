using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Obsolete
{
    //WaterChunk wat = chunk.transform.GetComponentInChildren<WaterChunk>();
    //wat.SetLocs(chunk.blocks);
    //wat.BuildMesh();

    /*
     * void GenerateTrees(BlockType[,,] blocks, int x, int z)
    {
        /*
        System.Random rand = new System.Random(x * 10000 + z);

        float simplex = noise.GetSimplex(x * .8f, z * .8f);

        if(simplex > 0)
        {
            simplex *= 2f;
            int treeCount = Mathf.FloorToInt((float)rand.NextDouble() * 5 * simplex);

            for(int i = 0; i < treeCount; i++)
            {
                int xPos = (int)(rand.NextDouble() * 14) + 1;
                int zPos = (int)(rand.NextDouble() * 14) + 1;

                int y = TerrainChunk.chunkHeight - 1;
                //find the ground
                while(y > 0 && blocks[xPos, y, zPos] == BlockType.Air)
                {
                    y--;
                }
                y++;

                int treeHeight = 4 + (int)(rand.NextDouble() * 4);

                for(int j = 0; j < treeHeight; j++)
                {
                    if(y+j < 64)
                        blocks[xPos, y+j, zPos] = BlockType.Trunk;
                }

                int leavesWidth = 1 + (int)(rand.NextDouble() * 6);
                int leavesHeight = (int)(rand.NextDouble() * 3);

                int iter = 0;
                for(int m = y + treeHeight - 1; m <= y + treeHeight - 1 + treeHeight; m++)
                {
                    for(int k = xPos - (int)(leavesWidth * .5)+iter/2; k <= xPos + (int)(leavesWidth * .5)-iter/2; k++)
                        for(int l = zPos - (int)(leavesWidth * .5)+iter/2; l <= zPos + (int)(leavesWidth * .5)-iter/2; l++)
                        {
                            if(k >= 0 && k < 16 && l >= 0 && l < 16 && m >= 0 && m < 64 && rand.NextDouble() < .8f)
                                blocks[k, m, l] = BlockType.Leaves;
                        }

                    iter++;
                }


            }
        }
}
    BlockType GetBlockType(int x, int y, int z)
    {
        float heightMap =  noise.GetSimplex(x / 10, z / 10) * 1000;

        //add the 2d noise to the middle of the terrain chunk
        float baseLandHeight = TerrainChunk.chunkHeight * .5f + heightMap;

        //3d noise for caves and overhangs and such
        float caveNoise1 = noise.GetPerlinFractal(x*5f, y*10f, z*5f);
        float caveMask = noise.GetSimplex(x * .3f, z * .3f)+.3f;

        //stone layer heightmap
        float simplexStone1 = noise.GetSimplex(x * 1f, z * 1f) * 10;
        float simplexStone2 = (noise.GetSimplex(x * 5f, z * 5f)+.5f) * 20 * (noise.GetSimplex(x * .3f, z * .3f) + .5f);

        float stoneHeightMap = simplexStone1 + simplexStone2;
        float baseStoneHeight = TerrainChunk.chunkHeight * .25f + stoneHeightMap;


        //float cliffThing = noise.GetSimplex(x * 1f, z * 1f, y) * 10;
        //float cliffThingMask = noise.GetSimplex(x * .4f, z * .4f) + .3f;



        BlockType blockType = BlockType.Air;

        //under the surface, dirt block
        if(y <= baseLandHeight)
        {
            blockType = BlockType.Dirt;

            //just on the surface, use a grass type
            if(y > baseLandHeight - 1 && y > WaterChunk.waterHeight-2)
                blockType = BlockType.Grass;

            if(y <= baseStoneHeight)
                blockType = BlockType.Stone;
        }


        if(caveNoise1 > Mathf.Max(caveMask, .2f))
            blockType = BlockType.Air;

        /*if(blockType != BlockType.Air)
            blockType = BlockType.Stone;

        //if(blockType == BlockType.Air && noise.GetSimplex(x * 4f, y * 4f, z*4f) < 0)
          //  blockType = BlockType.Dirt;

        //if(Mathf.PerlinNoise(x * .1f, z * .1f) * 10 + y < TerrainChunk.chunkHeight * .5f)
        //    return BlockType.Grass;

        return blockType;

        return BlockType.Air;
    }

*/
}
