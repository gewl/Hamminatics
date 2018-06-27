using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyManager : MonoBehaviour {

    [SerializeField]
    GameStateManager gameStateManager;

    [SerializeField]
    Sprite emptyEnergyBub;
    [SerializeField]
    Sprite fullEnergyBub;

    Image[] energyBubs;

    int currentEnergy;
    public int ProjectedEnergyGain = 0;

    private void Awake()
    {
        energyBubs = GetComponentsInChildren<Image>();
    }

    private void OnEnable()
    {
        gameStateManager.OnTurnEnded += TurnEndHandler;
    }

    private void OnDisable()
    {
        gameStateManager.OnTurnEnded -= TurnEndHandler;
    }

    void TurnEndHandler(GameState updatedGameState)
    {
        currentEnergy += ProjectedEnergyGain;

        if (currentEnergy > 5)
        {
            currentEnergy = 5;
        }

        for (int i = 0; i < energyBubs.Length; i++)
        {
            if (i < currentEnergy)
            {
                energyBubs[i].sprite = fullEnergyBub;
            }
            else
            {
                energyBubs[i].sprite = emptyEnergyBub;
            }
        }

        ProjectedEnergyGain = 0;
    }
}
