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
    public GameState ProjectedGameState { get { return upcomingGameStates.Last().gameState; } }
    public EntityData Player { get { return CurrentGameState.player; } }

    List<Vector2Int> potentialCardTargets;
    EntityData selectedEntity;
    ItemData selectedItem;
    List<ProjectedGameState> upcomingGameStates;
    public Vector2Int ProjectedPlayerPosition
    {
        get
        {
            if (upcomingGameStates.Any(state => state.activeEntity == Player))
            {
                return upcomingGameStates.Last(s => s.activeEntity == Player).activeEntity.Position;
            }
            else
            {
                return CurrentGameState.player.Position;
            }
        }
    }

    bool isResolvingTurn = false;
    public bool IsResolvingTurn { get { return isResolvingTurn; } }

    #region lifecycle
    private void Awake()
    {
        potentialCardTargets = new List<Vector2Int>();
        // This has to be delayed so layout group can space accordingly.
        Invoke("SetBoardUp", 0.1f);

        upcomingGameStates = new List<ProjectedGameState>();
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
    #endregion

    #region initialization/reset
    public void InitializeGameState(GameBoard board)
    {
        CurrentGameState = GameStateGenerator.GenerateNewGameState(board.Entrance.Position, board.BoardWidth);
        GameStateDelegates.OnCurrentGameStateChange += ResetBoard;
        GameStateDelegates.ReturnToDefaultBoard += ResetBoard;
    }

    void SetBoardUp()
    {
        InitializeGameState(boardController.InitializeBoard());
        GenerateNextTurnStack(CurrentGameState);
        GameStateDelegates.OnCurrentGameStateChange(CurrentGameState, upcomingGameStates);
    }

    public void ResetBoard(List<ProjectedGameState> upcomingStates)
    {
        ResetBoard(CurrentGameState, upcomingGameStates);
    }

    public void ResetBoard()
    {
        ResetBoard(CurrentGameState, upcomingGameStates);
    }

    void RecalculateUpcomingStates(List<Turn> turns)
    {
        if (!isResolvingTurn)
        {
            upcomingGameStates = UpcomingStateCalculator.CalculateUpcomingStates(CurrentGameState);
            turnDrawer.DrawUpcomingStates(CurrentGameState, upcomingGameStates);
        }
    }

    void ResetBoard(GameState currentGameState, List<ProjectedGameState> upcomingStates)
    {
        boardController.DrawBoard(currentGameState, isResolvingTurn);
        potentialCardTargets.Clear();
    }
    #endregion

    #region player interaction handling
    public void HighlightPotentialCardTargets(CardData card)
    {
        ResetBoard(CurrentGameState, upcomingGameStates);

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
            GameStateDelegates.OnCurrentGameStateChange(CurrentGameState, upcomingGameStates);
        }
        else if (selectedEntity != null || selectedItem != null)
        {
            DeselectEverything();
        }
        else if (CurrentGameState.IsTileOccupied(tileClickedPosition))
        {
            EntityData tileOccupant = CurrentGameState.GetTileOccupant(tileClickedPosition);
            SelectEntity(tileOccupant);
        }
        else if (CurrentGameState.DoesPositionContainItem(tileClickedPosition))
        {
            ItemData tileItem = CurrentGameState.GetItemInPosition(tileClickedPosition);
            SelectItem(tileItem);
        }
    }

    void SelectEntity(EntityData entity)
    {
        selectedEntity = entity;
        GameStateDelegates.OnEntitySelected(entity, CurrentGameState, upcomingGameStates);
    }

    void SelectItem(ItemData item)
    {
        selectedItem = item;
        GameStateDelegates.OnItemSelected(selectedItem);
    }

    void DeselectEverything()
    {
        selectedEntity = null;
        selectedItem = null;
        GameStateDelegates.ReturnToDefaultBoard(CurrentGameState, upcomingGameStates);
    }

    public void EndRound()
    {
        StartCoroutine(ProcessCurrentRoundActions());
    }
    #endregion

    #region turn transitioning

    IEnumerator ProcessCurrentRoundActions()
    {
        isResolvingTurn = true;
        DeselectEverything();

        Queue<ProjectedGameState> upcomingStateQueue = new Queue<ProjectedGameState>(upcomingGameStates);

        while (upcomingStateQueue.Count > 0)
        {
            GameState nextGameState = upcomingStateQueue.Dequeue().gameState;

            CurrentGameState = nextGameState;

            upcomingGameStates = new List<ProjectedGameState>(upcomingStateQueue);
            GameStateDelegates.OnCurrentGameStateChange(CurrentGameState, upcomingGameStates);

            yield return new WaitForSeconds(0.5f);
        }

        CurrentGameState.items = UpdateItemDurations(CurrentGameState);

        GameStateDelegates.OnRoundEnded?.Invoke(CurrentGameState);

        isResolvingTurn = false;

        GameStateDelegates.OnCurrentGameStateChange?.Invoke(CurrentGameState, upcomingGameStates);

        upcomingGameStates.Clear();
        GenerateNextTurnStack(CurrentGameState);
    }

    void GenerateNextTurnStack(GameState gameState)
    {
        enemyTurnCalculator.CalculateAndQueueEnemyTurns(CurrentGameState);
        turnStackController.AddEmptyPlayerTurn();
        turnStackController.OnTurnStackUpdate(new List<Turn>(turnStackController.TurnStack));
    }

    List<ItemData> UpdateItemDurations(GameState currentGameState)
    {
        List<ItemData> items = currentGameState.items;

        items.ForEach(i => i.Duration--);

        return items.Where(i => i.Duration > 0).ToList();
    }
    #endregion

}
