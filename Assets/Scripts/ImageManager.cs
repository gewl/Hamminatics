using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ImageManager : SerializedMonoBehaviour {
    [SerializeField]
    Dictionary<Direction, GameObject> directionPrefabs;

    [SerializeField]
    Dictionary<CardCategory, Dictionary<Direction, Sprite>> abilityImages;

    static ImageManager instance;

    private void Awake()
    {
        instance = this;
    }

    public static GameObject GetDirectionPrefab(Direction direction)
    {
        return instance.directionPrefabs[direction];
    }

    public static Sprite GetActionSprite(CardCategory cardCategory, Direction direction)
    {
        return instance.abilityImages[cardCategory][direction];
    }
}
