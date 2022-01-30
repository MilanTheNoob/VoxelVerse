using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    public static List<InventorySlot> Slots = new List<InventorySlot>();

    public GridLayoutGroup GridLayout;

    public InventorySlotUIController[] InvSlots;
    public InventorySlotUIController[] HotbarSlots;

    [Space]

    public Sprite HotbarSprite;
    public Sprite HotbarActiveSprite;

    public static int activeHotbar = 0;

    void Awake()
    {
        instance = this;

        for (int i = 0; i < InvSlots.Length; i++) { Slots.Add(new InventorySlot()); InvSlots[i].SetInventorySlot(Slots[i]); }
        for (int i = 0; i < HotbarSlots.Length; i++) { HotbarSlots[i].SetInventorySlot(Slots[i]); }
    }

    private void Update()
    {
        if (!GameManager.paused)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) { activeHotbar = 0; UpdateHotbar(); }
            else if (Input.GetKeyDown(KeyCode.Alpha2)) { activeHotbar = 1; UpdateHotbar(); }
            else if (Input.GetKeyDown(KeyCode.Alpha3)) { activeHotbar = 2; UpdateHotbar(); }
            if (Input.GetKeyDown(KeyCode.Alpha4)) { activeHotbar = 3; UpdateHotbar(); }

            if (Input.GetMouseButton(1))
            {
                RaycastHit hitInfo;
                Vector3 point = new Vector3();

                if (Physics.Raycast(transform.position, transform.forward, out hitInfo, 4, Interaction.instance.groundLayer))
                {
                    point = hitInfo.point - transform.forward * .01f;

                    int chunkPosX = Mathf.FloorToInt(point.x / 16f);
                    int chunkPosZ = Mathf.FloorToInt(point.z / 16f);

                    TerrainChunk chunk = TerrainGenerator.chunks[new Vector2Int(chunkPosX, chunkPosZ)];

                    int bix = Mathf.FloorToInt(point.x) - chunkPosX + 1;
                    int biy = Mathf.FloorToInt(point.y);
                    int biz = Mathf.FloorToInt(point.z) - chunkPosZ + 1;

                    chunk.blocks[bix, biy, biz] = Slots[activeHotbar].Item.blockReference;
                    chunk.BuildMesh();

                    Slots[activeHotbar].Quantity--;
                    Slots[activeHotbar].OnItemChange?.Invoke();
                }
            }
        }
    }

    void UpdateHotbar()
    {
        for (int i = 0; i < HotbarSlots.Length; i++)
        {
            if (i == activeHotbar) { HotbarSlots[i].localImage.sprite = HotbarActiveSprite; }
            else { HotbarSlots[i].localImage.sprite = HotbarSprite; }
        }
    }

    public static bool AddItem(Block item, int quantity = 1)
    {
        if (item == null) return false;

        InventorySlot slotToUse = FindFirst(slot => slot.Item == item);
        if (slotToUse == null) slotToUse = FindFirst(slot => slot.Item == null);

        return slotToUse.StoreItem(item, quantity);
    }

    public static InventorySlot FindFirst(Predicate<InventorySlot> predicate) { return Slots.Find(predicate); }
}