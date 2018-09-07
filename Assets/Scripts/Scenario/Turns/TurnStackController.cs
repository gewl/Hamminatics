using System;   
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TurnStackController : MonoBehaviour {

    [SerializeField]
    ScenarioStateManager scenarioStateManager;

    public delegate void TurnsDelegate(List<Turn> actions);
    public TurnsDelegate OnTurnStackUpdate;

    public Stack<Turn> TurnStack {
        get
        {
            return scenarioStateManager.CurrentScenarioState.turnStack; 
        }
        set
        {
            scenarioStateManager.CurrentScenarioState.UpdateTurnStack(value);
        }
    }
    public bool IsTurnStackEmpty { get { return TurnStack.Count == 0; } }

    [SerializeField]
    Button endRoundButton;
    [SerializeField]
    EnergyManager energyManager;

    private void OnEnable()
    {
        GameStateDelegates.OnResolvingState += ResolvingStateHandler; 
    }

    private void OnDisable()
    {
        GameStateDelegates.OnResolvingState -= ResolvingStateHandler; 
    }

    void ResolvingStateHandler(ProjectedGameState projectedState)
    {
        endRoundButton.interactable = false;
    }

    public void AddNewTurn(Turn newTurn)
    {
        TurnStack.Push(newTurn);
        endRoundButton.interactable = IsPlayerTurnComplete();
    }

    public void AddEmptyPlayerTurn()
    {
        Turn newTurn = new Turn(scenarioStateManager.Player);
        AddNewTurn(newTurn);
    }

    Turn GetPlayerTurn()
    {
        return TurnStack.First(t => t.Entity.ID == scenarioStateManager.Player.ID);
    }

    Turn GetEntityTurn(EntityData entity)
    {
        return TurnStack.First(t => t.Entity == entity);
    }

    public void AddToPlayerTurn(CardData card, EntityData player, Vector2Int originPosition, Vector2Int targetPosition)
    {
        if (card.category == CardCategory.Movement)
        {
            UpdatePlayerTurn_Movement(originPosition, targetPosition);
        }
        else
        {
            UpdatePlayerTurn_Action(card, player, originPosition, targetPosition);
        }
        endRoundButton.interactable = IsPlayerTurnComplete();
        OnTurnStackUpdate(new List<Turn>(TurnStack));
    }
    
    public void AddToPlayerTurn(CardData card, EntityData player, Tile originTile, List<Tile> tilePath)
    {
        List<Direction> directionPath = new List<Direction>();

        for (int i = 0; i < tilePath.Count; i++)
        {
            Tile thisTile = tilePath[i];
            Tile lastTile = i == 0 ? originTile : tilePath[i - 1];
            directionPath.Add(lastTile.GetDirectionOfNeighbor(thisTile));
        }

        GetPlayerTurn().moves = directionPath;
        endRoundButton.interactable = IsPlayerTurnComplete();
        OnTurnStackUpdate(new List<Turn> (TurnStack));
    }

    void UpdatePlayerTurn_Movement(Vector2Int originPosition, Vector2Int targetPosition)
    {
        List<Direction> pathToPosition = BoardHelperFunctions.FindPathBetweenTiles(BoardController.CurrentBoard.GetTileAtPosition(originPosition), BoardController.CurrentBoard.GetTileAtPosition(targetPosition));

        GetPlayerTurn().moves = pathToPosition;
    }

    void UpdatePlayerTurn_Action(CardData card, EntityData player, Vector2Int originPosition, Vector2Int targetPosition)
    {
        Tile playerTile = BoardController.CurrentBoard.GetTileAtPosition(originPosition);
        Tile targetTile = BoardController.CurrentBoard.GetTileAtPosition(targetPosition);
        GetPlayerTurn().action = new Action(card, player, BoardHelperFunctions.GetDirectionBetweenTiles(playerTile, targetTile), BoardHelperFunctions.GetLinearDistanceBetweenTiles(playerTile, targetTile));
        energyManager.UpdateProjectedEnergyCost(card.energyCost);
    }

    public void ChangePlayerTurnPosition(int newIndex)
    {
        List<Turn> turnList = TurnStack.ToList();

        Turn playerTurn = GetPlayerTurn();

        Stack<Turn> newTurnStack = new Stack<Turn>();

        for (int i = turnList.Count - 1; i >= newIndex; i--) 
        {
            Turn turn = turnList[i];
            if (turn.IsPlayerTurn())
            {
                continue;
            }

            newTurnStack.Push(turn);
        }

        newTurnStack.Push(playerTurn);

        for (int i = newIndex - 1; i >= 0; i--)
        {
            Turn turn = turnList[i];
            if (turn.IsPlayerTurn())
            {
                continue;
            }

            newTurnStack.Push(turn);
        }

        TurnStack = newTurnStack;
        energyManager.UpdateProjectedEnergyGain(newIndex - 1);
        OnTurnStackUpdate(TurnStack.ToList<Turn>());
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

        return nextTurn;
    }

    public bool IsPlayerTurnComplete()
    {
        return TurnStack.Any(turn => turn.IsCompletePlayerTurn());
    }

}
