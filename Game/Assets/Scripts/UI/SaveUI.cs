using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class SaveUI : MonoBehaviour
{
    public Image Icon;

    [Space]

    public TextMeshProUGUI Name;
    public TextMeshProUGUI Description;

    [Space]

    public Button Options;
    public Button Play;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SetSave(SaveDataClass saveDat, int index)
    {
        Icon.sprite = saveDat.Icon;

        Name.text = saveDat.Name;
        Description.text = "Version: " + saveDat.Version + ", Created: " + saveDat.DateCreated + ", Last Played: " + saveDat.DatePlayed;

        Play.onClick.AddListener(() =>
        {
            SavingManager.LoadAdventureSave(index);
        });
    }
}
