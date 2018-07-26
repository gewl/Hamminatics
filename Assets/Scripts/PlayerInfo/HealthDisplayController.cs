using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplayController : MonoBehaviour {

    [SerializeField]
    Sprite emptyHealthBub;
    [SerializeField]
    Sprite fullHealthBub;
    [SerializeField]
    Sprite projectedHealthGainBub;
    [SerializeField]
    Sprite projectedHealthLossBub;

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
        int currentPlayerHealth = updatedGameState.player.CurrentHealth;
        int projectedHealth = currentPlayerHealth;
        if (upcomingStates.Count > 0)
        {
            projectedHealth = upcomingStates.Last().gameState.player.CurrentHealth;
        }

        if (projectedHealth > currentPlayerHealth)
        {
            DrawHealthDisplayWithGain(currentPlayerHealth, projectedHealth);
        }
        else
        {
            DrawHealthDisplayWithLoss(currentPlayerHealth, projectedHealth);
        }
    }

    void DrawHealthDisplayWithGain(int currentHealth, int projectedHealth)
    {
        for (int i = 0; i < healthBubs.Length; i++)
        {
            if (i < currentHealth)
            {
                healthBubs[i].sprite = fullHealthBub;
            }
            else if (i < projectedHealth)
            {
                healthBubs[i].sprite = projectedHealthGainBub;
            }
            else
            {
                healthBubs[i].sprite = emptyHealthBub;
            }
        }
    }

    void DrawHealthDisplayWithLoss(int currentHealth, int projectedHealth)
    {
        for (int i = 0; i < healthBubs.Length; i++)
        {
            if (i < projectedHealth)
            {
                healthBubs[i].sprite = fullHealthBub;
            }
            else if (i < currentHealth)
            {
                healthBubs[i].sprite = projectedHealthLossBub;
            }
            else
            {
                healthBubs[i].sprite = emptyHealthBub;
            }
        }
    }
}
