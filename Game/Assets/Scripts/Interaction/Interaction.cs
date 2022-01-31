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
    public TextMeshProUGUI InteractText;

    [Space]

    public GameObject InteractableCube;
    public GameObject Highlight;

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

        Highlight.SetActive(false);
        InteractText.text = "";

        BlockProgress.fillAmount = 0;
    }

    void Update()
    {
        bool leftClick = Input.GetMouseButton(0);
        bool rightClick = Input.GetMouseButton(1);

        if (!GameManager.paused)
        {
            #region Blocks Interacting

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

                    TerrainChunk chunk = TerrainGenerator.terrainChunkDictionary[new Vector2Int(chunkPosX, chunkPosZ)];

                    int bix = Mathf.FloorToInt(point.x) - (chunkPosX * 16) + 1;
                    int biy = Mathf.FloorToInt(point.y);
                    int biz = Mathf.FloorToInt(point.z) - (chunkPosZ * 16) + 1;

                    lookingBlock = new Vector3Int(bix, biy, biz);

                    if (leftClick && allowBlockBreak && !breaking || leftClick && allowBlockBreak && breaking && lookingBlock != breakingBlock)
                    {
                        breaking = false;

                        StartCoroutine(IWaitBlock());
                        StartCoroutine(IBreakBlock(Block.blocks[chunk.heightMap[bix, biy, biz]], lookingBlock, new Vector3Int(bix + (chunkPosX * 16), biy, biz + (chunkPosZ * 16))
                            , chunk, hitInfo.point));
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

                        if (interactableBlock)
                        {
                            Block.blocks[chunk.heightMap[dlix, diy, dliz]].actionOnUse(new Vector3Int(dix, diy, diz));
                        }
                        else if (Inventory.Slots[Inventory.activeHotbar].Quantity > 0 && allowBlockBreak)
                        {
                            StartCoroutine(IWaitBlock());
                            SavingManager.ActiveSave.Chunks[new Vector2Int(chunkPosX, chunkPosZ)][bix, biy, biz] = Inventory.Slots[Inventory.activeHotbar].Item.blockReference;

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

            #endregion
            #region Item Interacting

            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;

            bool successfulHit = false;

            if (Physics.Raycast(ray, out hit, interactionDst))
            {
                Interactable interactable = hit.transform.GetComponent<Interactable>();

                if (interactable != null)
                {
                    InteractText.text = "Pickup " + hit.transform.name;
                    successfulHit = true;

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        Inventory.AddItem(interactable.Item, interactable.count);
                        Destroy(hit.transform.gameObject);
                    }
                }
            }

            if (!successfulHit)
            {
                InteractText.text = "";
            }

            #endregion
        }
    }

    IEnumerator IBreakBlock(Block block, Vector3Int blockPos, Vector3Int globalBlockPos, TerrainChunk chunk, Vector3 hitInfo)
    {
        breakingBlock = blockPos;
        int maxIters = Mathf.RoundToInt(block.time * 30);

        breaking = true;
        Cursor.gameObject.SetActive(false);

        for (int i = 0; i < maxIters; i++)
        {
            if (!Input.GetMouseButton(0) || !breaking || lookingBlock != breakingBlock) { maxIters = 0; break; }
            BlockProgress.fillAmount = (int)Mathf.Round((float)(100 * i) / maxIters) / 100f;

            yield return new WaitForSeconds(0.03f);
        }

        Cursor.gameObject.SetActive(true);
        breaking = false;

        BlockProgress.fillAmount = 0;

        if (maxIters > 0)
        {
            Inventory.AddItem(Block.blocks[chunk.heightMap[blockPos.x, blockPos.y, blockPos.z]], 1);

            /*
            Block oldBlock = Block.blocks[chunk.blocks[blockPos.x, blockPos.y, blockPos.z]];
            GameObject item = Instantiate(InteractableCube);

            if (oldBlock.actionOnDestroy != null) oldBlock.actionOnDestroy(globalBlockPos);

            item.transform.position = new Vector3(hitInfo.x, hitInfo.y + 1, hitInfo.z);
            item.transform.name = item.GetInstanceID().ToString();

            item.GetComponent<MeshRenderer>().material.color = oldBlock.ItemColor;
            item.AddComponent<Interactable>().Item = oldBlock;
            */
            chunk.heightMap[blockPos.x, blockPos.y, blockPos.z] = BlockType.Air;
            chunk.lodMeshes[chunk.previousLODIndex].RequestMesh(chunk.heightMap, chunk.coord);

            SavingManager.ActiveSave.Chunks[chunk.coord][blockPos.x, blockPos.y, blockPos.z] = BlockType.Air;

        }
    }

    IEnumerator IWaitBlock()
    {
        allowBlockBreak = false;
        yield return new WaitForSeconds(0.2f);
        allowBlockBreak = true;
    }
}
