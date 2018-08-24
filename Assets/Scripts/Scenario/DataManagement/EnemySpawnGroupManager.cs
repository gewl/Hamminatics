using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

public class EnemySpawnGroupManager : SerializedMonoBehaviour {

    // Explicitly keyed to depth:int, but each list is also sorted by difficulty.
    // The later a group is in the list, the more difficult it is.
    // Later nodes on a given map should generally retrieve later spawn groups from their
    // corresponding list, for a sort of smudgey increase in difficulty as players progress.
    [SerializeField]
    Dictionary<int, List<EnemySpawnGroupData>> depthSpawnGroupsMap;

    [SerializeField]
    List<EnemySpawnGroupData> eventScenarios;

    [SerializeField]
    float nodeDistanceVariance = 0.2f;

    List<EnemySpawnGroupData> GetEnemySpawnGroups(int depth)
    {
        if (!depthSpawnGroupsMap.ContainsKey(depth))
        {
            Debug.LogError("Depth spawn groups not found for depth " + depth);
            return depthSpawnGroupsMap[1];
        }
        return depthSpawnGroupsMap[depth];
    }

    public EnemySpawnGroupData GetEnemySpawnGroup(int depth, float nodeDistance)
    {
        List<EnemySpawnGroupData> enemySpawnGroups = GetEnemySpawnGroups(depth);
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
