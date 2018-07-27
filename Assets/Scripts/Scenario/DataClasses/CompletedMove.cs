using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompletedMove {

    public List<Direction> Moves { get; private set; }
    public EntityData Entity { get; private set; }
    public Tile OriginTile { get; private set; }
    public Tile DestinationTile { get; private set; }

    public CompletedMove(List<Direction> _moves, EntityData _entity, Tile _originTile, Tile _destinationTile)
    {
        Moves = _moves;
        Entity = _entity;
        OriginTile = _originTile;
        DestinationTile = _destinationTile;
    }
}
