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
        GameStateDelegates.OnCurrentGameStateChange += HandleGameStateChange;
    }

    private void OnDisable()
    {
        GameStateDelegates.OnCurrentGameStateChange -= HandleGameStateChange;
    }

    void HandleGameStateChange(GameState currentGameState, List<ProjectedGameState> upcomingGameStates)
    {
        goldValueReadout.text = currentGameState.inventory.gold.ToString();
    }

}
