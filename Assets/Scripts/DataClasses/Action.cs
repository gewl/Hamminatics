using UnityEngine;
using System;   

public struct Action {

    public CardData card;
    public EntityData entity;
    public Direction direction;
    public int distance;

    public Action(CardData _card, EntityData _entity, Direction _direction, int _distance)
    {
        card = _card;
        entity = _entity;
        direction = _direction;
        distance = _distance;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        Action action = (Action)obj;

        return card.ID == action.card.ID && entity.ID == action.entity.ID && direction == action.direction && distance == action.distance;
    }

    public override int GetHashCode()
    {
        int result = 0;
        result = (result * 397) ^ card.ID.GetHashCode();
        result = (result * 397) ^ entity.ID.GetHashCode();
        result = (result * 397) ^ direction.GetHashCode();
        result = (result * 397) ^ distance;
        return result;
    }

    public static bool operator == (Action a, Action b)
    {
        return a.Equals(b);
    }

    public static bool operator != (Action a, Action b)
    {
        return !a.Equals(b);
    }

}
