using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Treasure")]
public class TreasureData : ItemData {

    public override ItemCategory Category { get { return ItemCategory.Treasure; } }
    public override ItemCollectionType CollectionType { get { return ItemCollectionType.OnFinishMove; } }
    public int Value = 1;

    public override ItemData Copy()
    {
        TreasureData copy = base.Copy() as TreasureData;

        copy.Value = Value;

        return copy;
    }

}
