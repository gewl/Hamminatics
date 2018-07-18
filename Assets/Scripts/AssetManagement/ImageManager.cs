using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class ImageManager : SerializedMonoBehaviour {
    [SerializeField]
    Sprite emptyActionSprite;

    [SerializeField]
    GameObject pathImagePrefab;

    [SerializeField]
    GameObject abilityPointerPrefab;

    [SerializeField]
    Dictionary<CardCategory, Dictionary<Direction, Sprite>> abilityImages;

    [SerializeField]
    Dictionary<PathDirection, Sprite> pathSprites;

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

    public static GameObject GetPathImage(Sprite pathSprite, Direction entranceDirection)
    {
        GameObject pathImage = GameObject.Instantiate(instance.pathImagePrefab);
        RectTransform pathRect = pathImage.GetComponent<RectTransform>();

        pathImage.GetComponent<Image>().sprite = pathSprite;

        float imageRotation = 0f;

        switch (entranceDirection)
        {
            case Direction.Up:
                break;
            case Direction.Right:
                imageRotation = -90f;
                break;
            case Direction.Down:
                imageRotation = 180f;
                break;
            case Direction.Left:
                imageRotation = 90f;
                break;
            default:
                break;
        }


        pathRect.rotation = Quaternion.Euler(new Vector3(0f, 0f, imageRotation));

        return pathImage;
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

    public static Sprite GetPathSprite(PathDirection path)
    {
        return instance.pathSprites[path];
    }

}
