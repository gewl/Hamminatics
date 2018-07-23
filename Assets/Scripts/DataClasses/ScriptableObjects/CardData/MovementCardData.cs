using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Card/MovementCard")]
public class MovementCardData : CardData {
    public override CardCategory Category { get { return CardCategory.Movement; } }
}
