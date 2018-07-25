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
    Sprite translucentHealthBub;

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
        int projectedHealth = currentPlayerHealth;
        if (upcomingStates.Count > 0)
        {
            projectedHealth = upcomingStates.Last().gameState.player.Health;
        }

        int lowerValue = Mathf.Min(currentPlayerHealth, projectedHealth);
        int higherValue = Mathf.Max(currentPlayerHealth, projectedHealth);

        for (int i = 0; i < healthBubs.Length; i++)
        {
            if (i < lowerValue)
            {
                healthBubs[i].sprite = fullHealthBub;
            }
            else if (i < higherValue)
            {
                healthBubs[i].sprite = translucentHealthBub;
            }
            else
            {
                healthBubs[i].sprite = emptyHealthBub;
            }
        }
    }
}
