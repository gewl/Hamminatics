using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class ImageManager : SerializedMonoBehaviour {
    [SerializeField]
    Sprite emptyActionSprite;

    [SerializeField]
    GameObject abilityPointerPrefab;

    [SerializeField]
    Dictionary<CardCategory, Dictionary<Direction, Sprite>> abilityImages;

    static ImageManager instance;

    private void Awake()
    {
        instance = this;
    }

    public static GameObject GetAbilityPointer(CardCategory cardCategory, Direction direction)
    {
        GameObject abilityPointer = GameObject.Instantiate(instance.abilityPointerPrefab);

        abilityPointer.GetComponent<Image>().sprite = GetActionSprite(cardCategory, direction);

        return abilityPointer;
    }

    public static Sprite GetEmptyActionSprite()
    {
        return instance.emptyActionSprite;
    }

    public static Sprite GetActionSprite(CardCategory cardCategory, Direction direction)
    {
        return instance.abilityImages[cardCategory][direction];
    }

    public static Sprite GetMovementSprite(Direction direction)
    {
        return instance.abilityImages[CardCategory.Movement][direction];
    }
}
