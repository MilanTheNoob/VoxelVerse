using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Is responsible for saving the game
/// </summary>
public class SavingManager : MonoBehaviour
{
    public static int ActiveSaveID;
    public static GamemodeEnum GameMode;

    public static GameSaveClass GameSave = new GameSaveClass();

    public static AdventureSaveClass AdventureSave;
    public static AdventureSaveClass SurvivalSave;

    const string GameSaveLoc = "GameData.dat";
    const bool useSaves = true;

    private void Awake()
    {
        transform.parent = null;
        DontDestroyOnLoad(this);

        Load();
    }

    public static void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/" + GameSaveLoc) && useSaves)
        {
            GameSave.LoadFromDisk(GameSaveLoc);
            Debug.Log("load");
        }
        else
        {
            GameSave.CreateNew();
            ActiveSaveID = 0;
        }
    }

    public static void LoadAdventureSave(int id)
    {
        Debug.Log("LOAD");
        GameSave.AdventureSaves[id].DatePlayed = DateTime.Now.ToString("MM/dd/yy");
        ActiveSaveID = id;

        AdventureSave = new AdventureSaveClass();
        AdventureSave.LoadFromDisk("AdventureSave" + id + ".dat");

        TerrainGenerator.CanGenerate = true;
        LeanTween.alpha(MenuManager.instance.FadingPanel, 1f, 0.5f).setOnComplete(() => { SceneManager.LoadScene(1, LoadSceneMode.Single); });
    }

    void OnApplicationQuit()
    {
        if (AdventureSave != null) AdventureSave.SaveToDisk("AdventureSave" + ActiveSaveID + ".dat");
        if (SurvivalSave != null) SurvivalSave.SaveToDisk("SurvivalSave" + ActiveSaveID + ".dat");

        Debug.Log("save game");
        GameSave.SaveToDisk(GameSaveLoc);
    }

}

public class GameSaveClass
{
    public GameVersionEnum GameVersion = GameVersionEnum.Test_1_0_0;

    public float mouseSpeed;
    public float mouseSmoothing;
    public bool mouseInverted;
    public bool usePP;
    public bool useAA;
    public float fov;
    public int targetFPS;
    public int renderDst;
    public bool animateChunks;

    public List<SaveDataClass> AdventureSaves = new List<SaveDataClass>();
    public List<SaveDataClass> SurvivalSaves = new List<SaveDataClass>();

    public void CreateNew()
    {
        mouseSpeed = 3;
        mouseSmoothing = 7;
        mouseInverted = false;

        usePP = true;
        useAA = true;
        fov = 100;

        targetFPS = 60;
        renderDst = 5;
        animateChunks = true;
    }

    public void LoadFromDisk(string saveName)
    {
        byte[] serializedData = File.ReadAllBytes(Application.persistentDataPath + "/" + saveName);
        int pos = 0;

        GameVersion = (GameVersionEnum)serializedData[pos]; pos++;

        AdventureSaves.Clear();
        SurvivalSaves.Clear();

        int adventureSaves = BitConverter.ToInt32(serializedData, pos); pos += 4;
        for (int i = 0; i < adventureSaves; i++) { AdventureSaves.Add(LoadSaveData(ref serializedData, ref pos)); }

        int survivalSaves = BitConverter.ToInt32(serializedData, pos); pos += 4;
        for (int i = 0; i < survivalSaves; i++) { SurvivalSaves.Add(LoadSaveData(ref serializedData, ref pos)); }

        mouseSpeed = BitConverter.ToSingle(serializedData, pos); pos += 4;
        mouseSmoothing = BitConverter.ToSingle(serializedData, pos); pos += 4;
        mouseInverted = BitConverter.ToBoolean(serializedData, pos); pos++;

        usePP = BitConverter.ToBoolean(serializedData, pos); pos++;
        useAA = BitConverter.ToBoolean(serializedData, pos); pos++;
        fov = BitConverter.ToSingle(serializedData, pos); pos++;

        targetFPS = BitConverter.ToInt32(serializedData, pos); pos += 4;
        renderDst = BitConverter.ToInt32(serializedData, pos); pos += 4;
        animateChunks = BitConverter.ToBoolean(serializedData, pos); pos++;
    }

