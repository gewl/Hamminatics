using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyManager : MonoBehaviour {

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
        GameStateDelegates.OnRoundEnded += RoundEndHandler;
    }

    private void OnDisable()
    {
        GameStateDelegates.OnRoundEnded -= RoundEndHandler;
    }

    void RoundEndHandler(GameState updatedGameState)
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
