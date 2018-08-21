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

    public List<EnemySpawnGroupData> GetEnemySpawnGroups(int depth)
    {
        if (!depthSpawnGroupsMap.ContainsKey(depth))
        {
            Debug.LogError("Depth spawn groups not found for depth " + depth);
            return depthSpawnGroupsMap[1];
        }
        return depthSpawnGroupsMap[depth];
    }

    public EnemySpawnGroupData GetEventScenarioEnemySpawn(string enemySpawnID)
    {
        return eventScenarios.First(s => s.ID == enemySpawnID);
    }
}
