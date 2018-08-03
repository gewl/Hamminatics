using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Card/AttackCard")]
public class AttackCardData : CardData
{
    public override CardCategory category { get { return CardCategory.Attack; } }
    public int Damage = 1;
}
