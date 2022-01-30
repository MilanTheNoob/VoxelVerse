using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FileButton : MonoBehaviour
{
    public Button button;
    public Image buttonImage;
    public Image fileIcon;
    public TextMeshProUGUI label;
    public Sprite selectedSprite;

    [Space]

    public string text;
    public string fullPath;
    public bool isDir;
    public int id;

    public void Select()
    {
        button.transition = Selectable.Transition.None;
        buttonImage.overrideSprite = selectedSprite;
    }

    public void Unselect()
    {
        button.transition = Selectable.Transition.SpriteSwap;
        buttonImage.overrideSprite = null;
    }

    public void OnClick() { ProgramManager.instance.OnFileClick(id); }

    public void Set(string txt, string path, bool dir, int i)
    {
        text = txt;
        fullPath = path;
        isDir = dir;
        id = i;
        label.text = text;

        if (isDir) fileIcon.sprite = ProgramManager.instance.BrowseFolderIcon;
        else fileIcon.sprite = ProgramManager.instance.GetFileIcon(txt);
    }
}