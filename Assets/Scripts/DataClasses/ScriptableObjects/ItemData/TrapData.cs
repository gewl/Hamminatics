using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Trap")]
public class TrapData : ItemData {
    public override ItemCategory itemCategory { get { return ItemCategory.Trap; } }
    public override ItemCollectionType CollectionType { get { return ItemCollectionType.OnStep; } }

    public TrapCategory trapCategory;
    public ModifierData modifierToApply;

    public int value = 1;

    public override ItemData Copy()
    {
        TrapData copy = ScriptableObject.CreateInstance(typeof(TrapData)) as TrapData;

        copy.trapCategory = trapCategory;
        copy.value = value;
        copy.sprite = sprite;
        copy.ID = ID;
        copy.Duration = Duration;
        copy.Position = Position;

        return copy;

    }
}
