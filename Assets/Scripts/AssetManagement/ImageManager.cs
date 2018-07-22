using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class ImageManager : SerializedMonoBehaviour {
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

    static ImageManager instance;

    private void Awake()
    {
        instance = this;
    }

    public static Sprite GetTileTargetedSprite()
    {
        return instance.tileTargetedSprite;
    }

    public static GameObject GetAbilityPointer(CardCategory cardCategory, Direction direction)
    {
        GameObject abilityPointer = GameObject.Instantiate(instance.abilityPointerPrefab);

        abilityPointer.GetComponent<Image>().sprite = GetActionSprite(cardCategory, direction);

        return abilityPointer;
    }

    public static GameObject GetOverlayImage(Sprite overlaySprite)
    {
        GameObject overlayImage = GameObject.Instantiate(instance.pathImagePrefab);
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
