using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Turn {

    public EntityData Entity
    {
        get; private set;
    }

    public void UpdateEntity(EntityData newEntity)
    {
        Entity = newEntity;
        FirstAction.entity = newEntity;
        SecondAction.entity = newEntity;
    }

    public Action FirstAction;
    public Action SecondAction;

    //public Turn(EntityData _entity)
    //{
    //    if (_entity.ID != "Player")
    //    {
    //        Debug.LogError("Creating empty turn for non-player entity.");
    //    }

    //    Entity = _entity;
    //}

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
