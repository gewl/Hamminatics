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

    public void UpdateDisplay(CardData card)
    {
        if (card == null)
        {
            cardImage.sprite = openSlotSprite;
            cardTitle.text = "Free Slot";
        }
        else
        {
            cardImage.sprite = card.cardImage;
            cardTitle.text = card.ID;
        }
    }
}
