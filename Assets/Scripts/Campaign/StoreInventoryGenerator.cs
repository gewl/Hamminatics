using System.Collections;
using System.Linq;
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

        while (!depthCardsMap.ContainsKey(depth))
        {
            depth--;
        }
        List<CardData> availableCardList = depthCardsMap[depth].ToList();

        newInventory[0] = GenerateSlotItem(ref availableCardList, 100 - chancesOfHealthInSlot1, healthItem);
        newInventory[1] = GenerateSlotItem(ref availableCardList, 100 - chancesOfUpgradeInSlot2, upgradeItem);
        newInventory[2] = GenerateSlotItem(ref availableCardList, chancesOfCardInSlot3, null);
        newInventory[3] = GenerateSlotItem(ref availableCardList, chancesOfCardInSlot4, null);

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

    CardData GenerateSlotItem(ref List<CardData> availableCards, int cardOdds, CardData nonCardOption)
    {
        int randomInt = rand.Next(1, 101);

        CardData result = nonCardOption;

        if (randomInt <= cardOdds)
        {
            result = availableCards.GetAndRemoveRandomElement();
        }

        return result;
    }
}
