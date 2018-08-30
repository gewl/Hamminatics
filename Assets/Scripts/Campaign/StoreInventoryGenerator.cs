using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class StoreInventoryGenerator : SerializedMonoBehaviour {
    [SerializeField]
    GameStateManager gameStateManager;

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
        List<CardData> availableCardList = DataRetriever.GetRandomCardPool().ToList();

        newInventory[0] = GenerateSlotItem(ref availableCardList, 100 - chancesOfHealthInSlot1, gameStateManager.HealthCard);
        newInventory[1] = GenerateSlotItem(ref availableCardList, 100 - chancesOfUpgradeInSlot2, gameStateManager.UpgradeCard);
        newInventory[2] = GenerateSlotItem(ref availableCardList, chancesOfCardInSlot3, null);
        newInventory[3] = GenerateSlotItem(ref availableCardList, chancesOfCardInSlot4, null);

        return newInventory;
    }

    public string GetHealthID()
    {
        return gameStateManager.HealthCard.ID;
    }

    public string GetUpgradeID()
    {
        return gameStateManager.UpgradeCard.ID;
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
