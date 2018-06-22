﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ActionQueueController : MonoBehaviour {

    const string PLAYER_ID = "Player";

    Stack<Action> actionStack;
    public bool IsActionStackEmpty { get { return actionStack.Count == 0; } }

    [SerializeField]
    Button endTurnButton;

    private void Awake()
    {
        actionStack = new Stack<Action>();
    }

    public void AddNewAction(CardData card, EntityData entity, Direction direction, int distance)
    {
        Action newAction = new Action(card, entity, direction, distance);

        AddNewAction(newAction);
    }

    public void AddNewAction(Action newAction)
    {
        actionStack.Push(newAction);
        endTurnButton.interactable = DoesActionStackContainPlayerAction();
    }

    public void AddPlayerAction(CardData card, EntityData entity, Direction direction, int distance)
    {
        Action newAction = new Action(card, entity, direction, distance);
        if (DoesActionStackContainPlayerAction())
        {
            ReplacePlayerAction(newAction);
        }
        else
        {
            AddNewAction(newAction);
        }
    }

    public void ReplacePlayerAction(Action newAction)
    {
        List<Action> actionList = actionStack.ToList<Action>();
        int oldPlayerActionIndex = actionList.FindIndex(action => action.entity.ID == PLAYER_ID);
        if (actionList[oldPlayerActionIndex] == newAction)
        {
            return;
        }
        actionList[oldPlayerActionIndex] = newAction;
        actionStack = new Stack<Action>(actionList);
    }

    public Action GetNextAction()
    {
        Action nextAction = actionStack.Pop();

        endTurnButton.interactable = !IsActionStackEmpty;

        return nextAction;
    }

    public bool DoesActionStackContainPlayerAction()
    {
        return actionStack.Any<Action>(action => action.entity.ID == PLAYER_ID);
    }

}
