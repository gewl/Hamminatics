using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used for enemy turn calculation.
public class EntityTurnTargets {

    public Tile targetMovementTile;
    public Tile targetAttackTile;

    private EntityTurnTargets() { }

    public EntityTurnTargets(Tile _targetMoveTile, Tile _targetAttackTile)
    {
        targetMovementTile = _targetMoveTile;
        targetAttackTile = _targetAttackTile;
    }
}
