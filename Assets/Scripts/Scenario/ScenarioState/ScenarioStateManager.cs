using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ScenarioStateManager : MonoBehaviour {

    [SerializeField]
    GameObject winScreen;
    [SerializeField]
    GameStateManager gameStateManager;
    [SerializeField]
    EnemySpawnGroupManager enemySpawnGroupManager;
    [SerializeField]
    GameObject lostScreen;
    
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
    [SerializeField]
    BoardController boardController;
    [SerializeField]
    EquippedCardsManager equippedCardsManager;
    [SerializeField]
    TurnDrawer turnDrawer;

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

    public ScenarioState CurrentScenarioState { get; private set; }
    public ScenarioState ProjectedScenarioState { get { return upcomingScenarioStates.Last().scenarioState; } }
    public EntityData Player { get { return CurrentScenarioState.player; } }

    List<Vector2Int> potentialCardTargets;
    EntityData selectedEntity;
    ItemData selectedItem;
    List<ProjectedGameState> upcomingScenarioStates;
    public Vector2Int ProjectedPlayerPosition
    {
        get
        {
            if (upcomingScenarioStates.Any(state => state.activeEntity == Player))
            {
                return upcomingScenarioStates.Last(s => s.activeEntity == Player && s.action.card.category == CardCategory.Movement).activeEntity.Position;
            }
            else
            {
                return CurrentScenarioState.player.Position;
            }
        }
    }

    [SerializeField]
    GameObject exitArrow;

    bool isResolvingTurn = false;
    public bool IsResolvingTurn { get { return isResolvingTurn; } }

    // Fields for click+drag movement handling.
    CardData movementCard;
    bool isDraggingMove = false;
    int movementRange;
    List<Tile> movementTiles;

    #region lifecycle
    private void OnEnable()
    {
        upcomingScenarioStates = new List<ProjectedGameState>();
        potentialCardTargets = new List<Vector2Int>();

        turnStackController.OnTurnStackUpdate += RecalculateUpcomingStates;
        GameStateDelegates.OnCurrentScenarioStateChange += ResetBoard;
        GameStateDelegates.ReturnToDefaultBoard += ResetBoard;
    }

    private void OnDisable()
    {
        GameStateDelegates.ReturnToDefaultBoard -= ResetBoard;
        GameStateDelegates.OnCurrentScenarioStateChange -= ResetBoard;
        turnStackController.OnTurnStackUpdate -= RecalculateUpcomingStates;
    }
    #endregion

    #region initialization/reset
    public void GenerateAndDrawScenario(int depth, float nodeDistance)
    {
        EnemySpawnGroupData enemySpawnGroup = enemySpawnGroupManager.GetEnemySpawnGroup(depth, nodeDistance);
        GenerateAndDrawScenario(enemySpawnGroup);
    }

    public void GenerateAndDrawScenario(EnemySpawnGroupData enemySpawnGroup)
    {
        exitArrow.SetActive(false);
        gameObject.SetActive(true);
        GameBoard board = boardController.GenerateBoard();
        CurrentScenarioState = ScenarioStateGenerator.GenerateNewScenarioState(board, enemySpawnGroup);
        GenerateNextTurnStack(CurrentScenarioState);
        if (board.Exit != null)
        {
            PlaceExitArrow(exitArrow, boardController.currentBoard.Exit.Position, boardController.currentBoard.Width);
        }

        GameStateDelegates.OnNewScenario?.Invoke(CurrentScenarioState);
        GameStateDelegates.OnCurrentScenarioStateChange(CurrentScenarioState, upcomingScenarioStates);
    }

    public void GenerateAndDrawBossScenario(EnemySpawnGroupData bossSpawnGroup)
    {
        gameObject.SetActive(true);
        GameBoard board = boardController.GenerateBoard(true);
        CurrentScenarioState = ScenarioStateGenerator.GenerateNewScenarioState(board, bossSpawnGroup, true);
        GenerateNextTurnStack(CurrentScenarioState);
        
        GameStateDelegates.OnNewScenario?.Invoke(CurrentScenarioState);
        GameStateDelegates.OnCurrentScenarioStateChange(CurrentScenarioState, upcomingScenarioStates);
    }

    void PlaceExitArrow(GameObject arrow, Vector2Int exitPosition, int boardWidth)
    {
        arrow.transform.position = boardController.GetCellPosition(exitPosition);

        float rotation = 0f;
        if (exitPosition.x == 0)
        {
            rotation = 90f;
        }
        else if (exitPosition.x == boardWidth - 1)
        {
            rotation = -90f;
        }
        else if (exitPosition.y == boardWidth - 1)
        {
            rotation = 180f;
        }
        else if (exitPosition.y == 0)
        {
            rotation = 0f;
        }
        arrow.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, rotation));
    }

    public void ResetBoard(List<ProjectedGameState> upcomingStates)
    {
        ResetBoard(CurrentScenarioState, upcomingScenarioStates);
    }

    public void ResetBoard()
    {
        ResetBoard(CurrentScenarioState, upcomingScenarioStates);
    }

    void RecalculateUpcomingStates(List<Turn> turns)
    {
        if (!isResolvingTurn)
        {
            upcomingScenarioStates = UpcomingStateCalculator.CalculateUpcomingStates(CurrentScenarioState, BoardController.CurrentBoard);
            turnDrawer.DrawUpcomingStates(CurrentScenarioState, upcomingScenarioStates);
        }
    }

    void ResetBoard(ScenarioState currentScenarioState, List<ProjectedGameState> upcomingStates)
    {
        boardController.DrawBoard(currentScenarioState, isResolvingTurn);
        potentialCardTargets.Clear();
    }
    #endregion

    #region player interaction handling
    public void HighlightPotentialCardTargets(CardData card)
    {
        GameBoard currentBoard = boardController.currentBoard;
        ResetBoard(CurrentScenarioState, upcomingScenarioStates);

        int cardRange = card.range;

        // Movement availability is always 'from' player's current position.
        // Other actions are 'from' player's projected position.
        Vector2Int playerOrigin = card.category == CardCategory.Movement ? Player.Position : ProjectedPlayerPosition;
        Tile playerTile = currentBoard.GetTileAtPosition(playerOrigin);

        if (card.category == CardCategory.Movement)
        {
            cardRange += Player.GetMovementModifierValue();
            movementRange = cardRange;
            playerTile.Neighbors.ForEach(t => HighlightCell(t.Position));
        }
        else if (card.category == CardCategory.Self)
        {
            HighlightCell(playerTile.Position);
        }
        else
        {
            playerTile.GetTilesWithinLinearRange(cardRange).ForEach(t => HighlightCell(t.Position));
        }
    }

    void HighlightCell(Vector2Int position)
    {
        boardController.HighlightSelectedCell(position);
        potentialCardTargets.Add(position);
    }

    public void RegisterPointerDown(Vector2Int tileClickedPosition)
    {
        if (isResolvingTurn)
        {
            return;      
        }
        if (potentialCardTargets.Contains(tileClickedPosition))
        {
            CardData selectedCard = equippedCardsManager.GetSelectedCard();
            bool isMovement = selectedCard.category == CardCategory.Movement;

            Vector2Int playerOrigin = isMovement ? Player.Position : ProjectedPlayerPosition;
            if (isMovement)
            {
                movementCard = selectedCard;     
                movementRange = selectedCard.range;
                movementTiles = new List<Tile>();
                isDraggingMove = true;
                AddTileToMove(boardController.currentBoard.GetTileAtPosition(tileClickedPosition));
            }
            else
            {
                turnStackController.AddToPlayerTurn(selectedCard, Player, playerOrigin, tileClickedPosition);
                equippedCardsManager.ClearSelectedCard();
                GameStateDelegates.OnCurrentScenarioStateChange(CurrentScenarioState, upcomingScenarioStates);
            }
        }
        else if (selectedEntity != null || selectedItem != null)
        {
            DeselectEverything();
        }
        else if (CurrentScenarioState.IsTileOccupied(tileClickedPosition))
        {
            EntityData tileOccupant = CurrentScenarioState.GetTileOccupant(tileClickedPosition);
            SelectEntity(tileOccupant);
        }
        else if (CurrentScenarioState.DoesPositionContainItem(tileClickedPosition))
        {
            ItemData tileItem = CurrentScenarioState.GetItemInPosition(tileClickedPosition);
            SelectItem(tileItem);
        }
    }

    // TODO: This whole functionality probably needs a once-over for legibility/efficiency.
    // Either extends move to tile, or shortens move to tile if tile already present in move.
    public void AddTileToMove(Tile newTile)
    {
        if (!isDraggingMove)
        {
            return;
        }

        Tile originTile = boardController.currentBoard.GetTileAtPosition(Player.Position);
        Tile lastMovementTile = movementTiles.Count > 0 ? movementTiles.Last() : originTile;

        bool isValidMove = lastMovementTile.Neighbors.Contains(newTile);
        if (newTile.Position == Player.Position || !isValidMove)
        {
            StopMovementPathing();
            return;
        }
        else if (movementTiles.Contains(newTile))
        {
            int originalMoveLength = movementTiles.Count;
            int index = movementTiles.IndexOf(newTile);

            movementTiles = movementTiles.GetRange(0, index + 1).ToList();
            int newMoveLength = movementTiles.Count;
            int lostMoves = originalMoveLength - newMoveLength;
            movementRange += lostMoves;
        }
        else if (movementRange > 0)
        {
            movementRange--;
            movementTiles.Add(newTile);
        }
        turnStackController.AddToPlayerTurn(movementCard, Player, originTile, movementTiles);
        GameStateDelegates.OnCurrentScenarioStateChange(CurrentScenarioState, upcomingScenarioStates);

        if (movementRange > 0)
        {
            newTile
                .Neighbors
                .Where(n => n.Position != Player.Position)
                .ToList()
                .ForEach(t => boardController.HighlightSelectedCell(t.Position));
        }
    }

    public void StopMovementPathing()
    {
        if (!isDraggingMove)
        {
            return;
        }
        isDraggingMove = false;
        equippedCardsManager.ClearSelectedCard();
        GameStateDelegates.OnCurrentScenarioStateChange(CurrentScenarioState, upcomingScenarioStates);
    }

    public void RegisterExitArrowClick()
    {
        gameStateManager.SwitchToCampaign(CurrentScenarioState);
    }

    void SelectEntity(EntityData entity)
    {
        selectedEntity = entity;
        GameStateDelegates.OnEntitySelected(entity, CurrentScenarioState, upcomingScenarioStates);
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
        GameStateDelegates.ReturnToDefaultBoard(CurrentScenarioState, upcomingScenarioStates);
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

        Queue<ProjectedGameState> upcomingStateQueue = new Queue<ProjectedGameState>(upcomingScenarioStates);

        while (upcomingStateQueue.Count > 0)
        {
            ProjectedGameState dequeuedProjectedState = upcomingStateQueue.Dequeue();
            GameStateDelegates.OnResolvingState?.Invoke(dequeuedProjectedState);
            ScenarioState nextScenarioState = dequeuedProjectedState.scenarioState;

            if (dequeuedProjectedState.bumps.Count > 0)
            {
                dequeuedProjectedState.bumps.ForEach(b => Debug.Log(b.bumpingEntity + " bumping " + b.bumpedEntity));
            }

            UpdateScenarioState(nextScenarioState);

            upcomingScenarioStates = new List<ProjectedGameState>(upcomingStateQueue);

            GameStateDelegates.OnCurrentScenarioStateChange(CurrentScenarioState, upcomingScenarioStates);
            dequeuedProjectedState.attackedPositions.ForEach(pos => GameStateDelegates.OnPositionAttacked(pos));

            yield return new WaitForSeconds(0.5f);
        }

        if (CurrentScenarioState.isBossScenario && CurrentScenarioState.enemies.Count == 0)
        {
            winScreen.SetActive(true);
            yield break;
        }
        CurrentScenarioState.items = UpdateItemDurations(CurrentScenarioState);
        UpdateExitArrowVisibility();

        GameStateDelegates.OnRoundEnded?.Invoke(CurrentScenarioState);

        isResolvingTurn = false;
        upcomingScenarioStates.Clear();

        if (Player.CurrentHealth == 0)
        {
            lostScreen.SetActive(true);
            yield break;
        }

        GameStateDelegates.OnCurrentScenarioStateChange?.Invoke(CurrentScenarioState, upcomingScenarioStates);

        GenerateNextTurnStack(CurrentScenarioState);
    }

    void GenerateNextTurnStack(ScenarioState gameState)
    {
        enemyTurnCalculator.CalculateAndQueueEnemyTurns(CurrentScenarioState);
        turnStackController.AddEmptyPlayerTurn();
        turnStackController.OnTurnStackUpdate(new List<Turn>(turnStackController.TurnStack));
    }

    List<ItemData> UpdateItemDurations(ScenarioState currentScenarioState)
    {
        List<ItemData> items = currentScenarioState.items;

        items.ForEach(i => i.Duration--);

        return items.Where(i => i.Duration > 0).ToList();
    }
    
    void UpdateExitArrowVisibility()
    {
        if (BoardController.CurrentBoard.Exit == null)
        {
            return;
        }
        bool playerOnExit = Player.Position == BoardController.CurrentBoard.Exit.Position;

        exitArrow.SetActive(playerOnExit);
    }

    void UpdateScenarioState(ScenarioState newScenarioState)
    {
        CurrentScenarioState = newScenarioState;
        GameStateManager.CurrentCampaign.player = newScenarioState.player;
        GameStateManager.CurrentCampaign.inventory = newScenarioState.inventory;

        GameStateDelegates.OnCampaignStateUpdated(GameStateManager.CurrentCampaign);
    }
    #endregion

}
