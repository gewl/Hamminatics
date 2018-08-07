using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory {

    public int gold;
    public CardData[] equippedCards;

    public Inventory()
    {
        gold = 20;
        equippedCards = DataManager.GetBasicCards();
    }

    public Inventory(int _gold, CardData[] _equippedCards)
    {
        gold = _gold;
        equippedCards = _equippedCards;
    }

    public Inventory Copy()
    {
        return new Inventory(gold, equippedCards);
    }
}
