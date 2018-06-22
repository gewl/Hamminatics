using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ImageManager : SerializedMonoBehaviour {
    [SerializeField]
    Dictionary<Direction, GameObject> directionImagePrefabs;

    public GameObject GetDirectionImage(Direction direction)
    {
        return directionImagePrefabs[direction];
    }
}
