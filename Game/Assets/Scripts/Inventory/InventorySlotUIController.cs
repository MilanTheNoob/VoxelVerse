using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlotUIController : MonoBehaviour
{
    public TextMeshProUGUI TextField;
    public TextMeshProUGUI QuantityField;
    public Image ImageField;

    [Space]

    public bool isGarbage;

    [HideInInspector] public Image localImage;
    [HideInInspector] public InventorySlot inventorySlot;

    void Start() { localImage = GetComponent<Image>(); if (isGarbage) { SetInventorySlot(new InventorySlot()); } }

    public void SetInventorySlot(InventorySlot invSlot)
    {
        localImage = GetComponent<Image>();
        inventorySlot = invSlot;

        UpdateSlot();
        inventorySlot.OnItemChange += UpdateSlot;
    }

    public void UpdateSlot()
    {
        bool displaySlot = inventorySlot != null && inventorySlot.Item != null;

        if (isGarbage && inventorySlot.Quantity > 0)
        {
            inventorySlot.Item = null;
            inventorySlot.Quantity = 0;
        }
        else if (!isGarbage)
        {
            if (TextField != null) TextField.gameObject.SetActive(displaySlot);
            if (QuantityField != null) QuantityField.gameObject.SetActive(displaySlot);
            ImageField.gameObject.SetActive(displaySlot);

            if (inventorySlot.Item != null)
            {
                if (TextField != null) TextField.text = inventorySlot.Item.blockReference.ToString();
                if (QuantityField != null) QuantityField.text = inventorySlot.Quantity.ToString();
                ImageField.color = inventorySlot.Item.ItemColor;
            }
        }
    }
}
