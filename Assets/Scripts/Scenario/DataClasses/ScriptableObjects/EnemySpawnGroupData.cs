﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy Spawn Group")]
public class EnemySpawnGroupData : ScriptableObject {

    public string ID;
    [SerializeField]
    List<EntityData> enemiesToSpawn;

    public List<EntityData> EnemiesToSpawn { get { return enemiesToSpawn; } }
}
