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

    void Craft()
    {
        Crafting.CraftItems(craft);
    }

    public void DisplayItems(Craft craft)
    {
        this.craft = craft;

        for (int i = 0; i < craft.Input.Length; i++)
        {
            CraftUIItem inputG = Instantiate(CraftingItem, InputsHolder.transform).GetComponent<CraftUIItem>();
            inputG.ItemColorImg.color = Block.blocks[craft.Input[i].Item].ItemColor;

            inputG.QuantityText.text = craft.Input[i].Count.ToString();
            inputG.NameText.text = craft.Input[i].Item.ToString();
        }

        for (int i = 0; i < craft.Output.Length; i++)
        {
            CraftUIItem outputG = Instantiate(CraftingItem, OutputsHolder.transform).GetComponent<CraftUIItem>();
            outputG.ItemColorImg.color = Block.blocks[craft.Output[i].Item].ItemColor;

            outputG.QuantityText.text = craft.Output[i].Count.ToString();
            outputG.NameText.text = craft.Output[i].Item.ToString();
        }
    }
}
