using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ScenarioStateManager : MonoBehaviour {

    [SerializeField]
    GameStateManager gameStateManager;
    [SerializeField]
    EnemySpawnGroupManager enemySpawnGroupManager;
    
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
        EnemySpawnGroupData enemySpawnGroup = enemySpawnGroupManager.GetEnemySpawnGroups(depth).GetRandomElement();
        GenerateAndDrawScenario(enemySpawnGroup);
    }

    public void GenerateAndDrawScenario(EnemySpawnGroupData enemySpawnGroup)
    {
        gameObject.SetActive(true);
        GameBoard board = boardController.GenerateBoard();
        CurrentScenarioState = ScenarioStateGenerator.GenerateNewScenarioState(board, enemySpawnGroup);
        GenerateNextTurnStack(CurrentScenarioState);
        PlaceExitArrow(exitArrow, boardController.currentBoard.Exit.Position, boardController.currentBoard.BoardWidth);
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
            upcomingScenarioStates = UpcomingStateCalculator.CalculateUpcomingStates(CurrentScenarioState);
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
        ResetBoard(CurrentScenarioState, upcomingScenarioStates);

        int cardRange = card.range;

        // Movement availability is always 'from' player's current position.
        // Other actions are 'from' player's projected position.
        Vector2Int playerOrigin = card.category == CardCategory.Movement ? Player.Position : ProjectedPlayerPosition;

        if (card.category == CardCategory.Movement)
        {
            cardRange += Player.GetMovementModifierValue();
            BoardHelperFunctions.GetAllTilesWithinRange(playerOrigin, cardRange).ForEach(t => HighlightCell(t.Position));
        }
        else
        {
            BoardHelperFunctions.GetTilesWithinLinearRange(playerOrigin, cardRange).ForEach(t => HighlightCell(t.Position));
        }
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
            Vector2Int playerOrigin = selectedCard.category == CardCategory.Movement ? Player.Position : ProjectedPlayerPosition;
            turnStackController.AddToPlayerTurn(selectedCard, Player, playerOrigin, tileClickedPosition);
            equippedCardsManager.ClearSelectedCard();
            GameStateDelegates.OnCurrentScenarioStateChange(CurrentScenarioState, upcomingScenarioStates);
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
            ScenarioState nextScenarioState = dequeuedProjectedState.scenarioState;

            UpdateScenarioState(nextScenarioState);

            upcomingScenarioStates = new List<ProjectedGameState>(upcomingStateQueue);

            GameStateDelegates.OnCurrentScenarioStateChange(CurrentScenarioState, upcomingScenarioStates);
            dequeuedProjectedState.attackedPositions.ForEach(pos => GameStateDelegates.OnPositionAttacked(pos));

            yield return new WaitForSeconds(0.5f);
        }

        CurrentScenarioState.items = UpdateItemDurations(CurrentScenarioState);
        UpdateExitArrowVisibility();

        GameStateDelegates.OnRoundEnded?.Invoke(CurrentScenarioState);

        isResolvingTurn = false;

        GameStateDelegates.OnCurrentScenarioStateChange?.Invoke(CurrentScenarioState, upcomingScenarioStates);

        upcomingScenarioStates.Clear();
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
