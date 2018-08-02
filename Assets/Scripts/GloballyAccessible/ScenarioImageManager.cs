using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class ScenarioImageManager : SerializedMonoBehaviour {
    [SerializeField]
    Sprite emptyActionSprite;

    [SerializeField]
    Sprite tileTargetedSprite;

    [SerializeField]
    GameObject pathImagePrefab;

    [SerializeField]
    GameObject abilityPointerPrefab;

    [SerializeField]
    Dictionary<CardCategory, Dictionary<Direction, Sprite>> abilityImages;

    [SerializeField]
    Dictionary<PathType, Sprite> pathSprites;

    static ScenarioImageManager instance;

    private void Awake()
    {
        instance = this;
    }

    public static Sprite GetTileTargetedSprite()
    {
        return instance.tileTargetedSprite;
    }

    public static GameObject GetAbilityPointer(Sprite cardSprite, float rotation)
    {
        GameObject abilityPointer = GameObject.Instantiate(instance.abilityPointerPrefab);

        abilityPointer.GetComponent<Image>().sprite = cardSprite;
        abilityPointer.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(0f, 0f, rotation));

        return abilityPointer;
    }

    public static GameObject GetOverlayImage(Sprite overlaySprite, Transform parent)
    {
        GameObject overlayImage = GameObject.Instantiate(instance.pathImagePrefab, parent);
        overlayImage.GetComponent<Image>().sprite = overlaySprite;

        return overlayImage;
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

    public static Sprite GetPathSprite(PathType path)
    {
        return instance.pathSprites[path];
    }

}
