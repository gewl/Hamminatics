using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour {

    const string ENTITY_DIR = "Data/Entities/";
    const string CARD_DIR = "Data/Cards/";
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

    public static CardData GetGenericMovementCard()
    {
        return GenericMovementCard;
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

    public static CardData[] GetBasicCards()
    {
        return new CardData[4]
        {
            GetCardData(BASIC_MOVE_ID),
            GetCardData(BASIC_ATTACK_ID),
            null,
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
