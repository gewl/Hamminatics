using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Turn {

    public EntityData Entity;
    public Action FirstAction { get; private set; }
    public Action SecondAction { get; private set; }

    public Turn(EntityData _entity, Action _firstAction, Action _secondAction)
    {
        Entity = _entity;
        FirstAction = _firstAction;
        SecondAction = _secondAction;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        Turn turn = (Turn)obj;

        return FirstAction.GetHashCode() == turn.FirstAction.GetHashCode() && SecondAction.GetHashCode() == turn.SecondAction.GetHashCode();
    }

    public override int GetHashCode()
    {
        int result = 0;
        result = (result * 297) ^ FirstAction.GetHashCode();
        result = (result * 297) ^ SecondAction.GetHashCode();
        return result;
    }

    public static bool operator == (Turn a, Turn b)
    {
        return a.Equals(b);
    }

    public static bool operator != (Turn a, Turn b)
    {
        return !a.Equals(b);
    }

}
