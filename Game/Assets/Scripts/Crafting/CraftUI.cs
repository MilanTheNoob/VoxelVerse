using UnityEngine.UI;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Button))]
public class CraftUI : MonoBehaviour
{
    public GameObject CraftingItem;

    [Space]

    public GameObject InputsHolder;
    public GameObject OutputsHolder;

    Craft craft;

    void Start() { GetComponent<Button>().onClick.AddListener(Craft); }
    void Craft() { Crafting.CraftItems(craft); }

    public void DisplayItems(Craft craft)
    {
        this.craft = craft;

        for (int i = 0; i < craft.Input.Length; i++) { RenderItem(craft.Input[i], InputsHolder); }
        for (int i = 0; i < craft.Output.Length; i++) { RenderItem(craft.Output[i], OutputsHolder); }
    }

    void RenderItem(CraftItem item, GameObject holder)
    {
        CraftUIItem itemG = Instantiate(CraftingItem, holder.transform).GetComponent<CraftUIItem>();
        Debug.Log(item.Item.ToString());
        itemG.ItemColorImg.color = Block.blocks[item.Item].ItemColor;

        itemG.QuantityText.text = item.Count.ToString();
        itemG.NameText.text = item.Item.ToString();
    }
}
