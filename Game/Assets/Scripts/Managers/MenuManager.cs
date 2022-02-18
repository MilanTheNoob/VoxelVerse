using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    #region instance
    public static MenuManager instance;
    void Awake() {  instance = this; }
    #endregion

    public RectTransform MainMenu;
    public TextMeshProUGUI MenuText;
    public TextMeshProUGUI MenuAlphaText;
    public GameObject[] MenuButtons;

    [Space]

    public GameObject FadingPanel;
    public RectTransform SidePanel;

    [Space]

    public TextMeshProUGUI GamemodeName;
    public TextMeshProUGUI GamemodeDescription;

    [Space]

    public TMP_InputField NewAdventureName;
    public TMP_InputField NewAdventureSeed;
    public Button NewAdventureSubmit;

    [Space]

    public GameObject AdventureSavesHolder;
    public GameObject AdventureSaveUI;
    public GameObject AdventureSaveNothingHere;

    void Start()
    {
        StartCoroutine(IStart());
        HideDescription();

        NewAdventureSubmit.onClick.AddListener(() =>
        {
            SavingManager.GameSave.AdventureSaves.Add(new SaveDataClass()
            {
                Index = (byte)SavingManager.GameSave.AdventureSaves.Count,
                Name = NewAdventureName.text,

                DateCreated = DateTime.Now.ToString("MM/dd/yy"),
                DatePlayed = DateTime.Now.ToString("MM/dd/yy")
            });

            SavingManager.GameMode = GamemodeEnum.Adventure;
            SavingManager.AdventureSave = new AdventureSaveClass();
            SavingManager.AdventureSave.CreateNew();

            LeanTween.alpha(FadingPanel, 1f, 0.5f).setOnComplete(() => { SceneManager.LoadScene(1, LoadSceneMode.Single); });
        });
    }

    IEnumerator IStart()
    {
        FadingPanel.SetActive(true);

        yield return new WaitForSeconds(2f);
        LeanTween.alpha(FadingPanel, 0f, 0.2f).setEaseInCubic().setOnComplete(() => { FadingPanel.SetActive(false); });

        gameObject.GetComponent<MenuManagerUI>().OpenMenu(0);
    }

    public void DescribeGamemode(int gamemode)
    {
        SidePanel.gameObject.SetActive(true);

        GamemodeDescription.gameObject.SetActive(true);
        GamemodeName.gameObject.SetActive(true);

        GamemodeName.text = ((GamemodeEnum)gamemode).ToString();
        GamemodeDescription.text = GamemodeDescriptions[(GamemodeEnum)gamemode];
    }

    public void HideDescription()
    {
        SidePanel.gameObject.SetActive(false);

        GamemodeDescription.gameObject.SetActive(false);
        GamemodeName.gameObject.SetActive(false);
    }

    public void UpdateAdventureSaves()
    {
        foreach (Transform child in AdventureSavesHolder.transform) { Destroy(child.gameObject); }

        if (SavingManager.GameSave.AdventureSaves.Count == 0) { AdventureSaveNothingHere.SetActive(true); }
        else
        {
            AdventureSaveNothingHere.SetActive(false);

            for (int i = 0; i < SavingManager.GameSave.AdventureSaves.Count; i++)
            {
                GameObject g = Instantiate(AdventureSaveUI, AdventureSavesHolder.transform);
                g.transform.localScale = Vector3.one;
                g.GetComponent<SaveUI>().SetSave(SavingManager.GameSave.AdventureSaves[i], i);
            }
        }
    }

    public void Quit() { Application.Quit(); }

    public static void WriteText(TextMeshProUGUI textHolder, string text, float duration) { instance.StartCoroutine(IWriteText(textHolder, text, duration)); }
    public static IEnumerator IWriteText(TextMeshProUGUI textHolder, string text, float duration)
    {
        textHolder.text = "";
        float timePerChar = duration / text.Length;

        for (int i = 0; i < text.Length; i++)
        {
            textHolder.text = textHolder.text + text[i];
            yield return new WaitForSeconds(timePerChar);
        }
    }

    public Dictionary<GamemodeEnum, string> GamemodeDescriptions = new Dictionary<GamemodeEnum, string>
    {
        { GamemodeEnum.Adventure, "A unique gamemode where you explore an infinite world finding structures, following storylines, collecting artefacts & so much more!" },
        { GamemodeEnum.Survival, "Survive for as long as possible in a finite & random world with dangerous enemies and a harsh environment" },
        { GamemodeEnum.BattleRoyale, "The classic survival gamemode with unique twists! Hop into a finite procedural world with your friends and battle it out in an ever shrinking world" },
        { GamemodeEnum.Minigames, "Play with friends or play by yourself with fun minigames, from simple Jump Clubs to seemingly never ending parkour levels!" }
    };
}

public enum GamemodeEnum { Adventure, Survival, BattleRoyale, Minigames }