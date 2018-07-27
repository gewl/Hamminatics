using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Modifier")]
public class ModifierData : ScriptableObject {

    public string ID = "_Modifier";
    public Sprite sprite;
    public ModifierCategory modifierCategory;
    public int value = 1;
    public int duration = 1;

    public ModifierData Copy()
    {
        ModifierData copy = ScriptableObject.CreateInstance(typeof(ModifierData)) as ModifierData;

        copy.ID = ID;
        copy.sprite = sprite;
        copy.modifierCategory = modifierCategory;
        copy.value = value;
        copy.duration = duration;

        return copy;

    }
}
