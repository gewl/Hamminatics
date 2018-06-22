using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Data/Card/AttackCard")]
public class AttackCardData : CardData
{
    public override CardCategory Category { get { return CardCategory.Attack; } }
    public int Damage = 1;
}
