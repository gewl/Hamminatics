using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryDisplay : MonoBehaviour {

    [SerializeField]
    Text goldValueReadout;
    [SerializeField]
    Text artifactValueReadout;

    private void OnEnable()
    {
        GameStateDelegates.OnCurrentScenarioStateChange += HandleScenarioStateChange;
        GameStateDelegates.OnCampaignStateUpdated += HandleCampaignStateChange;
    }

    private void OnDisable()
    {
        GameStateDelegates.OnCurrentScenarioStateChange -= HandleScenarioStateChange;
        GameStateDelegates.OnCampaignStateUpdated -= HandleCampaignStateChange;
    }

    void HandleScenarioStateChange(ScenarioState currentGameState, List<ProjectedGameState> upcomingGameStates)
    {
        goldValueReadout.text = currentGameState.inventory.gold.ToString();
        artifactValueReadout.text = currentGameState.inventory.artifacts.ToString();
    }

    void HandleCampaignStateChange(CampaignState newCampaignState)
    {
        goldValueReadout.text = newCampaignState.inventory.gold.ToString();
        artifactValueReadout.text = newCampaignState.inventory.artifacts.ToString();
    }

}
