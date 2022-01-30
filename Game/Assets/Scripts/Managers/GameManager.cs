using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.IO;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.PostProcessing;

public class GameManager : MonoBehaviour
{
    public static GameVersionEnum GameVersion = GameVersionEnum.Test_1_0_0;
    public static GameManager instance;

    public static Dictionary<Vector3Int, InventorySlot[]> Storages = new Dictionary<Vector3Int, InventorySlot[]>();

    public RectTransform FadingPanel;
    public RectTransform HUD;
    public RectTransform ShiftMenu;
    public RectTransform StorageMenu;

    [Space]

    public RectTransform InventoryUI;
    public RectTransform CraftingUI;
    public RectTransform ComingSoonUI;

    [Space]

    public RectTransform PauseMenu;
    public RectTransform PauseText;
    public RectTransform PauseWarning;
    public RectTransform PauseVersion;
    public RectTransform[] PauseButtons;

    [Space]

    public RectTransform SettingsMenu;
    public RectTransform SettingsTitle;
    public RectTransform SettingsText;
    public RectTransform SettingsScrollview;

    [Space]

    public TMP_InputField MouseSpeedField;
    public TMP_InputField MouseSmoothingField;
    public Toggle MouseInvertField;
    public Toggle PPField;
    public Toggle AAField;
    public TMP_InputField FOVField;
    public TMP_InputField FPSTargetField;
    public TMP_InputField RenderDstField;
    public Toggle AnimateChunksField;

    [Space]

    public InventorySlotUIController[] StorageInvSlots;
    public InventorySlotUIController[] StorageChestSlots;

    [Space]

    public RectTransform CursorUI;
    public GameObject PP;

    [Space]

    public CharacterController Player;
    public MouseLook MouseLook;

    [Space]

    public AudioSource WalkingAudio;

    public static bool paused;
    public static GameStateEnum GameState;

    void Awake() { instance = this; }

    #region Start & Update

    void Start()
    {
        FadingPanel.gameObject.SetActive(true);
        StartCoroutine(DisableFading());

        HUD.gameObject.SetActive(true);
        ShiftMenu.gameObject.SetActive(false);
        PauseMenu.gameObject.SetActive(false);
        SettingsMenu.gameObject.SetActive(false);
        StorageMenu.gameObject.SetActive(false);

        LeanTween.alpha(InventoryUI, 0, 0);
        LeanTween.alpha(CraftingUI, 0, 0);
        LeanTween.alpha(ComingSoonUI, 0, 0);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    IEnumerator DisableFading() 
    {
        for (int i = 0; i < Inventory.instance.HotbarSlots.Length; i++)
        {
            RectTransform slot = Inventory.instance.HotbarSlots[i].GetComponent<RectTransform>();

            LeanTween.alpha(slot, 0f, 0f);
            slot.anchoredPosition = new Vector2(slot.anchoredPosition.x, slot.anchoredPosition.y + 100);
        }

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < StorageInvSlots.Length; i++) StorageInvSlots[i].SetInventorySlot(Inventory.Slots[i]);

        SetupSettings();
        LeanTween.alpha(FadingPanel, 0f, 0.2f);

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < Inventory.instance.HotbarSlots.Length; i++)
        {
            RectTransform slot = Inventory.instance.HotbarSlots[i].GetComponent<RectTransform>();

            LeanTween.alpha(slot, 1f, 1f).setDelay(i * 0.1f + 0.1f);
            LeanTween.moveLocalY(slot.gameObject, 0, 0.5f).setDelay(i * 0.1f).setEaseInExpo();
        }

        yield return new WaitForSeconds(1f);

