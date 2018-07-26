﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour {

    const string ENTITY_DIR = "Data/Entities/";
    const string PLAYER_CARD_DIR = "Data/Cards/PlayerCards/";
    const string ENEMY_CARD_DIR = "Data/Cards/EnemyCards";
    const string TRAP_DIR = "Data/Items/Traps/";
    const string TILE_DIR = "Sprites/Tiles/";

    const string GEN_MOVEMENT_CARD_LOC = "Data/Cards/InternalUse/_GenericMove";
    static CardData _genericMovementCard;
    static CardData GenericMovementCard
    {
        get
        {
            if (_genericMovementCard == null)
            {
                _genericMovementCard = Resources.Load<CardData>(GEN_MOVEMENT_CARD_LOC);
            }

            return _genericMovementCard;
        }
    }

    const string BASIC_ATTACK_ID = "Attack";
    const string BASIC_MOVE_ID = "Move";
    const string FIREBALL_ID = "Fireball";

    static Dictionary<string, EntityData> cachedEntityData;
    static Dictionary<string, TrapData> cachedTrapData;
    static Dictionary<string, CardData> cachedCardData;
    static Dictionary<string, Sprite> cachedTileSprites;

    private void Awake()
    {
        cachedEntityData = new Dictionary<string, EntityData>();
        cachedCardData = new Dictionary<string, CardData>();
        cachedTileSprites = new Dictionary<string, Sprite>();
        cachedTrapData = new Dictionary<string, TrapData>();
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

    public static CardData GetGenericMovementCard()
    {
        return GenericMovementCard;
    }

    public static CardData GetPlayerCardData(string cardName)
    {
        if (cachedEntityData.ContainsKey(cardName))
        {
            return cachedCardData[cardName];
        }

        CardData loadedCardData = Resources.Load<CardData>(PLAYER_CARD_DIR + cardName);
        cachedCardData[cardName] = loadedCardData;
        return Instantiate(loadedCardData);
    }

    public static CardData GetEnemyCardData(string cardName)
    {
        if (cachedEntityData.ContainsKey(cardName))
        {
            return cachedCardData[cardName];
        }

        CardData loadedCardData = Resources.Load<CardData>(ENEMY_CARD_DIR + cardName);
        cachedCardData[cardName] = loadedCardData;
        return Instantiate(loadedCardData);
    }

    public static TrapData GetTrapData(string trapName)
    {
        if (cachedTrapData.ContainsKey(trapName))
        {
            return cachedTrapData[trapName];
        }
        
        TrapData loadedTrapData = Resources.Load<TrapData>(TRAP_DIR + trapName);
        cachedTrapData[trapName] = loadedTrapData;
        return Instantiate(loadedTrapData);
    }

    public static CardData[] GetBasicCards()
    {
        return new CardData[4]
        {
            GetPlayerCardData(BASIC_MOVE_ID),
            GetPlayerCardData(BASIC_ATTACK_ID),
            GetPlayerCardData(FIREBALL_ID),
            null
        };
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
