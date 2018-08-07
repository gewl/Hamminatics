using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreItemDisplay : MonoBehaviour {

    [SerializeField]
    Image itemImage;
    [SerializeField]
    Text itemNameText;
    [SerializeField]
    Text itemCostText;

    public void UpdateItemDisplay(Sprite sprite, string itemName, int itemCost)
    {
        gameObject.SetActive(true);

        itemImage.sprite = sprite;
        itemNameText.text = itemName;
        itemCostText.text = itemCost.ToString();
    }

    public void HideItemDisplay()
    {
        gameObject.SetActive(false);
    }
}
