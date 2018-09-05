using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy Spawn Group")]
public class EnemySpawnGroupData : ScriptableObject {

    public string ID;
    public float probabilityWeight = 1f;

    [HideInInspector]
    public float probabilityRangeFrom;
    [HideInInspector]
    public float probabilityRangeTo;

    [SerializeField]
    List<EntityData> enemiesToSpawn;

    public List<EntityData> EnemiesToSpawn { get { return enemiesToSpawn; } }
    
    [SerializeField]
    List<ItemData> itemsToSpawn;

    public List<ItemData> ItemsToSpawn { get { return itemsToSpawn; } }

    public CardData scenarioReward;
}
