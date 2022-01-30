using System.Collections.Generic;
using UnityEngine;

public class InventorySlot
{
    public delegate void ItemChangeCallback();
    public ItemChangeCallback OnItemChange;

    public Block Item;
    public int Quantity;
    public int MaxQuantity = int.MaxValue;

    public bool StoreItem(Block item, int quantity)
    {
        if (Item != item) { Quantity = quantity; } else { Quantity += quantity; }
        Item = item;
        OnItemChange?.Invoke();

        return true;
    }

    public void Clear() { Item = null; Quantity = 0; OnItemChange?.Invoke(); }

    public bool MoveTo(InventorySlot slotDestination, int quantity)
    {
        if (slotDestination != null && slotDestination.CanSlotContainItem(Item))
        {
            slotDestination.StoreItem(Item, quantity);

            Quantity -= quantity;
            if (Quantity <= 0) Clear();

            OnItemChange?.Invoke();
            return true;
        }
        else { Debug.Log("move to didnt work"); return false; }
    }

    public bool MoveAllTo(InventorySlot slotDestination) { return MoveTo(slotDestination, Quantity); }
    public bool CanSlotHoldItems(int quantity) { return quantity <= MaxQuantity; }
    public bool CanAddItemsToSlot(int quantity) { return CanSlotHoldItems(Quantity + quantity); }

    public bool CanSlotContainItem(Block item)
    {
        if (Item == null || Item == item)
        {
            return true;
        }
        else { return false; }
    }
}