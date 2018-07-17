using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Path {

    public Vector2Int position;
    public PathDirection path;
    public Direction entranceDirection;
    
    public Path(Vector2Int _position, PathDirection _path, Direction _entranceDirection)
    {
        position = _position;
        path = _path;
        entranceDirection = _entranceDirection;
    }
}
