using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyManager : MonoBehaviour {

    [SerializeField]
    Sprite emptyEnergyBub;
    [SerializeField]
    Sprite fullEnergyBub;
    [SerializeField]
    Sprite translucentEnergyBub;

    Image[] energyBubs;

    int currentEnergy;
    int projectedEnergyGain = 0;

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

    public void UpdateProjectedEnergyGain(int energyGain)
    {
        projectedEnergyGain = energyGain;

        UpdateEnergyDisplay();
    }

    void RoundEndHandler(GameState updatedGameState)
    {
        currentEnergy += projectedEnergyGain;

        if (currentEnergy > 5)
        {
            currentEnergy = 5;
        }

        projectedEnergyGain = 0;

        UpdateEnergyDisplay();
    }

    void UpdateEnergyDisplay()
    {
        for (int i = 0; i < energyBubs.Length; i++)
        {
            if (i < currentEnergy)
            {
                energyBubs[i].sprite = fullEnergyBub;
            }
            else if (i < currentEnergy + projectedEnergyGain)
            {
                energyBubs[i].sprite = translucentEnergyBub;
            }
            else
            {
                energyBubs[i].sprite = emptyEnergyBub;
            }
        }

    }
}
