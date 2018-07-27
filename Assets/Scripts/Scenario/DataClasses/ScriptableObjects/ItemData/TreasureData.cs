using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Treasure")]
public class TreasureData : ItemData {

    public override ItemCategory itemCategory { get { return ItemCategory.Treasure; } }
    public override ItemCollectionType CollectionType { get { return ItemCollectionType.OnFinishMove; } }
    public int value = 1;

    public override ItemData Copy()
    {
        TreasureData copy = ScriptableObject.CreateInstance(typeof(TreasureData)) as TreasureData;

        copy.value = value;
        copy.sprite = sprite;
        copy.ID = ID;
        copy.Duration = Duration;
        copy.Position = Position;

        return copy;
    }
}
