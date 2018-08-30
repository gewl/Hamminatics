using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotDisplay : MonoBehaviour {

    [SerializeField]
    Image cardImage;
    [SerializeField]
    Text cardTitle;
    [SerializeField]
    Sprite openSlotSprite;
    [SerializeField]
    GameObject blockInteractionOverlay;

    public void UpdateDisplay_NewCard(CardData card)
    {
        if (card == null)
        {
            cardImage.sprite = openSlotSprite;
            cardTitle.text = "Free Slot";
            blockInteractionOverlay.SetActive(false);
        }
        else
        {
            cardImage.sprite = card.cardImage;
            cardTitle.text = card.ID;

            blockInteractionOverlay.SetActive(card.category == CardCategory.Movement);
        }
    }

    public void UpdateDisplay_UpdateInvalid(CardData card)
    {
        cardImage.sprite = card.cardImage;
        cardTitle.text = card.ID;

        blockInteractionOverlay.SetActive(true);
    }

    public void UpdateDisplay_Update(CardData card)
    {
        if (card == null)
        {
            cardImage.sprite = openSlotSprite;
            cardTitle.text = "No Card In Slot";
            blockInteractionOverlay.SetActive(true);
        }
        else
        {
            cardImage.sprite = card.cardImage;
            cardTitle.text = card.ID;

            blockInteractionOverlay.SetActive(false);
        }
    }
}
