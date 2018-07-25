using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemData : ScriptableObject {
    public string ID = "_newItem";
    public string description = "Placeholder description.";

    public abstract ItemCategory Category { get; }
    public abstract ItemCollectionType CollectionType { get; }
    public int Duration = -1;
    public Sprite sprite;
    [HideInInspector]
    public Vector2Int Position;

    public abstract ItemData Copy();
}