        FadingPanel.gameObject.SetActive(false); 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (GameState == GameStateEnum.Storage)
            {
                CloseStorageMenu();
            }
            else if (!paused && (GameState == GameStateEnum.Inventory || GameState == GameStateEnum.InGame))
            {
                paused = true;
                Player.pause = true;

                GameState = GameStateEnum.Inventory;

                for (int i = 0; i < Inventory.instance.HotbarSlots.Length; i++)
                {
                    RectTransform slot = Inventory.instance.HotbarSlots[i].GetComponent<RectTransform>();

                    LeanTween.alpha(slot, 0f, 0.5f).setDelay(i * 0.1f + 0.1f);
                    LeanTween.moveLocalY(slot.gameObject, 100, 0.5f).setDelay(i * 0.1f).setEaseOutBounce();
                }

                ShiftMenu.gameObject.SetActive(true);

                LeanTween.alpha(InventoryUI, 1f, 0.5f);
                LeanTween.alpha(CraftingUI, 1f, 0.5f);
                LeanTween.alpha(ComingSoonUI, 1f, 0.5f);

                InventoryUI.anchoredPosition = new Vector2(InventoryUI.anchoredPosition.x, InventoryUI.anchoredPosition.y + 300);
                LeanTween.moveY(InventoryUI, -540, 0.5f).setEaseInCubic();

                CraftingUI.anchoredPosition = new Vector2(CraftingUI.anchoredPosition.x, CraftingUI.anchoredPosition.y - 300);
                LeanTween.moveY(CraftingUI, 0, 0.5f).setEaseInCubic();
                ComingSoonUI.anchoredPosition = new Vector2(ComingSoonUI.anchoredPosition.x, ComingSoonUI.anchoredPosition.y - 300);
                LeanTween.moveY(ComingSoonUI, 0, 0.5f).setEaseInCubic().setOnComplete(() => { HUD.gameObject.SetActive(false); });
            }
            else if (GameState == GameStateEnum.Inventory || GameState == GameStateEnum.InGame)
            {
                CloseInventory();
            }

