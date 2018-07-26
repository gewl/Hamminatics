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
    Sprite projectedGainEnergyBub;
    [SerializeField]
    Sprite projectedLossEnergyBub;

    Image[] energyBubs;

    public int CurrentEnergy { get; private set; }
    int projectedEnergyGain = 0;
    int projectedEnergyLoss = 0;

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
        if (energyGain < 0)
        {
            energyGain = 0;
        }
        projectedEnergyGain = energyGain;

        UpdateEnergyDisplay();
    }

    public void UpdateProjectedEnergyCost(int energyCost)
    {
        if (energyCost > CurrentEnergy)
        {
            Debug.LogError("Trying to use an ability w/ greater cost than current energy.");
            return;
        }

        projectedEnergyLoss = energyCost;
        UpdateEnergyDisplay();
    }

    void RoundEndHandler(GameState updatedGameState)
    {
        CurrentEnergy = CalculateUpdatedEnergy(CurrentEnergy, projectedEnergyGain, projectedEnergyLoss);

        projectedEnergyGain = 0;
        projectedEnergyLoss = 0;

        UpdateEnergyDisplay();
    }

    void UpdateEnergyDisplay()
    {
        int projectedEnergyAmount = CalculateUpdatedEnergy(CurrentEnergy, projectedEnergyGain, projectedEnergyLoss);

        if (projectedEnergyAmount > CurrentEnergy)
        {
            DrawEnergyDisplayWithGain(CurrentEnergy, projectedEnergyAmount);
        }
        else
        {
            DrawEnergyDisplayWithLoss(CurrentEnergy, projectedEnergyAmount);
        }
    }

    void DrawEnergyDisplayWithGain(int currentEnergy, int projectedEnergy)
    {
        for (int i = 0; i < energyBubs.Length; i++)
        {
            if (i < CurrentEnergy)
            {
                energyBubs[i].sprite = fullEnergyBub;
            }
            else if (i < projectedEnergy)
            {
                energyBubs[i].sprite = projectedGainEnergyBub;
            }
            else
            {
                energyBubs[i].sprite = emptyEnergyBub;
            }
        }
    }

    void DrawEnergyDisplayWithLoss(int currentEnergy, int projectedEnergy)
    {
        for (int i = 0; i < energyBubs.Length; i++)
        {
            if (i < projectedEnergy)
            {
                energyBubs[i].sprite = fullEnergyBub;
            }
            else if (i < CurrentEnergy)
            {
                energyBubs[i].sprite = projectedLossEnergyBub;
            }
            else
            {
                energyBubs[i].sprite = emptyEnergyBub;
            }
        }
    }

    int CalculateUpdatedEnergy(int currentEnergy, int projectedGain, int projectedLoss)
    {
        currentEnergy -= projectedLoss;

        currentEnergy = Mathf.Max(0, currentEnergy);
        
        currentEnergy += projectedGain;

        return Mathf.Min(5, currentEnergy);
    }
}
