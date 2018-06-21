using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionQueueController : MonoBehaviour {
    List<Action> actionQueue;

    private void Awake()
    {
        actionQueue = new List<Action>();
    }

    public void AddNewAction(CardData card, EntityData entity, Direction direction, int distance)
    {
        Action newAction = new Action(card, entity, direction, distance);
    }
}
