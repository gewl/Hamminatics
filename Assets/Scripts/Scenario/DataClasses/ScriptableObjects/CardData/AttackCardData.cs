using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Card/AttackCard")]
public class AttackCardData : CardData
{
    public override CardCategory category { get { return CardCategory.Attack; } }
    public int damage = 1;
    public TargetType targetType = TargetType.Single;

    public override void Upgrade()
    {
        damage++;
        range++;
    }

    public override string GetUpgradeText()
    {
        return "Improves damage and range.";
    }

    public override string GetStatText()
    {
        string results = "";
        results += "Energy cost: " + energyCost + "\n";
        results += "Range: " + range + "\n";
        results += "Damage: " + damage + "\n";
        return results;
    }
}
