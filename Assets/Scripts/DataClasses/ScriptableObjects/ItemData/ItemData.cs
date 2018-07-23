using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemData : ScriptableObject {
    public string ID = "_newItem";
    public abstract ItemCategory Category { get; }
    public abstract ItemCollectionType CollectionType { get; }
    public int Duration = -1;
    public Sprite sprite;
    [HideInInspector]
    public Vector2Int Position;

    public virtual ItemData Copy()
    {
        ItemData copy = ScriptableObject.CreateInstance(typeof(ItemData)) as ItemData;

        copy.ID = ID;
        copy.Duration = Duration;
        copy.sprite = sprite;
        copy.Position = Position;

        return copy;

    }
}
