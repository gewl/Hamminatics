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
    GameObject costInfo;
    [SerializeField]
    Text infoCardGoldCostText;
    [SerializeField]
    Text infoCardEnergyCostText;

    [SerializeField]
    CardData healthItem;
    [SerializeField]
    CardData upgradeItem;

    [SerializeField]
    CardData DEBUG_purchasableCard;

    CardData[] availableItems;
    int selectedItemIndex;

    private void Awake()
    {
        itemDisplays = GetComponentsInChildren<StoreItemDisplay>();
    }

    public void ShowStore(int depth)
    {
        gameObject.SetActive(true);
        costInfo.SetActive(false);

        selectedItemIndex = -1;

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
        selectedItemIndex = itemIndex;

        costInfo.SetActive(true);
        CardData item = availableItems[selectedItemIndex];

        infoCardTitleText.text = item.ID;
        infoCardDescText.text = item.description;
        infoCardGoldCostText.text = item.baseGoldCost.ToString();
        infoCardEnergyCostText.text = item.energyCost.ToString();

        buyItemButton.interactable = GameStateManager.CurrentCampaign.inventory.gold >= item.baseGoldCost;
    }
}
