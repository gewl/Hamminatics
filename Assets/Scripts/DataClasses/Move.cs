using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Move {

    public Direction direction;
    public int range;

    public Move(Direction _direction, int _range)
    {
        direction = _direction;
        range = _range;
    }
}
