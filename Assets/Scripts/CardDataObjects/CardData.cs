using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardData : ScriptableObject {
    public string Name = "_NewCard";
    public int Cost = 0;
    public AbilityDirection Direction = AbilityDirection.AllDirections;
    public int Range = 1;
}
