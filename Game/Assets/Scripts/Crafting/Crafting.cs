using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crafting : MonoBehaviour
{
    #region Singleton
    public static Crafting instance;
    void Awake() { instance = this; }
    #endregion

    public GameObject ContentHolder;
    public GameObject CraftUIPrefab;

    void Start()
    {
        for (int i = 0; i < Inventory.instance.InvSlots.Length; i++) { Inventory.Slots[i].OnItemChange += UpdateRecipes; }
    }

    void UpdateRecipes()
    {
        foreach (Transform child in ContentHolder.transform) { Destroy(child.gameObject); }

        for (int i = 0; i < Crafts.Length; i++)
        {
            if (CanCraft(Crafts[i]))
            {
                CraftUI craftUI = Instantiate(CraftUIPrefab, ContentHolder.transform).GetComponent<CraftUI>();
                craftUI.DisplayItems(Crafts[i]);
            }
        }
    }

    public bool CanCraft(Craft craft)
    {
        bool canCraft = false;

        for (int i = 0; i < craft.Input.Length; i++)
        {
            int itemCount = 0;

            for (int j = 0; j < Inventory.instance.InvSlots.Length; j++)
            {
                try
                {
                    if (Inventory.Slots[j].Item.blockReference == craft.Input[i].Item)
                    {
                        itemCount += Inventory.Slots[j].Quantity;
                        if (itemCount >= craft.Input[i].Count) { canCraft = true; break; } else { canCraft = false; }
                    }
                }
                catch { return false; }
            }

            if (!canCraft) return false;
        }

        return true;
    }

    public static void CraftItems(Craft craft)
    {
        for (int i = 0; i < craft.Input.Length; i++)
        {
            int itemCount = craft.Input[i].Count;

            for (int j = 0; j < Inventory.instance.InvSlots.Length; j++)
            {
                if (Inventory.Slots[j].Item != null)
                {
                    if (Inventory.Slots[j].Item.blockReference == craft.Input[i].Item)
                    {
                        if (Inventory.Slots[j].Quantity > itemCount) { Inventory.Slots[j].Quantity -= itemCount; Inventory.Slots[j].OnItemChange?.Invoke(); break; }
                        else if (Inventory.Slots[j].Quantity == itemCount) { Inventory.Slots[j].Clear(); Inventory.Slots[j].OnItemChange?.Invoke(); break; }
                        else { itemCount -= Inventory.Slots[j].Quantity; Inventory.Slots[j].Clear(); }
                    }
                }
            }
        }

        for (int i = 0; i < craft.Output.Length; i++)
        {
            Inventory.AddItem(Block.blocks[craft.Output[i].Item], craft.Output[i].Count);
        }

        instance.UpdateRecipes();
    }

    public static Craft[] Crafts = new Craft[]
    {
        new Craft(new CraftItem[] { new CraftItem(BlockType.OakLeaves, 2) }, new CraftItem[] { new CraftItem(BlockType.Oak, 1) }),
        new Craft(new CraftItem[] { new CraftItem(BlockType.Spruce, 2) }, new CraftItem[] { new CraftItem(BlockType.Barrel, 1) }),
        new Craft(new CraftItem[] { new CraftItem(BlockType.Oak, 2) }, new CraftItem[] { new CraftItem(BlockType.Barrel, 1) }),
        new Craft(new CraftItem[] { new CraftItem(BlockType.Jungle, 2) }, new CraftItem[] { new CraftItem(BlockType.Barrel, 1) }),
        new Craft(new CraftItem[] { new CraftItem(BlockType.Birch, 2) }, new CraftItem[] { new CraftItem(BlockType.Barrel, 1) }),
    };
}

public class Craft
{
    public CraftItem[] Input;
    public CraftItem[] Output;

    public Craft(CraftItem[] input, CraftItem[] output)
    {
        Input = input;
        Output = output;
    }
}

public class CraftItem
{
    public BlockType Item;
    public int Count;

    public CraftItem(BlockType item, int count)
    {
        Item = item;
        Count = count;
    }
}
