using System;   
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ActionStackController : MonoBehaviour {

    [SerializeField]
    GameStateManager gameStateManager;

    public delegate void ActionsDelegate(List<Action> actions);
    public ActionsDelegate OnActionStackUpdate;

    public Stack<Action> ActionStack {
        get
        {
            return gameStateManager.CurrentGameState.actionStack; 
        }
        set
        {
            gameStateManager.CurrentGameState.UpdateActionStack(value);
        }
    }
    public bool IsActionStackEmpty { get { return ActionStack.Count == 0; } }

    [SerializeField]
    Button endTurnButton;
    [SerializeField]
    EnergyManager energyManager;

    public void AddNewAction(CardData card, EntityData entity, Direction direction, int distance)
    {
        Action newAction = new Action(card, entity, direction, distance);

        AddNewAction(newAction);
    }

    public void AddNewAction(Action newAction)
    {
        ActionStack.Push(newAction);
        endTurnButton.interactable = DoesActionStackContainPlayerAction();

        OnActionStackUpdate(new List<Action>(ActionStack));
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
        List<Action> actionList = ActionStack.ToList<Action>();
        int oldPlayerActionIndex = actionList.FindIndex(action => action.entity.ID == Constants.PLAYER_ID);
        if (actionList[oldPlayerActionIndex] == newAction)
        {
            return;
        }
        actionList[oldPlayerActionIndex] = newAction;

        ActionStack = ConvertListToStack(actionList);

        if (OnActionStackUpdate != null)
        {
            OnActionStackUpdate(actionList);
        }
    }

    public void ChangePlayerActionPosition(int newIndex)
    {
        List<Action> actionList = ActionStack.ToList();

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


        ActionStack = newActionStack;
        energyManager.ProjectedEnergyGain = newIndex - 1;
        OnActionStackUpdate(ActionStack.ToList<Action>());
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
        Action nextAction = ActionStack.Pop();

        endTurnButton.interactable = !IsActionStackEmpty;

        if (OnActionStackUpdate != null)
        {
            OnActionStackUpdate(new List<Action>(ActionStack));
        }

        return nextAction;
    }

    public bool DoesActionStackContainPlayerAction()
    {
        return ActionStack.Any<Action>(IsPlayerAction);
    }

    static Func<Action, bool> IsPlayerAction = (Action action) => action.entity.ID == Constants.PLAYER_ID;
    Predicate<Action> IsPlayerActionPredicate = new Predicate<Action>(IsPlayerAction);

}
