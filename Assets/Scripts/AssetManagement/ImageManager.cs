﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class ImageManager : SerializedMonoBehaviour {
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

    public static Sprite GetActionSprite(CardCategory cardCategory, Direction direction)
    {
        return instance.abilityImages[cardCategory][direction];
    }
}
