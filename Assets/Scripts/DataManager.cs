using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour {

    const string ENTITY_DIR = "Data/Entities/";
    const string CARD_DIR = "Data/Cards/";

    static Dictionary<string, EntityData> cachedEntityData;
    static Dictionary<string, CardData> cachedCardData;

    private void Awake()
    {
        cachedEntityData = new Dictionary<string, EntityData>();
        cachedCardData = new Dictionary<string, CardData>();
    }

    public static EntityData GetEntityData(string entityName)
    {
        if (cachedEntityData.ContainsKey(entityName))
        {
            return cachedEntityData[entityName];
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
}