    public SaveDataClass LoadSaveData(ref byte[] data, ref int pos)
    {
        Debug.Log("loaded a save data");
        SaveDataClass save = new SaveDataClass { Index = data[pos] };
        pos++;

        int nameLength = BitConverter.ToInt32(data, pos); pos += 4;
        save.Name = Encoding.ASCII.GetString(data, pos, nameLength); pos += nameLength;

        int dateLength = BitConverter.ToInt32(data, pos); pos += 4;
        save.DateCreated = Encoding.ASCII.GetString(data, pos, dateLength); pos += dateLength;

        dateLength = BitConverter.ToInt32(data, pos); pos += 4;
        save.DatePlayed = Encoding.ASCII.GetString(data, pos, dateLength); pos += dateLength;

        int imgLength = BitConverter.ToInt32(data, pos); pos += 4;
        /*save.Icon = Sprite.Create*/ pos += imgLength;

        return save;
    }

    public void SaveSaveData(ref List<byte> data, SaveDataClass saveData)
    {
        data.Add(saveData.Index);

        data.AddRange(BitConverter.GetBytes(saveData.Name.Length));
        data.AddRange(Encoding.ASCII.GetBytes(saveData.Name));

        data.AddRange(BitConverter.GetBytes(saveData.DateCreated.Length));
        data.AddRange(Encoding.ASCII.GetBytes(saveData.DatePlayed));

        data.AddRange(BitConverter.GetBytes(saveData.DatePlayed.Length));
        data.AddRange(Encoding.ASCII.GetBytes(saveData.DatePlayed));

        if (saveData.Icon == null) { data.AddRange(BitConverter.GetBytes(0)); }
        else
        {
            byte[] dat = saveData.Icon.texture.GetRawTextureData();
            data.AddRange(BitConverter.GetBytes(dat.Length));
            data.AddRange(dat);
        }
    }

    public void SaveToDisk(string saveName)
    {
        List<byte> serializedData = new List<byte>();
        serializedData.Add((byte)GameVersion);

        serializedData.AddRange(BitConverter.GetBytes(AdventureSaves.Count));
        for (int i = 0; i < AdventureSaves.Count; i++) { SaveSaveData(ref serializedData, AdventureSaves[i]); }

        serializedData.AddRange(BitConverter.GetBytes(SurvivalSaves.Count));
        for (int i = 0; i < SurvivalSaves.Count; i++) { SaveSaveData(ref serializedData, SurvivalSaves[i]); }

        serializedData.AddRange(BitConverter.GetBytes(mouseSpeed));
        serializedData.AddRange(BitConverter.GetBytes(mouseSmoothing));
        serializedData.AddRange(BitConverter.GetBytes(mouseInverted));

        serializedData.AddRange(BitConverter.GetBytes(usePP));
        serializedData.AddRange(BitConverter.GetBytes(useAA));
        serializedData.AddRange(BitConverter.GetBytes(fov));

        serializedData.AddRange(BitConverter.GetBytes(targetFPS));
        serializedData.AddRange(BitConverter.GetBytes(renderDst));
        serializedData.AddRange(BitConverter.GetBytes(animateChunks));

        File.WriteAllBytes(Application.persistentDataPath + "/" + saveName, serializedData.ToArray());
    }
}

public class SaveDataClass
{
    public byte Index;
    public string Name;

    public string DateCreated;
    public string DatePlayed;

    public Sprite Icon;
    public GameVersionEnum Version;
}

public class AdventureSaveClass
{
    public Dictionary<Vector2Int, BlockType[,,]> Chunks = new Dictionary<Vector2Int, BlockType[,,]>();

    public Vector3 PlayerPos;

    public class InventorySlotData
    {
        public BlockType Block;
        public int Quantity = 0;
    }

    public void CreateNew()
    {
        PlayerPos = new Vector3(0, 100, 0);
    }

