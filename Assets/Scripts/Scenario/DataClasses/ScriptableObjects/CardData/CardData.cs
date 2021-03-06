﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class CardData : SerializedScriptableObject {
    public abstract CardCategory category { get; }
    public string ID = "_NewCard";
    public string description = "Placeholder description.";
    public int energyCost = 0;
    public int baseGoldCost = 0;
    public AbilityDirection direction = AbilityDirection.AllDirections;
    public int range = 1;
    public Sprite cardImage;
    public List<ModifierData> modifiers;

    public abstract void Upgrade();
    public abstract string GetUpgradeText();
    public abstract string GetStatText();
}
