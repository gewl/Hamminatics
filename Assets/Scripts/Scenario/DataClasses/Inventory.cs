using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory {

    public int gold;
    public int artifacts;
    public CardData[] equippedCards;

    public Inventory()
    {
        gold = 5;
        artifacts = 0;
        equippedCards = DataRetriever.GetBasicCards();
    }

    public Inventory(int _gold, int _artifacts, CardData[] _equippedCards)
    {
        gold = _gold;
        artifacts = _artifacts;
        equippedCards = _equippedCards;
    }

    public Inventory Copy()
    {
        return new Inventory(gold, artifacts, equippedCards);
    }
}
