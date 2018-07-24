using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplayController : MonoBehaviour {

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
        GameStateDelegates.OnCurrentGameStateChange += OnGameStateChange;
    }

    private void OnDisable()
    {
        GameStateDelegates.OnCurrentGameStateChange -= OnGameStateChange;
    }

    void OnGameStateChange(GameState updatedGameState, List<ProjectedGameState> upcomingStates)
    {
        int currentPlayerHealth = updatedGameState.player.Health;

        for (int i = 0; i < healthBubs.Length; i++)
        {
            if (i < currentPlayerHealth)
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
