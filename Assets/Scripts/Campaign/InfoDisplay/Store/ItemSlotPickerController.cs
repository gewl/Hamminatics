using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotPickerController : MonoBehaviour {

    CardData newCard;
    int selectedSlot = -1;
    bool isChoosingUpgrade = false;

    const string DEFAULT_PREFIX = "Choose a slot for ";

    const string MOVE_PREFIX = "Can't replace ";
    const string REPLACE_PREFIX = "Replace ";
    const string FREE_SLOT = "Free Slot";

    const string INELIGIBLE_TEXT = "You can't place a new card in that slot.";
    const string REPLACE_PROMPT_PREFIX = "Do you want to replace ";
    const string PLACE_PROMPT_PREFIX = "Place in slot ";
    const string NEW_CARD_TITLE_PREFIX = "Pick a slot for ";

    const string UPGRADE_TEXT = "Choose a card to upgrade.";
    const string UPGRADE_PROMPT_PREFIX = "Do you want to upgrade ";
    const string UPGRADE_TITLE_TEXT = "Choose a card to upgrade:";

    InventorySlotDisplay[] slotDisplays;

    [SerializeField]
    Text titleText;
    [SerializeField]
    Text confirmationText;
    [SerializeField]
    Button confirmButton;
    [SerializeField]
    Button denyButton;
    [SerializeField]
    GameObject skipCard;

    private void Awake()
    {
        slotDisplays = GetComponentsInChildren<InventorySlotDisplay>();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    #region for upgrade

    public void DisplaySlotPickerForUpgrade()
    {
        skipCard.SetActive(false);

        isChoosingUpgrade = true;
        gameObject.SetActive(true);
        ReturnToUpgradeDefault();
        titleText.text = UPGRADE_TITLE_TEXT;

        for (int i = 0; i < slotDisplays.Length; i++)
        {
            CardData card = GameStateManager.CurrentCampaign.inventory.equippedCards[i];
            slotDisplays[i].UpdateDisplay_Update(card);
        }
    }

    void ReturnToUpgradeDefault()
    {
        confirmationText.text = UPGRADE_TEXT;
        confirmButton.gameObject.SetActive(false);
        denyButton.gameObject.SetActive(false);
        selectedSlot = -1;
    }

    void PickSlot_Upgrade(int slot)
    {
        selectedSlot = slot;
        CardData cardInSlot = GameStateManager.CurrentCampaign.inventory.equippedCards[slot];

        if (cardInSlot == null)
        {
            ReturnToUpgradeDefault();
            return;
        }

        confirmationText.text = UPGRADE_PROMPT_PREFIX + cardInSlot.ID + "?\n" + cardInSlot.GetUpgradeText();
        confirmButton.gameObject.SetActive(true);
        denyButton.gameObject.SetActive(true);
    }

    void ConfirmPick_Upgrade()
    {
        CardData cardInSlot = GameStateManager.CurrentCampaign.inventory.equippedCards[selectedSlot];
        cardInSlot.Upgrade();
        gameObject.SetActive(false);
    }
    #endregion

    #region for new card
    public void OfferCard(CardData _newCard, bool isInStore = false)
    {
        skipCard.SetActive(!isInStore);

        DisplaySlotPicker(_newCard);
    }

    void DisplaySlotPicker(CardData _newCard)
    {
        isChoosingUpgrade = false;
        newCard = _newCard;
        gameObject.SetActive(true);
        ReturnToNewCardDefault();
        titleText.text = NEW_CARD_TITLE_PREFIX + newCard.ID + ":";

        for (int i = 0; i < slotDisplays.Length; i++)
        {
            CardData card = GameStateManager.CurrentCampaign.inventory.equippedCards[i];
            slotDisplays[i].UpdateDisplay_NewCard(card);
        }
    }

    void PickSlot_NewCard(int slot)
    {
        if (slot == 0)
        {
            confirmationText.text = INELIGIBLE_TEXT;
            confirmButton.gameObject.SetActive(false);
            denyButton.gameObject.SetActive(false);
        }
        else
        {
            selectedSlot = slot;
            CardData cardInSlot = GameStateManager.CurrentCampaign.inventory.equippedCards[slot];

            if (cardInSlot == null)
            {
                confirmationText.text = PLACE_PROMPT_PREFIX + slot + "?";
                confirmButton.gameObject.SetActive(true);
                denyButton.gameObject.SetActive(true);
            }
            else
            {
                confirmationText.text = REPLACE_PROMPT_PREFIX + cardInSlot.ID + "?";
                confirmButton.gameObject.SetActive(true);
                denyButton.gameObject.SetActive(true);
            }
        }
    }

    public void ReturnToNewCardDefault()
    {
        confirmationText.text = DEFAULT_PREFIX + newCard.ID;
        confirmButton.gameObject.SetActive(false);
        denyButton.gameObject.SetActive(false);
        selectedSlot = -1;
    }


    public void ConfirmPick_NewCard()
    {
        GameStateManager.CurrentCampaign.inventory.equippedCards[selectedSlot] = newCard;
        gameObject.SetActive(false);
    }
    #endregion

    public void PickSlot(int slot)
    {
        if (!isChoosingUpgrade)
        {
            PickSlot_NewCard(slot);
        }
        else
        {
            PickSlot_Upgrade(slot);
        }
    }

    public void ReturnToDefault()
    {
        if (!isChoosingUpgrade)
        {
            ReturnToNewCardDefault();
        }
        else
        {
            ReturnToUpgradeDefault();
        }
    }

    public void ConfirmPick()
    {
        if (!isChoosingUpgrade)
        {
            ConfirmPick_NewCard();
        }
        else
        {
            ConfirmPick_Upgrade();
        }
    }
}
