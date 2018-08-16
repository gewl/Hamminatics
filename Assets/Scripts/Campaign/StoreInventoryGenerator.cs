using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class StoreInventoryGenerator : SerializedMonoBehaviour {
    [SerializeField]
    CardData healthItem;
    [SerializeField]
    CardData upgradeItem;

    [SerializeField]
    Dictionary<int, List<CardData>> depthCardsMap;

    [SerializeField, Range(1, 100)]
    int chancesOfHealthInSlot1;
    [SerializeField, Range(1, 100)]
    int chancesOfUpgradeInSlot2;
    [SerializeField, Range(1, 100)]
    int chancesOfCardInSlot3;
    [SerializeField, Range(1, 100)]
    int chancesOfCardInSlot4;

    System.Random rand;

    private void Awake()
    {
        rand = new System.Random();
    }

    public CardData[] GenerateStoreInventory(int depth)
    {
        CardData[] newInventory = new CardData[4];

        return newInventory;
    }

    public string GetHealthID()
    {
        return healthItem.ID;
    }

    public string GetUpgradeID()
    {
        return upgradeItem.ID;
    }
}