            Cursor.visible = paused;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            switch (GameState)
            {
                case GameStateEnum.PauseMenu:
                    paused = false;
                    Player.pause = false;

                    GameState = GameStateEnum.InGame;

                    for (int i = 0; i < Inventory.instance.HotbarSlots.Length; i++)
                    {
                        RectTransform slot = Inventory.instance.HotbarSlots[i].GetComponent<RectTransform>();

                        LeanTween.alpha(slot, 1f, 1f).setDelay(i * 0.1f + 0.1f);
                        LeanTween.moveLocalY(slot.gameObject, 0, 0.5f).setDelay(i * 0.1f + 0.1f).setEaseInExpo();
                    }

                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;

                    ClosePauseMenu();
                    break;
                case GameStateEnum.InGame: OpenPauseMenu(); break;
                case GameStateEnum.Inventory: CloseInventory(); break;
                case GameStateEnum.SettingsMenu: SettingsMenu.gameObject.SetActive(true); OpenPauseMenu(); break;
                case GameStateEnum.Storage: CloseStorageMenu(); break;
            }
        }
    }

    #endregion

    #region Pause Actions

    public void Resume() { ClosePauseMenu(); }
    public void Quit() { SavingManager.SaveGame(); Application.Quit(); }

    public void Settings() { StartCoroutine(ISettings()); }
    public IEnumerator ISettings()
    {
        ClosePauseMenu();
        GameState = GameStateEnum.SettingsMenu;

        yield return new WaitForSeconds(0.3f);
        OpenSettingsMenu();
    }

    #endregion
    #region Menu Openening & Closing

    void OpenPauseMenu()
    {
        paused = true;
        Player.pause = true;

        GameState = GameStateEnum.PauseMenu;

        for (int i = 0; i < Inventory.instance.HotbarSlots.Length; i++)
        {
            RectTransform slot = Inventory.instance.HotbarSlots[i].GetComponent<RectTransform>();

            LeanTween.alpha(slot, 0f, 0.5f).setDelay(i * 0.1f + 0.1f);
            LeanTween.moveLocalY(slot.gameObject, 100, 0.5f).setDelay(i * 0.1f).setEaseOutBounce();
        }

        ShiftMenu.gameObject.SetActive(false);
        SettingsMenu.gameObject.SetActive(false);
        PauseMenu.gameObject.SetActive(true);

        LeanTween.alpha(PauseMenu, 0, 0);
        LeanTween.alpha(PauseMenu, 1f, 0.3f);

        LeanTween.alpha(PauseText, 0, 0);
        LeanTween.alpha(PauseText, 1f, 0.3f).setDelay(0.3f);

        PauseText.anchoredPosition = new Vector2(PauseText.anchoredPosition.x, 400);
        LeanTween.moveY(PauseText, 325, 0.3f).setEaseInCubic().setDelay(0.1f);

        PauseWarning.rotation = Quaternion.identity;
        LeanTween.rotateZ(PauseWarning.gameObject, -40, 0.2f).setDelay(0.1f).setOnComplete(() => {
            LeanTween.rotateZ(PauseWarning.gameObject, 20, 0.2f).setDelay(0.1f).setOnComplete(() => {
                LeanTween.rotateZ(PauseWarning.gameObject, -20, 0.2f);
            });
        });

        PauseVersion.anchoredPosition = new Vector2(200, PauseVersion.anchoredPosition.y);
        LeanTween.moveX(PauseVersion, 0, 0.3f);

        LeanTween.alpha(PauseVersion, 0, 0);
        LeanTween.alpha(PauseVersion, 1f, 0.3f);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    void ClosePauseMenu()
    {
        LeanTween.alpha(PauseMenu, 0, 0.3f).setOnComplete(() => { PauseMenu.gameObject.SetActive(false); });
        LeanTween.alpha(PauseText, 0, 0.3f);

        LeanTween.moveY(PauseText, 400, 0.3f).setEaseInCubic();

        LeanTween.moveX(PauseVersion, 200, 0.3f);
        LeanTween.alpha(PauseVersion, 0f, 0.3f);
    }

    void OpenSettingsMenu()
    {
        SettingsMenu.gameObject.SetActive(true);

        LeanTween.alpha(SettingsMenu, 0, 0);
        LeanTween.alpha(SettingsMenu, 1f, 0.3f);

        SettingsTitle.anchoredPosition = new Vector2(0, 400);
        LeanTween.moveY(SettingsTitle, 345, 0.3f).setDelay(0.2f);

        LeanTween.alpha(SettingsTitle, 0, 0);
        LeanTween.alpha(SettingsTitle, 1f, 0.3f).setDelay(0.2f);
    }

    void CloseInventory()
    {
        paused = false;
        Player.pause = false;

        GameState = GameStateEnum.InGame;

        HUD.gameObject.SetActive(true);
        LeanTween.scale(CursorUI, Vector3.one, 0.5f).setEaseInBounce();

        for (int i = 0; i < Inventory.instance.HotbarSlots.Length; i++)
        {
            RectTransform slot = Inventory.instance.HotbarSlots[i].GetComponent<RectTransform>();

            LeanTween.alpha(slot, 1f, 1f).setDelay(i * 0.1f + 0.1f);
            LeanTween.moveLocalY(slot.gameObject, 0, 0.5f).setDelay(i * 0.1f + 0.1f).setEaseInExpo();
        }

        LeanTween.alpha(InventoryUI, 0f, 0.5f);
        LeanTween.alpha(CraftingUI, 0f, 0.5f);
        LeanTween.alpha(ComingSoonUI, 0f, 0.5f);

        InventoryUI.anchoredPosition = new Vector2(InventoryUI.anchoredPosition.x, InventoryUI.anchoredPosition.y);
        LeanTween.moveY(InventoryUI, 300, 0.5f).setEaseInCubic();

        CraftingUI.anchoredPosition = new Vector2(CraftingUI.anchoredPosition.x, CraftingUI.anchoredPosition.y);
        LeanTween.moveY(CraftingUI, -300, 0.5f).setEaseInCubic();
        ComingSoonUI.anchoredPosition = new Vector2(ComingSoonUI.anchoredPosition.x, ComingSoonUI.anchoredPosition.y);
        LeanTween.moveY(ComingSoonUI, -300, 0.5f).setEaseInCubic().setOnComplete(() => { ShiftMenu.gameObject.SetActive(false); });

        InventoryCursorController.instance.ForceHideCursor();
    }

    public void OpenStorageMenu()
    {
        GameState = GameStateEnum.Storage;

        paused = true;
        Player.pause = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        HUD.gameObject.SetActive(false);
        ShiftMenu.gameObject.SetActive(false);
        SettingsMenu.gameObject.SetActive(false);
        PauseMenu.gameObject.SetActive(false);

        StorageMenu.gameObject.SetActive(true);
    }

    public void CloseStorageMenu()
    {
        GameState = GameStateEnum.InGame;

        paused = false;
        Player.pause = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        HUD.gameObject.SetActive(true);

        ShiftMenu.gameObject.SetActive(false);
        SettingsMenu.gameObject.SetActive(false);
        PauseMenu.gameObject.SetActive(false);
        StorageMenu.gameObject.SetActive(false);
    }

    #endregion

    #region Misc

    void SetupSettings()
    {
        MouseSpeedField.text = SavingManager.GameSave.mouseSpeed.ToString();
        MouseSmoothingField.text = SavingManager.GameSave.mouseSmoothing.ToString();
        MouseInvertField.isOn = SavingManager.GameSave.mouseInverted;

        PPField.isOn = SavingManager.GameSave.usePP;
        AAField.isOn = SavingManager.GameSave.useAA;
        FOVField.text = SavingManager.GameSave.fov.ToString();

        FPSTargetField.text = SavingManager.GameSave.targetFPS.ToString();
        RenderDstField.text = SavingManager.GameSave.renderDst.ToString();
        AnimateChunksField.isOn = SavingManager.GameSave.animateChunks;

        MouseLook.lateralSensitivity = SavingManager.GameSave.mouseSpeed;
        MouseLook.verticalSensitivity = SavingManager.GameSave.mouseSpeed;

        MouseLook.smoothTime = SavingManager.GameSave.mouseSmoothing;
        MouseLook.isInverted = SavingManager.GameSave.mouseInverted;

        PP.SetActive(SavingManager.GameSave.usePP);
        Application.targetFrameRate = SavingManager.GameSave.targetFPS;

        //TerrainGenerator.renderDst = SavingManager.GameSave.renderDst;

        FindObjectOfType<Camera>().fieldOfView = SavingManager.GameSave.fov;

        MouseSpeedField.onEndEdit.AddListener((string value) =>
        {
            bool parsedInput = float.TryParse(value, out float fValue);
            if (!parsedInput || fValue < 0.1f || fValue > 10f)
            {
                StartCoroutine(ITempText(MouseSpeedField, "Invalid!", SavingManager.GameSave.mouseSpeed.ToString()));
            }
            else
            {
                MouseLook.lateralSensitivity = fValue;
                MouseLook.verticalSensitivity = fValue;

                SavingManager.GameSave.mouseSpeed = fValue;
            }
        });
        MouseSmoothingField.onEndEdit.AddListener((string value) =>
        {
            bool parsedInput = float.TryParse(value, out float fValue);
            if (!parsedInput || fValue > 20)
            {
                StartCoroutine(ITempText(MouseSmoothingField, "Invalid!", SavingManager.GameSave.mouseSmoothing.ToString()));
            }
            else
            {
                MouseLook.smoothTime = fValue;
                SavingManager.GameSave.mouseSmoothing = fValue;
            }
        });
        MouseInvertField.onValueChanged.AddListener((bool value) =>
        {
            SavingManager.GameSave.mouseInverted = value;
            MouseLook.isInverted = value;
        });

        PPField.onValueChanged.AddListener((bool value) =>
        {
            SavingManager.GameSave.usePP = value;
            PP.SetActive(value);
        });
        AAField.onValueChanged.AddListener((bool value) =>
        {
            SavingManager.GameSave.useAA = value;
        });

        FPSTargetField.onEndEdit.AddListener((string value) =>
        {
            bool parsedInput = int.TryParse(value, out int fValue);
            if (!parsedInput || fValue > 240 || fValue < 24)
            {
                StartCoroutine(ITempText(FPSTargetField, "Invalid!", SavingManager.GameSave.targetFPS.ToString()));
            }
            else
            {
                Application.targetFrameRate = fValue;
                SavingManager.GameSave.targetFPS = fValue;
            }
        });

        RenderDstField.onEndEdit.AddListener((string value) =>
        {
            bool parsedInput = int.TryParse(value, out int fValue);
            if (!parsedInput || fValue < 2 || fValue > 10)
            {
                StartCoroutine(ITempText(RenderDstField, "Invalid!", SavingManager.GameSave.renderDst.ToString()));
            }
            else
            {
                //TerrainGenerator.renderDst = fValue;
                //TerrainGenerator.instance.UpdateChunks();

                SavingManager.GameSave.renderDst = fValue;
            }
        });

        FOVField.onEndEdit.AddListener((string value) =>
        {
            bool parsedInput = int.TryParse(value, out int fValue);
            if (!parsedInput)
            {
                StartCoroutine(ITempText(FOVField, "Invalid!", SavingManager.GameSave.fov.ToString()));
            }
            else
            {
               // FindObjectOfType<Camera>().fieldOfView = fValue;
                SavingManager.GameSave.fov = fValue;
            }
        });
    }

    IEnumerator ITempText(TMP_InputField inputField, string text, string endText)
    {
        inputField.text = text;
        yield return new WaitForSeconds(1f);
        inputField.text = endText;
    }

    #endregion
}

#region Enums & Public Classes

public class ItemReference
{
    public Block block;
    public Vector3 pos;
}

public enum GameStateEnum { InGame, Inventory, PauseMenu, SettingsMenu, Storage }
public enum GameVersionEnum { Test_1_0_0 = 0 }

#endregion