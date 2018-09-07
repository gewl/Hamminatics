using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataRetriever : MonoBehaviour {

    const string ENTITY_DIR = "Data/Entities/";
    const string MODIFIER_DIR = "Data/Modifiers/";
    const string PLAYER_CARD_DIR = "Data/Cards/PlayerCards/";
    const string ENEMY_CARD_DIR = "Data/Cards/EnemyCards/";
    const string TRAP_DIR = "Data/Items/Traps/";
    const string TILE_DIR = "Sprites/Tiles/";

    const string DEPTH_DATA_DIR = "Data/DepthData/";
    const string DEPTH_FILE_PREFIX = "Depth_";

    const string GEN_MOVEMENT_CARD_PATH = "Data/Cards/InternalUse/_GenericMove";
    static CardData _genericMovementCard;
    static CardData GenericMovementCard
    {
        get
        {
            if (_genericMovementCard == null)
            {
                _genericMovementCard = Resources.Load<CardData>(GEN_MOVEMENT_CARD_PATH);
            }

            return _genericMovementCard;
        }
    }

    const string BASIC_ATTACK_ID = "Attack";
    const string BASIC_MOVE_ID = "Move";
    const string KINESIS_ID = "Kinesis";
    const string CARD4_ID = "InertialTwinge";

    static Dictionary<string, EntityData> cachedEntityData;
    static Dictionary<string, TrapData> cachedTrapData;
    static Dictionary<string, CardData> cachedCardData;
    static Dictionary<string, Sprite> cachedTileSprites;

    const string EVENTS_PATH = "Data/JSON/Events/";
    const string EVENTS_KEY = "events";

    static List<JSONObject> currentLayerEvents;
    static int currentDepth = -1;

    static DepthData currentDepthData;

    private void Awake()
    {
        cachedEntityData = new Dictionary<string, EntityData>();
        cachedCardData = new Dictionary<string, CardData>();
        cachedTileSprites = new Dictionary<string, Sprite>();
        cachedTrapData = new Dictionary<string, TrapData>();
    }

    private void OnEnable()
    {
        GameStateDelegates.OnCampaignStateUpdated += OnCampaignStateUpdate;
    }

    private void OnDisable()
    {
        GameStateDelegates.OnCampaignStateUpdated -= OnCampaignStateUpdate;
    }

    void OnCampaignStateUpdate(CampaignState newState)
    {
        if (newState.depth != currentDepth)
        {
            DepthData newDepthData = Resources.Load<DepthData>(DEPTH_DATA_DIR + DEPTH_FILE_PREFIX + newState.depth);

            if (newDepthData != null)
            {
                currentDepthData = newDepthData;
                currentDepth = newState.depth;
            }
        }
    }

    public static void UpdateDepthData(int newDepth)
    {
        if (currentDepth == newDepth)
        {
            return;
        }

        currentDepth = newDepth;

        DepthData newDepthData = Resources.Load<DepthData>(DEPTH_DATA_DIR + DEPTH_FILE_PREFIX + newDepth);
        if (newDepthData != null)
        {
            currentDepthData = newDepthData;
        }
    }

    public static int GetDepthGoldMultiplier()
    {
        return currentDepthData.goldValueMultiplier;
    }

    public static List<EnemySpawnGroupData> GetEnemySpawnGroups()
    {
        return currentDepthData.randomEnemySpawnPool;
    }

    public static List<CardData> GetRandomCardPool()
    {
        return currentDepthData.randomCardPool;
    }

    #region scriptable objects
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

    public static CardData GetPlayerCardData(string cardName, int currentDepth)
    {
        if (cachedEntityData.ContainsKey(cardName))
        {
            return cachedCardData[cardName];
        }

        CardData loadedCardData = Resources.Load<CardData>(PLAYER_CARD_DIR + DEPTH_FILE_PREFIX + currentDepth + "/" + cardName);
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
            null,
            null
        };
    }

    public static ModifierData GetModifier(string modifierFileName)
    {
        ModifierData modifierData = Resources.Load<ModifierData>(MODIFIER_DIR + modifierFileName);
        return Instantiate(modifierData);
    }
    #endregion

    #region sprites
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
    #endregion

    #region json/events
    public static JSONObject GetRandomEventByDepth(int depth)
    {
        if (depth != currentDepth || currentLayerEvents == null || currentLayerEvents.Count == 0)
        {
            TextAsset eventsText = Resources.Load<TextAsset>(EVENTS_PATH + depth.ToString());
            JSONObject layerEventObject = new JSONObject(eventsText.text);
            currentLayerEvents = layerEventObject[EVENTS_KEY].list;
            currentDepth = depth;
        }

        return currentLayerEvents.GetAndRemoveRandomElement();
    }
    #endregion
}
