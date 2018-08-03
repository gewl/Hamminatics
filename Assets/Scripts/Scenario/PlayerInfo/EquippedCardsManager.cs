using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class EquippedCardsManager : SerializedMonoBehaviour {
    [SerializeField]
    ScenarioStateManager scenarioStateManager;
    [SerializeField]
    EnergyManager energyManager;

    CardData[] equippedCards;

    int maximumEquippedCards;
    int selectedCardSlot = -1;

    GameObject[] cardDisplays;
    Button[] cardButtons;
    Text[] cardTitles;

    private void Awake()
    {
        maximumEquippedCards = Constants.MAX_EQUIPPED_CARDS;

        cardDisplays = new GameObject[maximumEquippedCards];
        cardButtons = new Button[maximumEquippedCards];
        cardTitles = new Text[maximumEquippedCards];

        for (int i = 0; i < maximumEquippedCards; i++)
        {
            cardDisplays[i] = transform.GetChild(i).gameObject;
            cardButtons[i] = cardDisplays[i].GetComponent<Button>();
            cardTitles[i] = cardDisplays[i].GetComponentInChildren<Text>();
        }
    }

    private void OnEnable()
    {
        GameStateDelegates.OnCurrentScenarioStateChange += UpdateEquippedCards;
    }

    private void OnDisable()
    {
        GameStateDelegates.OnCurrentScenarioStateChange -= UpdateEquippedCards;
    }

    public void ClearSelectedCard()
    {
        selectedCardSlot = -1;
        GameStateDelegates.OnCardDeselected();
        scenarioStateManager.ResetBoard();
    }

    public CardData GetSelectedCard()
    {
        return equippedCards[selectedCardSlot];
    }

    void UpdateEquippedCards(ScenarioState currentState, List<ProjectedGameState> upcomingStates)
    {
        equippedCards = currentState.inventory.equippedCards;

        UpdateCardDisplays(equippedCards);
    }

    void UpdateCardDisplays(CardData[] equippedCards)
    {
        for (int i = 0; i < maximumEquippedCards; i++)
        {
            CardData card = equippedCards[i];
            if (card == null)
            {
                cardTitles[i].text = "";
                cardButtons[i].interactable = false;
                continue;
            }

            cardTitles[i].text = card.ID;
            cardButtons[i].interactable = energyManager.CurrentEnergy >= card.cost;
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
        scenarioStateManager.HighlightPotentialCardTargets(equippedCards[cardSlot]);
    }
}
