using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class EquippedCardsManager : SerializedMonoBehaviour {
    GameStateManager boardStateManager;

    int maximumEquippedCards = 4;
    int selectedCardSlot = -1;

    [SerializeField]
    CardData[] equippedCards;

    GameObject[] cardDisplays;
    Text[] cardTitles;

    private void Awake()
    {
        boardStateManager = GetComponentInParent<GameStateManager>();

        cardDisplays = new GameObject[maximumEquippedCards];
        cardTitles = new Text[maximumEquippedCards];

        for (int i = 0; i < maximumEquippedCards; i++)
        {
            cardDisplays[i] = transform.GetChild(i).gameObject;
            cardTitles[i] = cardDisplays[i].GetComponentInChildren<Text>();
        }

        UpdateCardDisplays();
    }

    public void ClearSelectedCard()
    {
        selectedCardSlot = -1;
        boardStateManager.ResetBoard();
    }


    public CardData GetSelectedCard()
    {
        return equippedCards[selectedCardSlot];
    }

    void UpdateCardDisplays()
    {
        for (int i = 0; i < maximumEquippedCards; i++)
        {
            if (equippedCards[i] == null)
            {
                cardTitles[i].text = "";
                cardDisplays[i].GetComponent<Button>().interactable = false;
            }
            else
            {
                cardTitles[i].text = equippedCards[i].ID;
                cardDisplays[i].GetComponent<Button>().interactable = true;
            }
        }
    }

    public void OnEquippedCardClick(int cardSlot)
    {
        if (selectedCardSlot == cardSlot)
        {
            Debug.Log(selectedCardSlot + " " + cardSlot);
            ClearSelectedCard();
        }
        else
        {
            selectedCardSlot = cardSlot;
            ShowAvailableCardPlays(cardSlot);
        }
    }

    void ShowAvailableCardPlays(int cardSlot)
    {
        boardStateManager.HighlightPotentialCardTargets(equippedCards[cardSlot]);
    }
}
