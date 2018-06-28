using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameStateManager : MonoBehaviour {

    public delegate void GameStateChangeDelegate(GameState updatedGameState);
    public GameStateChangeDelegate OnGameStateChange;
    public GameStateChangeDelegate OnTurnEnded;

    ActionStackController _actionQueueController;
    ActionStackController actionQueueController
    {
        get
        {
            if (_actionQueueController == null)
            {
                _actionQueueController = GetComponent<ActionStackController>();
            }

            return _actionQueueController;
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

    GameState currentGameState;
    public GameState CurrentGameState { get { return currentGameState; } }
    public EntityData Player { get { return currentGameState.player; } }

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
        currentGameState = GameStateGenerator.GenerateNewGameState();
        OnGameStateChange += ResetBoard;
        // This has to be delayed so layout group can space accordingly.
        Invoke("SetBoardUp", 0.1f);
    }

    void SetBoardUp()
    {
        enemyActionCalculator.CalculateAndQueueActions(currentGameState);
        OnGameStateChange(currentGameState);
    }

    public void HighlightPotentialCardTargets(CardData card)
    {
        ResetBoard(currentGameState);

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

    public void ResetBoard()
    {
        ResetBoard(currentGameState);
    }

    void ResetBoard(GameState currentGameState)
    {
        boardController.DrawBoard(currentGameState);
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
            actionQueueController.AddPlayerAction(equippedCardsManager.GetSelectedCard(), Player, GameStateHelperFunctions.GetDirectionFromPlayer(cellPosition, currentGameState), GameStateHelperFunctions.GetCellDistanceFromPlayer(cellPosition, currentGameState));
            equippedCardsManager.ClearSelectedCard();
            OnGameStateChange(currentGameState);
        }
        else
        {
        }
    }

    public void EndTurn()
    {
        StartCoroutine(ProcessTurnActions());
    }

    IEnumerator ProcessTurnActions()
    {
        isHandlingActions = true;
        while (!actionQueueController.IsActionStackEmpty)
        {
            Action nextAction = actionQueueController.GetNextAction();

            switch (nextAction.card.Category)
            {
                case CardCategory.Movement:
                    HandleMovementAction(nextAction.entity, nextAction.direction, nextAction.distance);
                    break;
                case CardCategory.Attack:
                    HandleAttackAction(nextAction.entity, nextAction.card as AttackCardData, nextAction.direction, nextAction.distance);
                    break;
                default:
                    break;
            }

            OnGameStateChange(currentGameState);
            yield return new WaitForSeconds(0.5f);
        }

        enemyActionCalculator.CalculateAndQueueActions(currentGameState);

        if (OnGameStateChange != null)
        {
            OnGameStateChange(currentGameState);
        }

        if (OnTurnEnded != null)
        {
            OnTurnEnded(currentGameState);
        }
        isHandlingActions = false;
    }

    void HandleMovementAction(EntityData entity, Direction direction, int distance)
    {
        Vector2Int projectedPosition = GetCellPosition(entity.Position, direction, distance);

        if (!IsCellValid(projectedPosition))
        {
            return;
        }

        if (GameStateHelperFunctions.IsCellOccupied(projectedPosition, currentGameState))
        {
            EntityData cellOccupant = GameStateHelperFunctions.GetOccupantOfCell(projectedPosition, currentGameState);

            Vector2Int projectedBumpPosition = GetCellPosition(projectedPosition, direction, 1);

            bool canBump = IsCellValid(projectedBumpPosition) && !GameStateHelperFunctions.IsCellOccupied(projectedBumpPosition, currentGameState);

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

    void HandleAttackAction(EntityData entity, AttackCardData card, Direction direction, int distance)
    {
        Vector2Int targetCellPosition = GetCellPosition(entity.Position, direction, distance);
        if (!IsCellValid(targetCellPosition) || !GameStateHelperFunctions.IsCellOccupied(targetCellPosition, currentGameState))
        {
            return;
        }

        EntityData targetEntity = GameStateHelperFunctions.GetOccupantOfCell(targetCellPosition, currentGameState);

        targetEntity.Health -= card.Damage;
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
