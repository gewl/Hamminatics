using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class StorePane : MonoBehaviour {
    StoreItemDisplay[] itemDisplays;

    [SerializeField]
    StoreInventoryGenerator inventoryGenerator;
    [SerializeField]
    ItemSlotPickerController slotPicker;

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

    string _healthItemID;
    string HealthItemID
    {
        get
        {
            if (_healthItemID == null)
            {
                _healthItemID = inventoryGenerator.GetHealthID();
            }
            return _healthItemID;
        }
    }
    string _upgradeItemID;
    string UpgradeItemID
    {
        get
        {
            if (_upgradeItemID == null)
            {
                _upgradeItemID = inventoryGenerator.GetUpgradeID();
            }
            return _upgradeItemID;
        }
    }

    [SerializeField]
    CardData DEBUG_purchasableCard;

    CardData[] availableItems;
    int selectedItemIndex;

    const string ITEM_BOUGHT_TITLE = "Very good!";
    const string ITEM_BOUGHT_DESC = "Anything else I can get for you?";

    private void Awake()
    {
        itemDisplays = GetComponentsInChildren<StoreItemDisplay>();
    }

    void ResetInfoPane()
    {
        selectedItemIndex = -1;

        costInfo.SetActive(false);

        infoCardTitleText.text = ITEM_BOUGHT_TITLE;
        infoCardDescText.text = ITEM_BOUGHT_DESC;
        buyItemButton.interactable = false;
    }

    public void ShowStore(int depth)
    {
        gameObject.SetActive(true);
        costInfo.SetActive(false);
        buyItemButton.interactable = false;

        selectedItemIndex = -1;

        availableItems = inventoryGenerator.GenerateStoreInventory(depth);

        DrawItems();
    }

    void DrawItems()
    {
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

        CampaignState currentCampaign = GameStateManager.CurrentCampaign;
        bool isItemHealthAndPlayerHealthy = item.ID == HealthItemID && 
            currentCampaign.player.CurrentHealth == currentCampaign.player.MaxHealth;
        buyItemButton.interactable = currentCampaign.inventory.gold >= item.baseGoldCost && !isItemHealthAndPlayerHealthy;
    }

    public void OnBuyItem()
    {
        CardData item = availableItems[selectedItemIndex];

        availableItems[selectedItemIndex] = null;
        DrawItems();
        ResetInfoPane();

        // TODO: Bake this into extension method or something.
        GameStateManager.CurrentCampaign.inventory.gold -= item.baseGoldCost;

        if (item.ID == HealthItemID)
        {
            GameStateManager.CurrentCampaign.player.ChangeHealthValue(1);
        }
        else if (item.ID == UpgradeItemID)
        {
            slotPicker.DisplaySlotPickerForUpgrade();
        }
        else
        {
            slotPicker.OfferCard(item, true);
        }
        GameStateDelegates.OnCampaignStateUpdated(GameStateManager.CurrentCampaign);
    }
}
