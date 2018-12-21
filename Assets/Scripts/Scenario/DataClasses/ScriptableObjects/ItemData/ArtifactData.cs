using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Artifact")]
public class ArtifactData : ItemData {

    public override ItemCategory itemCategory { get { return ItemCategory.Artifact; } }
    public override ItemCollectionType CollectionType { get { return ItemCollectionType.OnFinishMove; } }
    public int value = 1;

    public override ItemData Copy()
    {
        ArtifactData copy = ScriptableObject.CreateInstance(typeof(ArtifactData)) as ArtifactData;

        copy.value = value;
        copy.sprite = sprite;
        copy.ID = ID;
        copy.Duration = Duration;
        copy.Position = Position;

        return copy;
    }
}
