using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class SpriteManager : SerializedMonoBehaviour {

    [SerializeField]
    Dictionary<CellContents, Sprite> boardCellSprites;

    public Sprite GetCellSprite(CellContents contents)
    {
        return boardCellSprites[contents];
    }
}
