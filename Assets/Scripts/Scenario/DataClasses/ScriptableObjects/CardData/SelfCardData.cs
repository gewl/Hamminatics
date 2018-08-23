using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Card/SelfCard")]
public class SelfCardData : CardData {

    public override CardCategory category { get { return CardCategory.Self; } }
    public int healthChange = 0;

    public override void Upgrade()
    {
        // TODO: Implement self card upgrades (upgrading modifiers?)
        Debug.Log("Upgrade not implemented for self cards.");
    }

    public override string GetUpgradeText()
    {
        return "Does nothing.";
    }

    public override string GetStatText()
    {
        string results = "";
        results += "Energy cost:" + energyCost + "\n";
        return results;
    }
}
