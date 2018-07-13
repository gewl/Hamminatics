using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turn {

    public EntityData Entity
    {
        get; private set;
    }

    public List<Direction> moves;
    public Action action;

    public void UpdateEntity(EntityData newEntity)
    {
        Entity = newEntity;
        action.entity = newEntity;
    }

    public Turn(EntityData _entity)
    {
        Entity = _entity;
        moves = new List<Direction>();
    }   

    public Turn(EntityData _entity, List<Direction> _moves, Action _action)
    {
        Entity = _entity;
        moves = _moves;
        action = _action;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        Turn turn = (Turn)obj;

        return turn.GetHashCode() == GetHashCode();
    }

    public override int GetHashCode()
    {
        int result = 0;
        result = (result * 297) ^ Entity.ID.GetHashCode();
        result = (result * 297) ^ action.GetHashCode();
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
