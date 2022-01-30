using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DirectoryButton : MonoBehaviour
{
    public TextMeshProUGUI label;

    public string text;
    public string fullPath;
    public int id;

    public void OnClick() { ProgramManager.instance.OnDirectoryClick(id); }
    public void Set(string txt, string path, int i) { text = txt; fullPath = path; id = i; label.text = text; }
}