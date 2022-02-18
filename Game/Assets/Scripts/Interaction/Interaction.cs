using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Interaction : MonoBehaviour
{
    #region Singleton
    public static Interaction instance;
    void Awake() { instance = this; }
    #endregion

    public LayerMask groundLayer;
    public LayerMask interactableLayer;

    [Space]

    public float interactionDst = 4;

    [Space]

    public Image Cursor;
    public Image BlockProgress;

    Camera cam;

    bool allowBlockBreak = true;
    bool breaking = false;

    Vector3Int breakingBlock;
    Vector3Int lookingBlock;

    void Start()
    {
        cam = FindObjectOfType<Camera>();

        BlockProgress.fillAmount = 0;
    }

    void Update()
    {
        bool leftClick = Input.GetMouseButton(0);
        bool rightClick = Input.GetMouseButton(1);

        if (!GameManager.paused)
        {
            RaycastHit hitInfo;
            Vector3 point = new Vector3();

            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hitInfo, 4, groundLayer))
            {
                if (leftClick || rightClick)
                {
                    if (leftClick) point = hitInfo.point + cam.transform.forward * .01f;
                    else point = hitInfo.point - cam.transform.forward * .01f;

                    int chunkPosX = Mathf.FloorToInt(point.x / 16f);
                    int chunkPosZ = Mathf.FloorToInt(point.z / 16f);

                    TerrainChunk chunk = TerrainGenerator.Chunks[new Vector2Int(chunkPosX, chunkPosZ)];

                    int bix = Mathf.FloorToInt(point.x) - (chunkPosX * 16) + 1;
                    int biy = Mathf.FloorToInt(point.y);
                    int biz = Mathf.FloorToInt(point.z) - (chunkPosZ * 16) + 1;

                    lookingBlock = new Vector3Int(bix, biy, biz);

                    if (leftClick && allowBlockBreak && !breaking || leftClick && allowBlockBreak && breaking && lookingBlock != breakingBlock)
                    {
                        breaking = false;

                        StartCoroutine(IWaitBlock());
                        StartCoroutine(IBreakBlock(Block.blocks[chunk.heightMap[bix, biy, biz]], lookingBlock, new Vector3Int(Mathf.FloorToInt(point.x), biy, 
                            Mathf.FloorToInt(point.z)), new Vector2Int(chunkPosX, chunkPosZ), bix, biz, hitInfo.point));
                    }
                    else if (rightClick)
                    {
                        bool interactableBlock = false;
                        Vector3 downHitPos = hitInfo.point + cam.transform.forward * .01f;

                        int dix = Mathf.FloorToInt(downHitPos.x) + 1;
                        int diy = Mathf.FloorToInt(downHitPos.y);
                        int diz = Mathf.FloorToInt(downHitPos.z) + 1;

                        int dlix = dix - (chunkPosX * 16);
                        int dliz = diz - (chunkPosZ * 16);

                        if (chunk.heightMap[dlix, diy, dliz] != BlockType.Air) interactableBlock = Block.blocks[chunk.heightMap[dlix, diy, dliz]].isInteractable;

                        if (interactableBlock) Block.blocks[chunk.heightMap[dlix, diy, dliz]].actionOnUse(new Vector3Int(dix, diy, diz));
                        else if (Inventory.Slots[Inventory.activeHotbar].Quantity > 0 && allowBlockBreak)
                        {
                            StartCoroutine(IWaitBlock());
                            //SavingManager.ActiveSave.Chunks[new Vector2Int(chunkPosX, chunkPosZ)][bix, biy, biz] = Inventory.Slots[Inventory.activeHotbar].Item.blockReference;

                            chunk.heightMap[bix, biy, biz] = Inventory.Slots[Inventory.activeHotbar].Item.blockReference;
                            chunk.lodMeshes[chunk.previousLODIndex].RequestMesh(chunk.heightMap, chunk.coord);

                            Inventory.Slots[Inventory.activeHotbar].Quantity--;
                            Inventory.Slots[Inventory.activeHotbar].OnItemChange?.Invoke();

                            if (Inventory.Slots[Inventory.activeHotbar].Item.actionOnCreate != null) 
                                Inventory.Slots[Inventory.activeHotbar].Item.actionOnCreate(new Vector3Int(bix + (chunkPosX * 16), biy, biz + (chunkPosZ * 16)));
                            if (Inventory.Slots[Inventory.activeHotbar].Quantity < 1) Inventory.Slots[Inventory.activeHotbar].Clear();
                        }
                    }
                }
            }
        }
    }

    IEnumerator IBreakBlock(Block block, Vector3Int blockPos, Vector3Int globalBlockPos, Vector2Int chunk, int bix, int biy, Vector3 hitInfo)
    {
        breakingBlock = blockPos;
        int maxIters = Mathf.RoundToInt(block.time * 30);

        breaking = true;
        BlockProgress.fillAmount = 0;

        LeanTween.alpha(Cursor.gameObject, 0f, 0.2f);
        LeanTween.scale(Cursor.gameObject, Vector3.zero, 0.2f);

        LeanTween.alpha(BlockProgress.gameObject, 1f, 0.2f);
        LeanTween.scale(BlockProgress.gameObject, Vector3.one, 0.2f);

        for (int i = 0; i < maxIters; i++)
        {
            if (!Input.GetMouseButton(0) || !breaking || lookingBlock != breakingBlock) { maxIters = 0; break; }
            BlockProgress.fillAmount = (int)Mathf.Round((float)(100 * i) / maxIters) / 100f;

            yield return new WaitForSeconds(0.03f);
        }

        breaking = false;
        BlockProgress.fillAmount = 0;

        LeanTween.alpha(Cursor.gameObject, 1f, 0.2f);
        LeanTween.scale(Cursor.gameObject, Vector3.one, 0.2f);

        LeanTween.alpha(BlockProgress.gameObject, 0f, 0.2f);
        LeanTween.scale(BlockProgress.gameObject, Vector3.zero, 0.2f);

        if (maxIters > 0)
        {
            Inventory.AddItem(Block.blocks[TerrainGenerator.Chunks[chunk].heightMap[blockPos.x, blockPos.y, blockPos.z]], 1);

            TerrainGenerator.Chunks[chunk].heightMap[blockPos.x, blockPos.y, blockPos.z] = BlockType.Air;
            TerrainGenerator.Chunks[chunk].lodMeshes[TerrainGenerator.Chunks[chunk].previousLODIndex].
                RequestMesh(TerrainGenerator.Chunks[chunk].heightMap, TerrainGenerator.Chunks[chunk].coord);

            //SavingManager.ActiveSave.Chunks[TerrainGenerator.Chunks[chunk].coord][blockPos.x, blockPos.y, blockPos.z] = BlockType.Air;
            AudioManager.instance.GrabItem.Play();

            if (bix == 1)
            {
                Vector2Int sideChunk = new Vector2Int(chunk.x - 1, chunk.y);

                TerrainGenerator.Chunks[sideChunk].heightMap[17, blockPos.y, blockPos.z] = BlockType.Air;
                TerrainGenerator.Chunks[sideChunk].lodMeshes[TerrainGenerator.Chunks[sideChunk].previousLODIndex].
                    RequestMesh(TerrainGenerator.Chunks[sideChunk].heightMap, TerrainGenerator.Chunks[sideChunk].coord);
            }
            else if (bix == 16)
            {
                Vector2Int sideChunk = new Vector2Int(chunk.x + 1, chunk.y);

                TerrainGenerator.Chunks[sideChunk].heightMap[0, blockPos.y, blockPos.z] = BlockType.Air;
                TerrainGenerator.Chunks[sideChunk].lodMeshes[TerrainGenerator.Chunks[sideChunk].previousLODIndex].
                    RequestMesh(TerrainGenerator.Chunks[sideChunk].heightMap, TerrainGenerator.Chunks[sideChunk].coord);
            }

            if (biy == 1)
            {
                Vector2Int sideChunk = new Vector2Int(chunk.x, chunk.y - 1);

                TerrainGenerator.Chunks[sideChunk].heightMap[blockPos.x, blockPos.y, 17] = BlockType.Air;
                TerrainGenerator.Chunks[sideChunk].lodMeshes[TerrainGenerator.Chunks[sideChunk].previousLODIndex].
                    RequestMesh(TerrainGenerator.Chunks[sideChunk].heightMap, TerrainGenerator.Chunks[sideChunk].coord);
            }
            else if (biy == 16)
            {
                Vector2Int sideChunk = new Vector2Int(chunk.x, chunk.y + 1);

                TerrainGenerator.Chunks[sideChunk].heightMap[blockPos.x, blockPos.y, 0] = BlockType.Air;
                TerrainGenerator.Chunks[sideChunk].lodMeshes[TerrainGenerator.Chunks[sideChunk].previousLODIndex].
                    RequestMesh(TerrainGenerator.Chunks[sideChunk].heightMap, TerrainGenerator.Chunks[sideChunk].coord);
            }
        }
    }

    IEnumerator IWaitBlock()
    {
        allowBlockBreak = false;
        yield return new WaitForSeconds(0.2f);
        allowBlockBreak = true;
    }
}
