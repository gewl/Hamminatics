using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameStateManager : MonoBehaviour {

    public delegate void GameStateChangeDelegate(GameState updatedGameState);
    public GameStateChangeDelegate OnCurrentGameStateChange;
    public GameStateChangeDelegate OnTurnEnded;

    ActionStackController _actionStackController;
    ActionStackController actionStackController
    {
        get
        {
            if (_actionStackController == null)
            {
                _actionStackController = GetComponent<ActionStackController>();
            }

            return _actionStackController;
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
    EnemyActionCalculator _enemyActionCalculator;
    EnemyActionCalculator enemyActionCalculator
    {
        get
        {
            if (_enemyActionCalculator == null)
            {
                _enemyActionCalculator = GetComponent<EnemyActionCalculator>();
            }

            return _enemyActionCalculator;
        }
    }

    int boardWidth;

    public GameState CurrentGameState { get; private set; }
    public GameState ProjectedGameState { get; private set; }
    public EntityData Player { get { return CurrentGameState.player; } }

    List<Vector2Int> potentialCardTargets;

    bool isHandlingActions = false;
    public bool IsHandlingActions { get { return isHandlingActions; } }

    private void Awake()
    {
        potentialCardTargets = new List<Vector2Int>();
        boardWidth = boardController.BoardWidth;
    }

    private void Start()
    {
        CurrentGameState = GameStateGenerator.GenerateNewGameState();
        OnCurrentGameStateChange += ResetBoard;
        // This has to be delayed so layout group can space accordingly.
        Invoke("SetBoardUp", 0.1f);
    }

    private void OnEnable()
    {
        actionStackController.OnActionStackUpdate += ResetBoard; 
    }

    private void OnDisable()
    {
        actionStackController.OnActionStackUpdate -= ResetBoard; 
    }

    void SetBoardUp()
    {
        enemyActionCalculator.CalculateAndQueueActions(CurrentGameState);
        OnCurrentGameStateChange(CurrentGameState);
    }

    public void HighlightPotentialCardTargets(CardData card)
    {
        ResetBoard(CurrentGameState);

        int cardRange = card.Range;

        int leftMaximumX = Player.Position.x - cardRange;
        int rightMaximumX = Player.Position.x + cardRange;
        int topMaximumY = Player.Position.y - cardRange;
        int bottomMaximumY = Player.Position.y + cardRange;

        int testX = leftMaximumX;
        int testY = topMaximumY;

        while (testX <= rightMaximumX)
        {
            Vector2Int testPosition = new Vector2Int(testX, Player.Position.y);

            AttemptToHighlightCell(testPosition);

            testX++;
        }

        while (testY <= bottomMaximumY)
        {
            Vector2Int testPosition = new Vector2Int(Player.Position.x, testY);

            AttemptToHighlightCell(testPosition);

            testY++;
        }
    }

    void AttemptToHighlightCell(Vector2Int position)
    {
        if (IsCellValid(position) && Player.Position != position)
        {
            boardController.HighlightCell(position);
            potentialCardTargets.Add(position);
        }
    }

    public void ResetBoard(List<Action> actionList)
    {
        ResetBoard(CurrentGameState);
    }

    public void ResetBoard()
    {
        ResetBoard(CurrentGameState);
    }

    void ResetBoard(GameState currentGameState)
    {
        if (!isHandlingActions)
        {
            ProjectedGameState = CalculateFollowingGameState(currentGameState);
        }
        boardController.DrawBoard(currentGameState, ProjectedGameState);
        potentialCardTargets.Clear();
    }

    public void RegisterCellInteraction(Vector2Int cellPosition)
    {
        if (isHandlingActions)
        {
            return;      
        }
        if (potentialCardTargets.Contains(cellPosition))
        {
            actionStackController.AddPlayerAction(equippedCardsManager.GetSelectedCard(), Player, GameStateHelperFunctions.GetDirectionFromPlayer(cellPosition, CurrentGameState), GameStateHelperFunctions.GetCellDistanceFromPlayer(cellPosition, CurrentGameState));
            equippedCardsManager.ClearSelectedCard();
            OnCurrentGameStateChange(CurrentGameState);
        }
        else
        {
        }
    }

    public void EndTurn()
    {
        StartCoroutine(ProcessCurrentTurnActions());
    }

    IEnumerator ProcessCurrentTurnActions()
    {
        isHandlingActions = true;
        while (!actionStackController.IsActionStackEmpty)
        {
            Action nextAction = actionStackController.GetNextAction();

            ProcessAction(nextAction, CurrentGameState);

            OnCurrentGameStateChange(CurrentGameState);
            yield return new WaitForSeconds(0.5f);
        }

        enemyActionCalculator.CalculateAndQueueActions(CurrentGameState);

        if (OnCurrentGameStateChange != null)
        {
            OnCurrentGameStateChange(CurrentGameState);
        }

        if (OnTurnEnded != null)
        {
            OnTurnEnded(CurrentGameState);
        }
        isHandlingActions = false;
    }

    void ProcessAction(Action action, GameState state)
    {
        switch (action.card.Category)
        {
            case CardCategory.Movement:
                HandleMovementAction(action.entity, action.direction, action.distance, state);
                break;
            case CardCategory.Attack:
                HandleAttackAction(action.entity, action.card as AttackCardData, action.direction, action.distance, state);
                break;
            default:
                break;
        }
    }

    void HandleMovementAction(EntityData entity, Direction direction, int distance, GameState gameState)
    {
        Vector2Int projectedPosition = GetCellPosition(entity.Position, direction, distance);

        if (!IsCellValid(projectedPosition))
        {
            return;
        }

        if (GameStateHelperFunctions.IsCellOccupied(projectedPosition, gameState))
        {
            EntityData cellOccupant = GameStateHelperFunctions.GetOccupantOfCell(projectedPosition, gameState);

            Vector2Int projectedBumpPosition = GetCellPosition(projectedPosition, direction, 1);

            bool canBump = IsCellValid(projectedBumpPosition) && !GameStateHelperFunctions.IsCellOccupied(projectedBumpPosition, gameState);

            if (canBump)
            {
                cellOccupant.Position = projectedBumpPosition;
                entity.Position = projectedPosition;
            }

            cellOccupant.Health -= 1;
            entity.Health -= 1;
        }
        else
        {
            entity.Position = projectedPosition;
        }
    }

    void HandleAttackAction(EntityData entity, AttackCardData card, Direction direction, int distance, GameState gameState)
    {
        Vector2Int targetCellPosition = GetCellPosition(entity.Position, direction, distance);
        if (!IsCellValid(targetCellPosition) || !GameStateHelperFunctions.IsCellOccupied(targetCellPosition, gameState))
        {
            return;
        }

        EntityData targetEntity = GameStateHelperFunctions.GetOccupantOfCell(targetCellPosition, gameState);

        targetEntity.Health -= card.Damage;
    }

    GameState CalculateFollowingGameState(GameState currentState)
    {
        GameState projectedState = GameStateHelperFunctions.DeepCopyGameState(currentState);

        while (projectedState.actionStack.Count > 0)
        {
            Action nextAction = projectedState.actionStack.Pop();

            ProcessAction(nextAction, projectedState);
        }

        return projectedState;
    }

    #region Cell helper functions

    public bool IsCellValid(Vector2Int position)
    {
        return position.x >= 0 && position.x < boardWidth && position.y >= 0 && position.y < boardWidth;
    }

    public Vector2Int GetCellPosition(Vector2Int origin, Direction direction, int distance)
    {
        Vector2Int updatedPosition = origin;

        switch (direction)
        {
            case Direction.Up:
                updatedPosition.y -= distance;
                break;
            case Direction.Down:
                updatedPosition.y += distance;
                break;
            case Direction.Left:
                updatedPosition.x -= distance;
                break;
            case Direction.Right:
                updatedPosition.x += distance;
                break;
            default:
                break;
        }

        return updatedPosition;
    }

    #endregion
}
