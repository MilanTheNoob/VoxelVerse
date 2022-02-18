using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The crafting script that deals with all crafting related features in Adventure mode
/// </summary>
public class Crafting : MonoBehaviour
{
    #region Singleton
    public static Crafting instance;
    void Awake() { instance = this; }
    #endregion

    [Header("The gameObject that holds the UI for the recipes")] public GameObject ContentHolder;
    [Header("The UI Object that displays a recipe")] public GameObject CraftUIPrefab;

    void Start() { for (int i = 0; i < Inventory.instance.InvSlots.Length; i++) { Inventory.Slots[i].OnItemChange += UpdateRecipes; } }

    /// <summary>
    /// Updates all the available recipes to the player
    /// </summary>
    public void UpdateRecipes()
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

    /// <summary>
    /// Checks if the provided recipe can actually be crafted by the player
    /// </summary>
    /// <param name="craft">The crafting recipe</param>
    /// <returns>The boolean returning if the player can craft the recipe</returns>
    public bool CanCraft(Craft craft)
    {
        bool canCraft = false;

        for (int i = 0; i < craft.Input.Length; i++)
        {
            int itemCount = 0;

            for (int j = 0; j < Inventory.instance.InvSlots.Length; j++)
            {
                if (Inventory.Slots[j].Item != null)
                {
                    if (Inventory.Slots[j].Item.blockReference == craft.Input[i].Item)
                    {
                        itemCount += Inventory.Slots[j].Quantity;
                        if (itemCount >= craft.Input[i].Count) { canCraft = true; break; } else { canCraft = false; }
                    }
                }
            }

            if (!canCraft) return false;
        }

        return true;
    }

    /// <summary>
    /// Crafts an item by removing the inputs from the player and giving them the outputs
    /// NOTE : The function does not check if the player has the necessary items for performance reasons
    /// </summary>
    /// <param name="craft">The recipe to craft? I guess</param>
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

        for (int i = 0; i < craft.Output.Length; i++) Inventory.AddItem(Block.blocks[craft.Output[i].Item], craft.Output[i].Count);
        instance.UpdateRecipes();
    }

    /// <summary>
    /// List of current crafting recipes in the entire game
    /// </summary>
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