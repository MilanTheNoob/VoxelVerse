using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

/// <summary>
/// Deals with almost all features in the flat world allowing me to create structures in the game
/// </summary>
public class FlatWorldManager : MonoBehaviour
{
    public static BlockType CurrentBlock;
    public static bool paused;

    [Header("The UI menu displaying all the available blocks to build with")] public RectTransform ItemsPanel;
    [Header("The UI object that holds all the buttons for their respective blocks")] public GameObject ItemsHolder;

    [Space, Header("The font to display the name of the items")] public TMP_FontAsset MainFont;
    [Header("The plain circular sprite to display the color of the block")] public Sprite CircleSprite;
    [Header("The sprite of the background of the button")] public Sprite PanelSprite;


    [Space, Header("The main player object in the flat world")] public CharacterController Player;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        ItemsPanel.gameObject.SetActive(false);

        for (int i = 0; i < Block.blocks.Count; i++)
        {
            GameObject item = new GameObject();
            Block block = Block.blocks.ElementAt(i).Value;

            item.transform.parent = ItemsHolder.transform;
            Image itemI = item.AddComponent<Image>();
            itemI.sprite = PanelSprite;
            itemI.type = Image.Type.Sliced;
            itemI.pixelsPerUnitMultiplier = 2;

            GameObject itemText = new GameObject();
            itemText.transform.parent = item.transform;

            TextMeshProUGUI itemTextT = itemText.AddComponent<TextMeshProUGUI>();
            itemTextT.text = block.blockReference.ToString();
            itemTextT.font = MainFont;
            itemTextT.fontSize = 9;
            itemTextT.alignment = TextAlignmentOptions.Center;

            itemText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -12);
            itemText.GetComponent<RectTransform>().sizeDelta = new Vector2(43, 21);
            item.GetComponent<RectTransform>().localScale = Vector3.one;

            GameObject itemImage = new GameObject();
            itemImage.transform.parent = item.transform;

            Image itemImageI = itemImage.AddComponent<Image>();
            itemImageI.sprite = CircleSprite;
            itemImageI.color = block.ItemColor;

            RectTransform itemimageRT = itemImage.GetComponent<RectTransform>();

            itemimageRT.anchoredPosition = new Vector2(1, 10.8f);
            itemimageRT.localScale = Vector3.one;
            itemimageRT.sizeDelta = new Vector2(25, 25);

            Button itemB = item.AddComponent<Button>();
            itemB.onClick.AddListener(() =>
            {
                CurrentBlock = block.blockReference;
                ItemsPanel.gameObject.SetActive(false);

                paused = false;
                Player.pause = false;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            });
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            paused = !paused;

            ItemsPanel.gameObject.SetActive(paused);
            Player.pause = paused;

            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            paused = !paused;
            Player.pause = paused;

            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused;
        }
    }
}
