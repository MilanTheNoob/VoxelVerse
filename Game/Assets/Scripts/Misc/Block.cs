using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The most important class in the game, the block
/// Responsible for storing and returning values of all blocks in the game
/// </summary>
public class Block
{
    const float sourceRes = 256;

    public float time;
    public bool isInteractable;
    public BlockType blockReference;

    public Color32 ItemColor;
    public Vector2[] uvs;

    public Action<Vector3Int> actionOnUse;
    public Action<Vector3Int> actionOnCreate;
    public Action<Vector3Int> actionOnDestroy;

    public Block(int posX, int posY, BlockType blockType, float timeToBreak = 1, Color32 color = new Color32(), int size = 8, 
        Action<Vector3Int> onUse = null, Action<Vector3Int> onCreate = null, Action<Vector3Int> onDestroy = null, bool interactable = false)
    {
        time = timeToBreak;
        ItemColor = color;
        blockReference = blockType;
        isInteractable = interactable;

        actionOnUse = onUse;
        actionOnCreate = onCreate;
        actionOnDestroy = onDestroy;

        uvs = new Vector2[]
            {
                new Vector2(posX / sourceRes + 0.001f, posY / sourceRes + 0.001f),
                new Vector2(posX / sourceRes + 0.001f, (posY + size) / sourceRes - 0.001f),
                new Vector2((posX + size) / sourceRes - 0.001f, (posY + size) / sourceRes - 0.001f),
                new Vector2((posX + size) / sourceRes - 0.001f, posY / sourceRes + 0.001f),
            };
    }

    public Vector2[] GetUVs() { return uvs; }

    /// <summary>
    /// An all important dictionary that will return all necessary information about every block in the game (except 'Air')
    /// Including actions for interactable blocks such as Barrels
    /// </summary>
    public static Dictionary<BlockType, Block> blocks = new Dictionary<BlockType, Block>()
    {
        { BlockType.Grass, new Block(16, 8, BlockType.Grass, 0.5f, new Color32(53, 122, 27, 255), 8) },
        { BlockType.Dirt, new Block(16, 0, BlockType.Dirt, 0.5f, new Color32(84, 61, 43, 255), 8) },
        { BlockType.Stone, new Block(24, 8, BlockType.Stone, 1f, new Color32(122, 127, 129, 255), 8) },

        { BlockType.Oak, new Block(32, 8, BlockType.Oak, 1f, new Color32(117, 94, 53, 255), 8) },
        { BlockType.OakLeaves, new Block(32, 16 , BlockType.OakLeaves, 0.2f, new Color32(21, 92, 19, 255), 8) },

        { BlockType.Spruce, new Block(32, 0, BlockType.Spruce, 1f, new Color32(116, 90, 54, 255), 8) },
        { BlockType.SpruceLeaves, new Block(32, 24, BlockType.SpruceLeaves, 0.2f, new Color32(19, 58, 18, 255), 8) },

        { BlockType.Birch, new Block(32, 32, BlockType.Birch, 1f, new Color32(210, 208, 184, 255), 8) },
        { BlockType.BirchLeaves, new Block(32, 40, BlockType.BirchLeaves, 0.2f, new Color32(80, 105, 44, 255), 8) },

        { BlockType.Jungle, new Block(32, 48, BlockType.Jungle, 1f, new Color32(180, 139, 92, 255), 8) },
        { BlockType.JungleLeaves, new Block(32, 56, BlockType.JungleLeaves, 0.2f, new Color32(25, 63, 24, 255), 8) },

        { BlockType.Barrel, new Block(40, 0, BlockType.Barrel, 1f, new Color32(48, 44, 41, 255), 8, OnOpenBarrel, OnCreateBarrel, OnDestroyBarrel, true) },
        { BlockType.Spacer, new Block(0, 0, BlockType.Spacer, 2f, new Color32(237, 237, 237, 255), 16) }
    };

    #region Block Functions

    public static void OnCreateBarrel(Vector3Int pos) 
    {
        if (!GameManager.Storages.ContainsKey(pos))
        {
            GameManager.Storages.Add(pos, new InventorySlot[40]);
            for (int i = 0; i < 40; i++) { GameManager.Storages[pos][i] = new InventorySlot(); }
        }
    }

    public static void OnOpenBarrel(Vector3Int pos)
    {
        if (GameManager.Storages.ContainsKey(pos))
        {
            GameManager.instance.OpenStorageMenu();

            for (int i = 0; i < GameManager.Storages[pos].Length; i++)
            {
                GameManager.instance.StorageChestSlots[i].SetInventorySlot(GameManager.Storages[pos][i]);
            }
        }
    }

    public static void OnDestroyBarrel(Vector3Int pos)
    {
        if (GameManager.Storages.ContainsKey(pos)) GameManager.Storages.Remove(pos);
    }

    #endregion
}

/// <summary>
/// An enum to reference any block you want
/// WARNING : Do not interfere with the order they are listed! Doing so will cause major problems!
/// ONLY ADD NEW ONES AT THE END IN ORDER OR SPECIFY THE NUMBER CORRELATING TO THE ITEM
/// </summary>
public enum BlockType 
{ 
    Air = 0, Spacer,

    Dirt, Grass, Stone, 
    
    Oak, OakLeaves,
    Spruce, SpruceLeaves,

    Barrel,

    Birch, BirchLeaves,
    Jungle, JungleLeaves
}