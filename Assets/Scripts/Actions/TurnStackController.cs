using System;   
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TurnStackController : MonoBehaviour {

    [SerializeField]
    GameStateManager gameStateManager;

    public delegate void TurnsDelegate(List<Turn> actions);
    public TurnsDelegate OnTurnStackUpdate;

    public Stack<Turn> TurnStack {
        get
        {
            return gameStateManager.CurrentGameState.turnStack; 
        }
        set
        {
            gameStateManager.CurrentGameState.UpdateTurnStack(value);
        }
    }
    public bool IsTurnStackEmpty { get { return TurnStack.Count == 0; } }

    [SerializeField]
    Button endRoundButton;
    [SerializeField]
    EnergyManager energyManager;

    public void AddNewTurn(Turn newTurn)
    {
        TurnStack.Push(newTurn);
        endRoundButton.interactable = DoesTurnStackContainCompletePlayerTurn();

        OnTurnStackUpdate(new List<Turn>(TurnStack));
    }

    public void AddPlayerTurn()
    {
        //Turn newTurn = new Action(card, entity, direction, distance);
        //if (DoesTurnStackContainCompletePlayerTurn())
        //{
        //    ReplacePlayerAction(newTurn);
        //}
        //else
        //{
        //    AddNewTurn(newTurn);
        //}
    }

    public void ReplacePlayerTurn()
    {
        //List<Action> actionList = TurnStack.ToList<Action>();
        //int oldPlayerActionIndex = actionList.FindIndex(action => action.entity.ID == Constants.PLAYER_ID);
        //if (actionList[oldPlayerActionIndex] == newAction)
        //{
        //    return;
        //}
        //actionList[oldPlayerActionIndex] = newAction;

        //TurnStack = ConvertListToStack(actionList);

        //if (OnTurnStackUpdate != null)
        //{
        //    OnTurnStackUpdate(actionList);
        //}
    }

    public void ChangePlayerTurnPosition(int newIndex)
    {
        //List<Action> actionList = TurnStack.ToList();

        //Action playerAction = actionList.Find(IsPlayerActionPredicate);

        //Stack<Action> newActionStack = new Stack<Action>();
        //for (int i = actionList.Count - 1; i >= newIndex; i--)
        //{
        //    Action action = actionList[i];
        //    if (IsPlayerAction(action))
        //    {
        //        continue;
        //    }

        //    newActionStack.Push(action);
        //}

        //newActionStack.Push(playerAction);

        //for (int i = newIndex-1; i >= 0; i--)
        //{
        //    Action action = actionList[i];
        //    if (IsPlayerAction(action))
        //    {
        //        continue;
        //    }

        //    newActionStack.Push(action);
        //}


        //TurnStack = newActionStack;
        //energyManager.ProjectedEnergyGain = newIndex - 1;
        //OnTurnStackUpdate(TurnStack.ToList<Action>());
    }

    public Stack<Turn> ConvertListToStack(List<Turn> turnList)
    {
        Stack<Turn> newStack = new Stack<Turn>();

        for (int i = turnList.Count - 1; i >= 0; i--)
        {
            newStack.Push(turnList[i]);
        }

        return newStack;
    }

    public Turn GetNextTurn()
    {
        Turn nextTurn = TurnStack.Pop();

        endRoundButton.interactable = !IsTurnStackEmpty;

        if (OnTurnStackUpdate != null)
        {
            OnTurnStackUpdate(new List<Turn>(TurnStack));
        }

        return nextTurn;
    }

    public bool DoesTurnStackContainCompletePlayerTurn()
    {
        return TurnStack.Any<Turn>(IsPlayerTurn);
    }

    static Func<Turn, bool> IsPlayerTurn = (Turn turn) => turn.Entity.ID == Constants.PLAYER_ID;
    Predicate<Turn> IsPlayerActionPredicate = new Predicate<Turn>(IsPlayerTurn);

}
