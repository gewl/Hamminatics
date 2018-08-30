using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName ="DepthData")]
public class DepthData : SerializedScriptableObject {

    public int ID = -1;

    public List<CardData> randomCardPool;
    public TextAsset eventPool;
    public List<EnemySpawnGroupData> randomEnemySpawnPool;
    public int goldValueMultiplier = 1;
}
