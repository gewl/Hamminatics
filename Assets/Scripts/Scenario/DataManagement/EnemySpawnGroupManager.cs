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

    public EnemySpawnGroupData GetEnemySpawnGroup(int depth, float nodeDistance)
    {
        List<EnemySpawnGroupData> enemySpawnGroups = DataRetriever.GetEnemySpawnGroups();
        int groupsCount = enemySpawnGroups.Count;

        int scaledIndex = Mathf.RoundToInt(nodeDistance * groupsCount);
        System.Random rand = new System.Random();
        int scaledVariance = Mathf.RoundToInt(groupsCount * nodeDistanceVariance);
        int adjustedIndex = scaledIndex + rand.Next(-scaledVariance, scaledVariance);

        adjustedIndex = Mathf.Max(0, adjustedIndex);
        adjustedIndex = Mathf.Min(groupsCount, adjustedIndex);

        return enemySpawnGroups[adjustedIndex];
    }

    public EnemySpawnGroupData GetEventScenarioEnemySpawn(string enemySpawnID)
    {
        return eventScenarios.First(s => s.ID == enemySpawnID);
    }
}
