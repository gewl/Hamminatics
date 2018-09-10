using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterInfoPane : MonoBehaviour {

    [Header("Health Display")]
    [SerializeField]
    Sprite emptyHealthBub;
    [SerializeField]
    Sprite fullHealthBub;
    [SerializeField]
    Transform healthBubsParent;

    [Header("Inventory Display")]
    [SerializeField]
    Text goldAmountText;
    [SerializeField]
    CampaignCardDisplay cardDisplayPane;

    [Header("Miscellaneous")]
    [SerializeField]
    Text depthText;

    Image[] healthBubs;

    private void Awake()
    {
        healthBubs = healthBubsParent.GetComponentsInChildren<Image>();
    }

    private void OnEnable()
    {
        GameStateDelegates.OnCampaignStateUpdated += OnCampaignStateChange;
    }

    private void OnDisable()
    {
        GameStateDelegates.OnCampaignStateUpdated -= OnCampaignStateChange;
    }

    #region propagating state change information
    void OnCampaignStateChange(CampaignState newCampaignState)
    {
        UpdateHealthDisplay(newCampaignState.player.CurrentHealth);
        UpdateInventoryDisplay(newCampaignState.inventory);

        depthText.text = newCampaignState.depth.ToString();
    }

    void UpdateHealthDisplay(int currentHealth)
    {
        for (int i = 0; i < healthBubs.Length; i++)
        {
            if (i < currentHealth)
            {
                healthBubs[i].sprite = fullHealthBub;
            }
            else
            {
                healthBubs[i].sprite = emptyHealthBub;
            }
        }

    }

    void UpdateInventoryDisplay(Inventory currentInventory)
    {
        goldAmountText.text = currentInventory.gold.ToString();
    }
    #endregion

    #region button handlers
    public void ShowCards()
    {
        cardDisplayPane.DisplayCards(GameStateManager.CurrentCampaign.inventory.equippedCards);
        GameStateManager.SetDim(true);
        GameStateManager.ActivateFullScreenTrigger((BaseEventData data) => HideCards());
    }

    public void HideCards()
    {
        cardDisplayPane.gameObject.SetActive(false);
        GameStateManager.SetDim(false);

        GameStateManager.DisableFullScreenTrigger();
    }

    #endregion
}