using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorePane : MonoBehaviour {
    StoreItemDisplay[] itemDisplays;

    [SerializeField]
    Button buyItemButton;
    [SerializeField]
    Text infoCardTitleText;
    [SerializeField]
    Text infoCardDescText;

    [SerializeField]
    CardData healthItem;
    [SerializeField]
    CardData upgradeItem;

    [SerializeField]
    CardData DEBUG_purchasableCard;

    CardData[] availableItems;
    int selectedCardIndex;

    private void Awake()
    {
        itemDisplays = GetComponentsInChildren<StoreItemDisplay>();
    }

    public void ShowStore(int depth)
    {
        gameObject.SetActive(true);
        buyItemButton.interactable = false;
        selectedCardIndex = -1;

        // TODO: Actually generate store inventory
        availableItems = new CardData[4]
        {
            healthItem,
            upgradeItem,
            DEBUG_purchasableCard,
            null
        };

        for (int i = 0; i < itemDisplays.Length; i++)
        {
            CardData availableItem = availableItems[i];

            if (availableItem == null)
            {
                itemDisplays[i].HideItemDisplay();
            }
            else
            {
                itemDisplays[i].UpdateItemDisplay(availableItem.cardImage, availableItem.ID, availableItem.baseGoldCost);
            }
        }
    }

    public void OnItemClick(int itemIndex)
    {
        Debug.Log("Item clicked: " + itemIndex);
    }
}
