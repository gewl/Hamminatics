using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class EquippedCardsManager : SerializedMonoBehaviour {
    BoardStateManager boardStateManager;

    int maximumEquippedCards = 4;

    [SerializeField]
    CardData[] equippedCards;

    GameObject[] cardDisplays;
    Text[] cardTitles;

    private void Awake()
    {
        boardStateManager = GetComponentInParent<BoardStateManager>();

        cardDisplays = new GameObject[maximumEquippedCards];
        cardTitles = new Text[maximumEquippedCards];

        for (int i = 0; i < maximumEquippedCards; i++)
        {
            cardDisplays[i] = transform.GetChild(i).gameObject;
            cardTitles[i] = cardDisplays[i].GetComponentInChildren<Text>();
        }

        UpdateCardDisplays();
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
                cardTitles[i].text = equippedCards[i].Name;
                cardDisplays[i].GetComponent<Button>().interactable = true;
            }
        }
    }

    public void ShowAvailableCardPlays(int cardSlot)
    {
        boardStateManager.HighlightCardMoves(equippedCards[cardSlot]);
    }
}
