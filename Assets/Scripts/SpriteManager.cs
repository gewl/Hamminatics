using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class SpriteManager : SerializedMonoBehaviour {

    [SerializeField]
    Dictionary<SpaceContents, Sprite> boardSpaceSprites;

    public Sprite GetSpaceSprite(SpaceContents contents)
    {
        return boardSpaceSprites[contents];
    }
}
