using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioHealthDisplay : MonoBehaviour {

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
        GameStateDelegates.OnCurrentScenarioStateChange += OnScenarioStateChange;
    }

    private void OnDisable()
    {
        GameStateDelegates.OnCurrentScenarioStateChange -= OnScenarioStateChange;
    }

    void OnScenarioStateChange(ScenarioState updatedScenarioState, List<ProjectedGameState> upcomingStates)
    {
        int currentPlayerHealth = updatedScenarioState.player.CurrentHealth;
        int projectedHealth = currentPlayerHealth;
        if (upcomingStates.Count > 0)
        {
            projectedHealth = upcomingStates.Last().scenarioState.player.CurrentHealth;
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
