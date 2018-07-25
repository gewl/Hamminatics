using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardData : ScriptableObject {
    public abstract CardCategory Category { get; }
    public string ID = "_NewCard";
    public string description = "Placeholder description.";
    public int Cost = 0;
    public AbilityDirection Direction = AbilityDirection.AllDirections;
    public int Range = 1;
}
