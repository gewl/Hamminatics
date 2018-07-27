using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanelHandler : MonoBehaviour {

    Text title;
    Text body;

    private void Awake()
    {
        title = transform.GetChild(0).GetComponent<Text>();
        body = transform.GetChild(1).GetComponent<Text>();
    }

    public void UpdateText(CardData card)
    {
        title.text = card.ID;
        string newBodyText = "Energy cost: " + card.Cost + "\n\n" + card.description;
        body.text = newBodyText;
    }

    public void UpdateText(EntityData entity)
    {
        title.text = entity.ID;
        body.text = entity.description;
    }

    public void UpdateText(ItemData item)
    {
        title.text = item.ID;
        body.text = item.description;
    }
}
