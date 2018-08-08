using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampaignCardDisplay : MonoBehaviour {

    CardRow[] cardRows;

    const string NO_CARD_TITLE = "NONE";
    const string NO_CARD_DESC = "No card equipped in this slot.";

    private void Awake()
    {
        cardRows = GetComponentsInChildren<CardRow>();
    }

    public void DisplayCards(CardData[] equippedCards)
    {
        gameObject.SetActive(true);
        for (int i = 0; i < equippedCards.Length; i++)
        {
            CardData card = equippedCards[i];
            CardRow cardRow = cardRows[i];

            string cardTitle = NO_CARD_TITLE;
            Sprite cardImage = null;
            string cardDesc = NO_CARD_DESC;
            if (card != null)
            {
                cardTitle = card.ID;
                cardImage = card.cardImage;
                cardDesc = card.description + "\n" + card.GetStatText();
            }

            cardRow.UpdateCardRow(cardTitle, cardImage, cardDesc);
        }
    }

}
