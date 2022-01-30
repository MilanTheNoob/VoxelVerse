using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventoryCursorController : MonoBehaviour
{
    #region Singleton
    public static InventoryCursorController instance;
    void Awake() { instance = this; }
    #endregion

    public Image CursorItem;
    public TextMeshProUGUI CursorText;

    RectTransform CursorItemR;
    RectTransform CursorTextR;

    int quantity;
    Block block;

    private void Start()
    {
        CursorItem.gameObject.SetActive(false);
        CursorText.gameObject.SetActive(false);

        CursorItemR = CursorItem.GetComponent<RectTransform>();
        CursorTextR = CursorText.GetComponent<RectTransform>();

        CursorItemR.anchoredPosition = new Vector2(CursorItemR.anchoredPosition.x, -65);
        CursorTextR.anchoredPosition = new Vector2(CursorTextR.anchoredPosition.x, 15);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            InventorySlot slot = FindSlotAtPosition();

            if (slot != null)
            {
                if (slot.Quantity < 1 || slot.Item == null)
                {
                    slot.StoreItem(block, quantity);

                    DisableCursor();

                    block = null;
                    quantity = 0;
                }
                else if (block != null && slot.CanSlotContainItem(block))
                {
                    if (slot.CanSlotHoldItems(quantity))
                    {
                        slot.Quantity += quantity;
                        slot.OnItemChange?.Invoke();

                        block = null;
                        quantity = 0;

                        DisableCursor();
                    }
                    else
                    {
                        quantity -= slot.MaxQuantity - slot.Quantity;

                        slot.Quantity = slot.MaxQuantity;
                        slot.OnItemChange?.Invoke();
                    }
                }
                else if (block == null && quantity < 1)
                {
                    quantity = slot.Quantity;
                    block = slot.Item;

                    slot.Clear();

                    EnableCursor();

                    CursorText.text = quantity.ToString();
                    CursorItem.color = block.ItemColor;
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            InventorySlot slot = FindSlotAtPosition();

            if (slot != null && (slot.Item == block || block == null) && slot.Quantity > 0)
            {
                block = slot.Item;

                if (slot.Quantity == 1)
                {
                    quantity = 1;
                    slot.Clear();
                }
                else
                {
                    quantity = Mathf.FloorToInt(slot.Quantity / 2);

                    slot.Quantity -= quantity;
                    slot.OnItemChange?.Invoke();
                }

                EnableCursor();

                CursorText.text = quantity.ToString();
                CursorItem.color = block.ItemColor;
            }
        }
    }

    public void ForceHideCursor()
    {
        if (quantity > 0) Inventory.AddItem(block, quantity);

        block = null;
        quantity = 0;

        DisableCursor();
    }

    private InventorySlot FindSlotAtPosition()
    {
        InventorySlot foundSlot = null;

        PointerEventData pointerEventData = new PointerEventData(null);
        pointerEventData.position = transform.position;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);

        foreach (RaycastResult result in results)
        {
            InventorySlotUIController slotController = result.gameObject.GetComponent<InventorySlotUIController>();
            if (slotController != null)
            {
                foundSlot = slotController.inventorySlot;
                break;
            }
        }

        return foundSlot;
    }

    void DisableCursor()
    {
        LeanTween.moveY(CursorItemR, -65, 0.3f).setEaseInExpo();
        LeanTween.alpha(CursorItemR, 0f, 0.3f).setOnComplete(() => { CursorItem.gameObject.SetActive(false); });

        LeanTween.moveY(CursorTextR, 15, 0.3f).setEaseInExpo();
        LeanTween.alpha(CursorTextR, 0f, 0.3f).setOnComplete(() => { CursorText.gameObject.SetActive(false); });
    }

    void EnableCursor()
    {
        CursorItem.gameObject.SetActive(true);
        CursorText.gameObject.SetActive(true);

        LeanTween.moveY(CursorItemR, -25, 0.3f).setEaseInExpo();
        LeanTween.alpha(CursorItemR, 1f, 0.3f);

        LeanTween.moveY(CursorTextR, -25, 0.3f).setEaseInExpo();
        LeanTween.alpha(CursorTextR, 1f, 0.3f);
    }
}
