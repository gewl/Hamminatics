using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompletedAction {

    public Tile OriginTile { get; private set; }
    public Tile TargetTile { get; private set; }
    public Direction Direction { get; private set; }
    public CardCategory Category { get; private set; }

    public CompletedAction(Tile _originTile, Tile _targetTile, Direction _direction, CardCategory _category)
    {
        OriginTile = _originTile;
        TargetTile = _targetTile;
        Direction = _direction;
        Category = _category;
    }
}
