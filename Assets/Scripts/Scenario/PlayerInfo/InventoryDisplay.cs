using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryDisplay : MonoBehaviour {

    [SerializeField]
    GameObject goldDisplay;

    Text goldValueReadout;

    private void Awake()
    {
        goldValueReadout = goldDisplay.transform.GetChild(1).GetComponent<Text>();
    }

    private void OnEnable()
    {
        ScenarioStateDelegates.OnCurrentScenarioStateChange += HandleGameStateChange;
    }

    private void OnDisable()
    {
        ScenarioStateDelegates.OnCurrentScenarioStateChange -= HandleGameStateChange;
    }

    void HandleGameStateChange(ScenarioState currentGameState, List<ProjectedGameState> upcomingGameStates)
    {
        goldValueReadout.text = currentGameState.inventory.gold.ToString();
    }

}
