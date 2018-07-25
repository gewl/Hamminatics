using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class EquippedCardsManager : SerializedMonoBehaviour {
    [SerializeField]
    GameStateManager gameStateManager;

    CardData[] equippedCards;

    int maximumEquippedCards;
    int selectedCardSlot = -1;

    GameObject[] cardDisplays;
    Text[] cardTitles;

    private void Awake()
    {
        maximumEquippedCards = Constants.MAX_EQUIPPED_CARDS;

        cardDisplays = new GameObject[maximumEquippedCards];
        cardTitles = new Text[maximumEquippedCards];

        for (int i = 0; i < maximumEquippedCards; i++)
        {
            cardDisplays[i] = transform.GetChild(i).gameObject;
            cardTitles[i] = cardDisplays[i].GetComponentInChildren<Text>();
        }
    }

    private void OnEnable()
    {
        GameStateDelegates.OnCurrentGameStateChange += UpdateEquippedCards;
    }

    private void OnDisable()
    {
        GameStateDelegates.OnCurrentGameStateChange -= UpdateEquippedCards;
    }

    public void ClearSelectedCard()
    {
        selectedCardSlot = -1;
        GameStateDelegates.OnCardDeselected();
        gameStateManager.ResetBoard();
    }

    public CardData GetSelectedCard()
    {
        return equippedCards[selectedCardSlot];
    }

    void UpdateEquippedCards(GameState currentState, List<ProjectedGameState> upcomingStates)
    {
        equippedCards = currentState.inventory.equippedCards;

        UpdateCardDisplays(equippedCards);
    }

    void UpdateCardDisplays(CardData[] equippedCards)
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
            GameStateDelegates.OnCardDeselected();
            ClearSelectedCard();
        }
        else
        {
            selectedCardSlot = cardSlot;
            GameStateDelegates.OnCardSelected(equippedCards[cardSlot]);
            ShowAvailableCardPlays(cardSlot);
        }
    }

    void ShowAvailableCardPlays(int cardSlot)
    {
        gameStateManager.HighlightPotentialCardTargets(equippedCards[cardSlot]);
    }
}
