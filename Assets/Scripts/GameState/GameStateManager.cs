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
    public GameState ProjectedGameState { get { return upcomingGamestates.Last().gameState; } }
    public EntityData Player { get { return CurrentGameState.player; } }

    List<Vector2Int> potentialCardTargets;
    EntityData selectedEntity;
    List<ProjectedGameState> upcomingGamestates;
    public Vector2Int ProjectedPlayerPosition
    {
        get
        {
            return ProjectedGameState.player.Position;
        }
    }

    bool isResolvingTurn = false;
    public bool IsResolvingTurn { get { return isResolvingTurn; } }

    private void Awake()
    {
        potentialCardTargets = new List<Vector2Int>();
        // This has to be delayed so layout group can space accordingly.
        Invoke("SetBoardUp", 0.1f);

        upcomingGamestates = new List<ProjectedGameState>();
    }

    public void InitializeGameState(GameBoard board)
    {
        CurrentGameState = GameStateGenerator.GenerateNewGameState(board.Entrance.Position, board.BoardWidth);
        GameStateDelegates.OnCurrentGameStateChange += ResetBoard;
        GameStateDelegates.ReturnToDefaultBoard += ResetBoard;
    }

    private void OnEnable()
    {
        turnStackController.OnTurnStackUpdate += RecalculateUpcomingStates;
    }

    private void OnDisable()
    {
        GameStateDelegates.ReturnToDefaultBoard -= ResetBoard;
        GameStateDelegates.OnCurrentGameStateChange -= ResetBoard;
        turnStackController.OnTurnStackUpdate -= RecalculateUpcomingStates;
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
        Vector2Int playerOrigin = card.Category == CardCategory.Movement ? Player.Position : ProjectedPlayerPosition;

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

    void RecalculateUpcomingStates(List<Turn> turns)
    {
        if (!isResolvingTurn)
        {
            upcomingGamestates = UpcomingStateCalculator.CalculateUpcomingStates(CurrentGameState);
            turnDrawer.DrawUpcomingStates(upcomingGamestates);
        }
    }

    void ResetBoard(GameState currentGameState)
    {
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
            Vector2Int playerOrigin = selectedCard.Category == CardCategory.Movement ? Player.Position : ProjectedPlayerPosition;
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

        for (int i = 0; i < upcomingGamestates.Count; i++)
        {
            GameState nextGameState = upcomingGamestates[i].gameState;

            CurrentGameState = nextGameState;
           
            GameStateDelegates.OnCurrentGameStateChange(CurrentGameState);

            yield return new WaitForSeconds(0.5f);
        }

        if (GameStateDelegates.OnRoundEnded != null)
        {
            GameStateDelegates.OnRoundEnded(CurrentGameState);
        }

        isResolvingTurn = false;

        if (GameStateDelegates.OnCurrentGameStateChange != null)
        {
            GameStateDelegates.OnCurrentGameStateChange(CurrentGameState);
        }

        upcomingGamestates.Clear();
    }

    void GenerateNextTurnStack(GameState gameState)
    {
        enemyTurnCalculator.CalculateAndQueueEnemyTurns(CurrentGameState);
        turnStackController.AddEmptyPlayerTurn();
        turnStackController.OnTurnStackUpdate(new List<Turn>(turnStackController.TurnStack));
    }
}
