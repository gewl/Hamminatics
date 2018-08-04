using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CampaignHealthDisplay : MonoBehaviour {

    [SerializeField]
    Sprite emptyHealthBub;
    [SerializeField]
    Sprite fullHealthBub;

    Image[] healthBubs;

    private void Awake()
    {
        healthBubs = GetComponentsInChildren<Image>();
    }

    private void OnEnable()
    {
        GameStateDelegates.OnCampaignStateUpdated += OnCampaignStateChange;
    }

    private void OnDisable()
    {
        GameStateDelegates.OnCampaignStateUpdated -= OnCampaignStateChange;
    }

    void OnCampaignStateChange(CampaignState newCampaignState)
    {
        int playerHealth = newCampaignState.player.CurrentHealth;
        for (int i = 0; i < healthBubs.Length; i++)
        {
            if (i < playerHealth)
            {
                healthBubs[i].sprite = fullHealthBub;
            }
            else
            {
                healthBubs[i].sprite = emptyHealthBub;
            }
        }
    }
}
