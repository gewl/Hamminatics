using System;   
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ActionStackController : MonoBehaviour {

    public delegate void ActionsDelegate(List<Action> actions);
    public ActionsDelegate OnActionStackUpdate;

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

        OnActionStackUpdate(new List<Action>(actionStack));
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
        int oldPlayerActionIndex = actionList.FindIndex(action => action.entity.ID == Constants.PLAYER_ID);
        if (actionList[oldPlayerActionIndex] == newAction)
        {
            return;
        }
        actionList[oldPlayerActionIndex] = newAction;

        actionStack = ConvertListToStack(actionList);

        if (OnActionStackUpdate != null)
        {
            OnActionStackUpdate(actionList);
        }
    }

    public void ChangePlayerActionPosition(int newIndex)
    {
        List<Action> actionList = actionStack.ToList();

        Action playerAction = actionList.Find(IsPlayerActionPredicate);

        Stack<Action> newActionStack = new Stack<Action>();
        for (int i = actionList.Count - 1; i >= newIndex; i--)
        {
            Action action = actionList[i];
            if (IsPlayerAction(action))
            {
                continue;
            }

            newActionStack.Push(action);
        }

        newActionStack.Push(playerAction);

        for (int i = newIndex-1; i >= 0; i--)
        {
            Action action = actionList[i];
            if (IsPlayerAction(action))
            {
                continue;
            }

            newActionStack.Push(action);
        }


        actionStack = newActionStack;
        OnActionStackUpdate(actionStack.ToList<Action>());
    }

    public Stack<Action> ConvertListToStack(List<Action> actionList)
    {
        Stack<Action> newStack = new Stack<Action>();

        for (int i = actionList.Count - 1; i >= 0; i--)
        {
            newStack.Push(actionList[i]);
        }

        return newStack;
    }

    public Action GetNextAction()
    {
        Action nextAction = actionStack.Pop();

        endTurnButton.interactable = !IsActionStackEmpty;

        if (OnActionStackUpdate != null)
        {
            OnActionStackUpdate(new List<Action>(actionStack));
        }

        return nextAction;
    }

    public bool DoesActionStackContainPlayerAction()
    {
        return actionStack.Any<Action>(IsPlayerAction);
    }

    static Func<Action, bool> IsPlayerAction = (Action action) => action.entity.ID == Constants.PLAYER_ID;
    Predicate<Action> IsPlayerActionPredicate = new Predicate<Action>(IsPlayerAction);

}