    public void LoadFromDisk(string saveName)
    {
        byte[] serializedData = File.ReadAllBytes(Application.persistentDataPath + "/" + saveName);
        int pos = 16;

        GameObject.Find("Player").transform.position = new Vector3(BitConverter.ToSingle(serializedData, 0), 
            BitConverter.ToSingle(serializedData, 4) + 10, BitConverter.ToSingle(serializedData, 8));
        int chunksCount = BitConverter.ToInt32(serializedData, 12);

        Chunks.Clear();

        for (int i = 0; i < chunksCount; i++)
        {
            BlockType[,,] blocks = new BlockType[18, 255, 18];
            Vector2Int chunkLoc = new Vector2Int(BitConverter.ToInt32(serializedData, pos), BitConverter.ToInt32(serializedData, pos + 4));

            pos += 8;

            for (int x = 0; x < 18; x++)
            {
                for (int z = 0; z < 18; z++)
                {
                    for (int y = 0; y < 255; y++)
                    {
                        blocks[x, y, z] = (BlockType)serializedData[pos];
                        pos++;
                    }
                }
            }

            Chunks.Add(chunkLoc, blocks);
        }

        for (int i = 0; i < Inventory.Slots.Count; i++)
        {
            if (serializedData[pos] != 0) { Inventory.Slots[i].Item = Block.blocks[(BlockType)serializedData[pos]]; Debug.Log(Inventory.Slots[i].Item.blockReference.ToString()); }
            Inventory.Slots[i].Quantity = serializedData[pos + 1];

            Inventory.Slots[i].OnItemChange?.Invoke();
            pos += 2;
        }
        
        int storageCount = BitConverter.ToInt32(serializedData, pos); pos += 4;
        GameManager.Storages.Clear();
        Debug.Log(storageCount);

        for (int i = 0; i < storageCount; i++)
        {
            InventorySlot[] storageSlots = new InventorySlot[40];

            for (int j = 0; j < 40; j++)
            {
                InventorySlot storageSlot = new InventorySlot();

                if (serializedData[pos] != 0)
                {
                    storageSlot.Item = Block.blocks[(BlockType)serializedData[pos]]; pos++;
                    storageSlot.Quantity = serializedData[pos]; pos++;
                }
                else pos++;

                storageSlots[j] = storageSlot;
            }

            GameManager.Storages.Add(new Vector3Int(BitConverter.ToInt32(serializedData, pos), BitConverter.ToInt32(serializedData, pos + 4),
                BitConverter.ToInt32(serializedData, pos + 8)), storageSlots); pos += 12;
        }

        TerrainGenerator.playerPosOld = new Vector2Int(Mathf.FloorToInt(
                TerrainGenerator.Instance.viewer.position.x / 16), Mathf.FloorToInt(TerrainGenerator.Instance.viewer.position.z / 16));
        TerrainGenerator.CanGenerate = true;
        TerrainGenerator.UpdateVisibleChunks();
    }

    public void SaveToDisk(string saveName)
    {
        List<byte> serializedData = new List<byte>();
        GameObject Player = GameObject.Find("Player");

        serializedData.AddRange(BitConverter.GetBytes(Player.transform.position.x));
        serializedData.AddRange(BitConverter.GetBytes(Player.transform.position.y));
        serializedData.AddRange(BitConverter.GetBytes(Player.transform.position.z));

        serializedData.AddRange(BitConverter.GetBytes(Chunks.Count));

        for (int i = 0; i < Chunks.Count; i++)
        {
            serializedData.AddRange(BitConverter.GetBytes(Chunks.ElementAt(i).Key.x));
            serializedData.AddRange(BitConverter.GetBytes(Chunks.ElementAt(i).Key.y));

            BlockType[,,] blocks = Chunks.ElementAt(i).Value;

            for (int x = 0; x < 18; x++)
            {
                for (int z = 0; z < 18; z++)
                {
                    for (int y = 0; y < 255; y++)
                    {
                        serializedData.Add((byte)blocks[x, y, z]);
                    }
                }
            }
        }

        for (int i = 0; i < Inventory.Slots.Count; i++)
        {
            if (Inventory.Slots[i].Item == null)
            {
                serializedData.Add(0);
                serializedData.Add(0);
            }
            else
            {
                serializedData.Add((byte)Inventory.Slots[i].Item.blockReference);
                serializedData.Add((byte)Inventory.Slots[i].Quantity);
            }
        }

        serializedData.AddRange(BitConverter.GetBytes(GameManager.Storages.Count));
        for (int i = 0; i < GameManager.Storages.Count; i++)
        {
            for (int j = 0; j < 40; j++)
            {
                if (GameManager.Storages.ElementAt(i).Value[j].Quantity > 0)
                {
                    serializedData.Add((byte)GameManager.Storages.ElementAt(i).Value[j].Item.blockReference);
                    serializedData.Add((byte)GameManager.Storages.ElementAt(i).Value[j].Quantity);
                }
                else
                {
                    serializedData.Add(0);
                }
            }

            serializedData.AddRange(BitConverter.GetBytes(GameManager.Storages.ElementAt(i).Key.x));
            serializedData.AddRange(BitConverter.GetBytes(GameManager.Storages.ElementAt(i).Key.y));
            serializedData.AddRange(BitConverter.GetBytes(GameManager.Storages.ElementAt(i).Key.z));
        }

        File.WriteAllBytes(Application.persistentDataPath + "/" + saveName, serializedData.ToArray());
    }
}

