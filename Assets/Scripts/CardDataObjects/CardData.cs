﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardData : ScriptableObject {
    public abstract CardCategory Category { get; }
    public string Name = "_NewCard";
    public int Cost = 0;
    public AbilityDirection Direction = AbilityDirection.AllDirections;
    public int Range = 1;
}
