using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics;
using System.IO;

public class VersionUIController : MonoBehaviour
{
    public Image UIBackground;
    
    [Space]

    public TextMeshProUGUI NameText;
    public TextMeshProUGUI SecondaryText;

    [Space]

    public Button DeleteButton;
    public Button PlayButton;

    VersionInfoClass attachedVersion;

    public void SetVersion(VersionInfoClass newVer)
    {
        attachedVersion = newVer;
        NameText.text = attachedVersion.ToString();

        switch (attachedVersion.VersionType)
        {
            case VersionTypeEnum.Prototype: SecondaryText.text = "Beware! Undercaffeinated programmers & bugs everywhere!"; break;
            case VersionTypeEnum.Alpha: SecondaryText.text = "Alr alr, it ain't too bad. Just got to fix that 2GB memory leak before its too late."; break;
            case VersionTypeEnum.Beta: SecondaryText.text = "Ok, just don't screw up the code before release, your so close, ahhh fuuuuu"; break;
        }

        PlayButton.onClick.AddListener(PlayVersion);
        DeleteButton.onClick.AddListener(DeleteVersion);
    }

    public void PlayVersion()
    {
        ProgramManager.ActiveGame = Process.Start(Application.persistentDataPath + "/Installs/" + attachedVersion.ToString() + "/VoxelVerse.exe");
    }

    public void DeleteVersion()
    {
        try { ProgramManager.ActiveGame.Kill(); ProgramManager.ActiveGame = null; } catch { }
        string path = Application.persistentDataPath + "/Installs/" + attachedVersion.ToString();

        try
        {
            foreach (var item in Directory.GetFiles(path)) { File.Delete(item); }
            Directory.Delete(path);
        } catch { }

        Destroy(gameObject);
    }
}
