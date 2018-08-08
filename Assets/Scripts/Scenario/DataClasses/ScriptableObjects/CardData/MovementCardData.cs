using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Card/MovementCard")]
public class MovementCardData : CardData {
    public override CardCategory category { get { return CardCategory.Movement; } }

    public override void Upgrade()
    {
        range++;
    }

    public override string GetUpgradeText()
    {
        return "Improves range.";
    }

    public override string GetStatText()
    {
        string results = "";
        results += "Energy cost: " + energyCost + "\n";
        results += "Range: " + range + "\n";
        return results;
    }
}
