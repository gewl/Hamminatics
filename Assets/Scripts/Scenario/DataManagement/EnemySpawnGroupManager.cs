using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

public class EnemySpawnGroupManager : SerializedMonoBehaviour {
    [SerializeField]
    List<EnemySpawnGroupData> eventScenarios;

    [SerializeField]
    float nodeDistanceVariance = 0.2f;

    List<EnemySpawnGroupData> cachedSpawnTable;
    int cachedDepth = -1;
    float probabilityTotalWeight;

    public EnemySpawnGroupData GetEnemySpawnGroup(int depth, float nodeDistance)
    {
        if (depth != cachedDepth)
        {
            List<EnemySpawnGroupData> newSpawnTable = DataRetriever.GetEnemySpawnGroups();
            if (newSpawnTable != null && newSpawnTable.Count > 0)
            {
                ValidateAndCacheSpawnTable(newSpawnTable);
                cachedDepth = depth;
            }
        }
        float scaledNodeDistance = probabilityTotalWeight * nodeDistance;
        float scaledVariance = Mathf.RoundToInt(probabilityTotalWeight * nodeDistanceVariance);
        float adjustedDistance = scaledNodeDistance + Random.Range(-scaledVariance, scaledVariance);

        adjustedDistance = Mathf.Max(0, adjustedDistance);
        adjustedDistance = Mathf.Min(probabilityTotalWeight, adjustedDistance);

        return PickSpawnGroup(adjustedDistance);
    }

    // Cribbed, with love, from http://hyperfoxstudios.com/loot-drop-table-implementation-unity3d/
    void ValidateAndCacheSpawnTable(List<EnemySpawnGroupData> newSpawnTable)
    {
        cachedSpawnTable = DataRetriever.GetEnemySpawnGroups();
        float currentProbabilityWeightTotal = 0f;

        for (int i = 0; i < cachedSpawnTable.Count; i++)
        {
            EnemySpawnGroupData spawnGroup = cachedSpawnTable[i];
            spawnGroup.probabilityRangeFrom = currentProbabilityWeightTotal;
            currentProbabilityWeightTotal += spawnGroup.probabilityWeight;
            spawnGroup.probabilityRangeTo = currentProbabilityWeightTotal;
        }

        probabilityTotalWeight = currentProbabilityWeightTotal;
    }

    EnemySpawnGroupData PickSpawnGroup(float pickedNumber)
    {
        foreach (EnemySpawnGroupData spawnGroup in cachedSpawnTable)
        {
            if (pickedNumber >= spawnGroup.probabilityRangeFrom && pickedNumber <= spawnGroup.probabilityRangeTo)
            {
                return spawnGroup;
            }
        }

        Debug.LogError("Spawn group couldn't be selected.");
        return cachedSpawnTable[0];
    }

    public EnemySpawnGroupData GetEventScenarioEnemySpawn(string enemySpawnID)
    {
        return eventScenarios.First(s => s.ID == enemySpawnID);
    }
}
