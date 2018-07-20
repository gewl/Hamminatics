using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameStateManager : MonoBehaviour {

    TurnStackController _turnStackController;
    TurnStackController turnStackController
    {
        get
        {
            if (_turnStackController == null)
            {
                _turnStackController = GetComponent<TurnStackController>();
            }

            return _turnStackController;
        }
    }
    BoardController _boardController;
    BoardController boardController
    {
        get
        {
            if (_boardController == null)
            {
                _boardController = GetComponentInChildren<BoardController>();
            }

            return _boardController;
        }
    }
    EquippedCardsManager _equippedCardsManager;
    EquippedCardsManager equippedCardsManager
    {
        get
        {
            if (_equippedCardsManager == null)
            {
                _equippedCardsManager = GetComponentInChildren<EquippedCardsManager>();
            }

            return _equippedCardsManager;
        }
    }
    EnemyTurnCalculator _enemyTurnCalculator;
    EnemyTurnCalculator enemyTurnCalculator
    {
        get
        {
            if (_enemyTurnCalculator == null)
            {
                _enemyTurnCalculator = GetComponent<EnemyTurnCalculator>();
            }

            return _enemyTurnCalculator;
        }
    }
    TurnDrawer _turnDrawer;
    TurnDrawer turnDrawer
    {
        get
        {
            if (_turnDrawer == null)
            {
                _turnDrawer = GetComponentInChildren<TurnDrawer>();
            }

            return _turnDrawer;
        }
    }

    public GameState CurrentGameState { get; private set; }
    public GameState ProjectedGameState { get; private set; }
    public EntityData Player { get { return CurrentGameState.player; } }

    List<Vector2Int> potentialCardTargets;
    EntityData selectedEntity;

    bool isResolvingTurn = false;
    public bool IsResolvingTurn { get { return isResolvingTurn; } }

    private void Awake()
    {
        potentialCardTargets = new List<Vector2Int>();
        // This has to be delayed so layout group can space accordingly.
        Invoke("SetBoardUp", 0.1f);
    }

    public void InitializeGameState(GameBoard board)
    {
        CurrentGameState = GameStateGenerator.GenerateNewGameState(board.Entrance.Position, board.BoardWidth);
        GameStateDelegates.OnCurrentGameStateChange += ResetBoard;
        GameStateDelegates.ReturnToDefaultBoard += ResetBoard;
    }

    private void OnEnable()
    {
        turnStackController.OnTurnStackUpdate += ResetBoard; 
    }

    private void OnDisable()
    {
        GameStateDelegates.ReturnToDefaultBoard -= ResetBoard;
        GameStateDelegates.OnCurrentGameStateChange -= ResetBoard;
        turnStackController.OnTurnStackUpdate -= ResetBoard; 
    }

    void SetBoardUp()
    {
        GenerateNextTurnStack(CurrentGameState);
        GameStateDelegates.OnCurrentGameStateChange(CurrentGameState);
    }

    public void HighlightPotentialCardTargets(CardData card)
    {
        ResetBoard(CurrentGameState);

        int cardRange = card.Range;

        // Movement availability is always 'from' player's current position.
        // Other actions are 'from' player's projected position.
        Vector2Int playerOrigin = card.Category == CardCategory.Movement ? Player.Position : CurrentGameState.GetProjectedPlayerPosition();

        BoardHelperFunctions.GetPotentialBranchingTargets(playerOrigin, cardRange).ForEach(t => HighlightCell(t.Position));
    }

    void HighlightCell(Vector2Int position)
    {
        boardController.HighlightSelectedCell(position);
        potentialCardTargets.Add(position);
    }

    public void ResetBoard(List<Turn> turnList)
    {
        ResetBoard(CurrentGameState);
    }

    public void ResetBoard()
    {
        ResetBoard(CurrentGameState);
    }

    void ResetBoard(GameState currentGameState)
    {
        if (!isResolvingTurn)
        {
            currentGameState.entityPathsMap = currentGameState.GenerateAllEntityPaths();
            turnDrawer.DrawAllPaths(currentGameState);
        }
        boardController.DrawBoard_Standard(currentGameState, isResolvingTurn);
        potentialCardTargets.Clear();
    }

    public void RegisterCellClick(Vector2Int tileClickedPosition)
    {
        if (isResolvingTurn)
        {
            return;      
        }
        if (potentialCardTargets.Contains(tileClickedPosition))
        {
            CardData selectedCard = equippedCardsManager.GetSelectedCard();
            Vector2Int playerOrigin = selectedCard.Category == CardCategory.Movement ? Player.Position : CurrentGameState.GetProjectedPlayerPosition();
            turnStackController.AddToPlayerTurn(selectedCard, Player, playerOrigin, tileClickedPosition);
            equippedCardsManager.ClearSelectedCard();
            GameStateDelegates.OnCurrentGameStateChange(CurrentGameState);
        }
        else if (selectedEntity != null)
        {
            DeselectEntity();
        }
        else if (CurrentGameState.IsTileOccupied(tileClickedPosition))
        {
            EntityData tileOccupant = CurrentGameState.GetTileOccupant(tileClickedPosition);
            SelectEntity(tileOccupant);
        }
    }

    void SelectEntity(EntityData entity)
    {
        selectedEntity = entity;
        GameStateDelegates.OnEntitySelected(entity, CurrentGameState, ProjectedGameState);
    }

    void DeselectEntity()
    {
        selectedEntity = null;
        GameStateDelegates.ReturnToDefaultBoard();
    }

    public void EndRound()
    {
        StartCoroutine(ProcessCurrentRoundActions());
    }

    IEnumerator ProcessCurrentRoundActions()
    {
        isResolvingTurn = true;
        CurrentGameState.actionsCompletedLastRound.Clear();
        CurrentGameState.movesCompletedLastRound.Clear();

        while (!turnStackController.IsTurnStackEmpty)
        {
            Turn nextTurn = turnStackController.GetNextTurn();
            EntityData entity = nextTurn.Entity; 
            GameStateDelegates.OnResolvingTurn(nextTurn);

            PathEnumerator pathEnumerator = CurrentGameState.entityPathsMap[entity].GetEnumerator();

            while (pathEnumerator.Current != null)
            {
                entity.Position = pathEnumerator.Current.newPosition;

                if (pathEnumerator.Current.bumpedBy != null || pathEnumerator.Current.bumpedEntity != null)
                {
                    entity.Health--;
                }

                GameStateDelegates.OnCurrentGameStateChange(CurrentGameState);
                pathEnumerator.MoveNext();
                yield return new WaitForSeconds(0.5f);
            }

            turnDrawer.DrawSingleAction(nextTurn.action);
            GameStateHelperFunctions.ProcessAction(nextTurn.action, CurrentGameState);
            GameStateDelegates.OnCurrentGameStateChange(CurrentGameState);

            yield return new WaitForSeconds(0.5f);
        }

        //while (!turnStackController.IsTurnStackEmpty)
        //{
        //    turnDrawer.Clear();
        //    Turn nextTurn = turnStackController.GetNextTurn();

        //    GameStateDelegates.OnResolvingTurn(nextTurn);

        //    Tile entityInitialTile = BoardController.CurrentBoard.GetTileAtPosition(nextTurn.Entity.Position);

        //    for (int i = 0; i < nextTurn.moves.Count; i++)
        //    {
        //        GameStateHelperFunctions.ProcessMove(nextTurn.moves[i], nextTurn.Entity, CurrentGameState);
        //        GameStateDelegates.OnCurrentGameStateChange(CurrentGameState);

        //        if (i < nextTurn.moves.Count - 1)
        //        {
        //            turnDrawer.DrawSingleMove(nextTurn.Entity.Position, nextTurn.moves[i+1]);
        //        }
        //        else
        //        {
        //            turnDrawer.Clear();
        //        }
        //        yield return new WaitForSeconds(0.5f);
        //    }

        //    Tile entityResultingTile = BoardController.CurrentBoard.GetTileAtPosition(nextTurn.Entity.Position);
        //    CompletedMove completedMove = new CompletedMove(nextTurn.moves, nextTurn.Entity, entityInitialTile, entityResultingTile);

        //    CurrentGameState.movesCompletedLastRound.Add(completedMove);

        //    turnDrawer.DrawSingleAction(nextTurn.action);
        //    GameStateHelperFunctions.ProcessAction(nextTurn.action, CurrentGameState);

        //    GameStateDelegates.OnCurrentGameStateChange(CurrentGameState);
        //    yield return new WaitForSeconds(0.5f);
        //}

        GenerateNextTurnStack(CurrentGameState);

        if (GameStateDelegates.OnRoundEnded != null)
        {
            GameStateDelegates.OnRoundEnded(CurrentGameState);
        }

        isResolvingTurn = false;

        if (GameStateDelegates.OnCurrentGameStateChange != null)
        {
            GameStateDelegates.OnCurrentGameStateChange(CurrentGameState);
        }
    }

    void GenerateNextTurnStack(GameState gameState)
    {
        enemyTurnCalculator.CalculateAndQueueEnemyTurns(CurrentGameState);
        turnStackController.AddEmptyPlayerTurn();
    }
}
