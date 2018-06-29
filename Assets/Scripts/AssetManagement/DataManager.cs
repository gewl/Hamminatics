﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour {

    const string ENTITY_DIR = "Data/Entities/";
    const string CARD_DIR = "Data/Cards/";
    const string TILE_DIR = "Sprites/Tiles/";

    static Dictionary<string, EntityData> cachedEntityData;
    static Dictionary<string, CardData> cachedCardData;
    static Dictionary<string, Sprite> cachedTileSprites;

    private void Awake()
    {
        cachedEntityData = new Dictionary<string, EntityData>();
        cachedCardData = new Dictionary<string, CardData>();
        cachedTileSprites = new Dictionary<string, Sprite>();
    }

    public static EntityData GetEntityData(string entityName)
    {
        if (cachedEntityData.ContainsKey(entityName))
        {
            return Instantiate(cachedEntityData[entityName]);
        }
        else
        {
            EntityData loadedEntityData = Resources.Load<EntityData>(ENTITY_DIR + entityName);
            cachedEntityData[entityName] = loadedEntityData;
            return Instantiate(loadedEntityData);
        }
    }

    public static CardData GetCardData(string cardName)
    {
        if (cachedEntityData.ContainsKey(cardName))
        {
            return cachedCardData[cardName];
        }
        else
        {
            CardData loadedCardData = Resources.Load<CardData>(CARD_DIR + cardName);
            cachedCardData[cardName] = loadedCardData;
            return Instantiate(loadedCardData);
        }
    }

    public static Sprite GetTileSprite(string ID)
    {
        if (cachedTileSprites.ContainsKey(ID))
        {
            return cachedTileSprites[ID];
        }
        else
        {
            Sprite loadedTileSprite = Resources.Load<Sprite>(TILE_DIR + ID);
            cachedTileSprites[ID] = loadedTileSprite;
            return loadedTileSprite;
        }
    }
}
