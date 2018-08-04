using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardRow : MonoBehaviour {

    [SerializeField]
    Text cardName;
    [SerializeField]
    Image cardImage;
    [SerializeField]
    Text cardDescription;

    public void UpdateCardRow(string cardTitle, Sprite _cardImage, string cardDesc)
    {
        cardName.text = cardTitle;
        cardImage.sprite = _cardImage;
        cardDescription.text = cardDesc;
    }
}
