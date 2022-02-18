using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// A moderately simple class responsible for creating structures in the flat world
/// NOTE : Needs rework due to difficult movement
/// </summary>
public class StructureCreator : MonoBehaviour
{
    public LayerMask groundLayer;
    public TextMeshProUGUI coordText;

    public List<StructureBlockClass> blocks = new List<StructureBlockClass>();

    void Update()
    {
        bool leftClick = Input.GetMouseButtonDown(0);
        bool rightClick = Input.GetMouseButtonDown(1);

        RaycastHit hitInfo;
        Vector3 point;

        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, 8, groundLayer))
        {
            if (leftClick) point = hitInfo.point + transform.forward * .01f;
            else point = hitInfo.point - transform.forward * .01f;

            Vector3Int pointRounded = new Vector3Int(Mathf.FloorToInt(point.x) + 1, Mathf.FloorToInt(point.y) - 2, Mathf.FloorToInt(point.z) + 1);
            coordText.text = "Looking at: " + pointRounded.x + ", " + pointRounded.y + ", " + pointRounded.z;

            if ((leftClick || rightClick) && !FlatWorldManager.paused)
            {
                int chunkPosX = Mathf.FloorToInt(point.x / 16f);
                int chunkPosZ = Mathf.FloorToInt(point.z / 16f);

                TerrainChunk chunk = TerrainGenerator.Chunks[new Vector2Int(chunkPosX, chunkPosZ)];

                int bix = Mathf.FloorToInt(point.x) - (chunkPosX * 16) + 1;
                int biy = Mathf.FloorToInt(point.y);
                int biz = Mathf.FloorToInt(point.z) - (chunkPosZ * 16) + 1;

                if (leftClick)
                {
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if (blocks[i].Pos == pointRounded)
                        {
                            chunk.heightMap[bix, biy, biz] = BlockType.Air;
                            chunk.lodMeshes[chunk.previousLODIndex].RequestMesh(chunk.heightMap, chunk.coord);

                            blocks.RemoveAt(i);
                            break;
                        }
                    }
                }
                else if (rightClick && FlatWorldManager.CurrentBlock != BlockType.Air)
                {
                    chunk.heightMap[bix, biy, biz] = FlatWorldManager.CurrentBlock;
                    chunk.lodMeshes[chunk.previousLODIndex].RequestMesh(chunk.heightMap, chunk.coord);

                    blocks.Add(new StructureBlockClass()
                    {
                        Pos = pointRounded,
                        Block = FlatWorldManager.CurrentBlock
                    });
                }
            }
        }
    }
}
