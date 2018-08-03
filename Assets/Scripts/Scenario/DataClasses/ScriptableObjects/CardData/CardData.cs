using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardData : ScriptableObject {
    public abstract CardCategory category { get; }
    public string ID = "_NewCard";
    public string description = "Placeholder description.";
    public int cost = 0;
    public AbilityDirection direction = AbilityDirection.AllDirections;
    public int range = 1;
    public Sprite cardImage;
}
