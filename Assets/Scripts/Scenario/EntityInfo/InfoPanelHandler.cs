using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanelHandler : MonoBehaviour {

    Text title;
    Text body;
    [SerializeField]
    ModifierDisplay modifierDisplay;

    private void Awake()
    {
        title = transform.GetChild(0).GetComponent<Text>();
        body = transform.GetChild(1).GetComponent<Text>();
    }

    public void DisplayCardInfo(CardData card)
    {
        title.text = card.ID;
        string newBodyText = "Energy cost: " + card.energyCost + "\n\n" + card.description;
        body.text = newBodyText;

        modifierDisplay.gameObject.SetActive(false);
    }

    public void DisplayEntityInfo(EntityData entity)
    {
        title.text = entity.ID;
        body.text = entity.description;

        modifierDisplay.gameObject.SetActive(true);
        modifierDisplay.UpdateDisplayedModifiers(entity.activeModifiers);
    }

    public void DisplayItemInfo(ItemData item)
    {
        title.text = item.ID;
        body.text = item.description;

        modifierDisplay.gameObject.SetActive(false);
    }
}
