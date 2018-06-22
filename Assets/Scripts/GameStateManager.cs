using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameStateManager : MonoBehaviour {

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
    public EntityData Player { get { return currentGameState.player; } }

    List<Vector2Int> potentialCardTargets;

    private void Awake()
    {
        potentialCardTargets = new List<Vector2Int>();
        boardWidth = boardController.BoardWidth;
    }

    private void Start()
    {
        currentGameState = GameStateGenerator.GenerateNewGameState();
        // This has to be delayed so layout group can space accordingly.
        Invoke("SetBoardUp", 0.1f);
    }

    void SetBoardUp()
    {
        enemyActionCalculator.CalculateAndQueueActions(currentGameState);
        ResetBoard();
    }

    public void HighlightPotentialCardTargets(CardData card)
    {
        ResetBoard();

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
        if (IsCellValid(position) && !IsCellOccupied(position))
        {
            boardController.HighlightCell(position);
            potentialCardTargets.Add(position);
        }
    }

    public void ResetBoard()
    {
        boardController.DrawBoard(currentGameState);
        potentialCardTargets.Clear();
    }

    public void RegisterCellInteraction(Vector2Int cellPosition)
    {
        if (potentialCardTargets.Contains(cellPosition))
        {
            actionQueueController.AddPlayerAction(equippedCardsManager.GetSelectedCard(), Player, GetDirectionFromPlayer(cellPosition), GetCellDistanceFromPlayer(cellPosition));
            equippedCardsManager.ClearSelectedCard();
        }
        else
        {
        }
    }

    public void EndTurn()
    {
        while (!actionQueueController.IsActionStackEmpty)
        {
            Action nextAction = actionQueueController.GetNextAction();

            switch (nextAction.card.Category)
            {
                case CardCategory.Movement:
                    HandleMovementAction(nextAction.entity, nextAction.direction, nextAction.distance);
                    break;
                case CardCategory.Attack:
                    break;
                default:
                    break;
            }
        }

        enemyActionCalculator.CalculateAndQueueActions(currentGameState);
        ResetBoard();
    }

    void HandleMovementAction(EntityData entity, Direction direction, int distance)
    {
        switch (direction)
        {
            case Direction.Up:
                entity.Position.y -= distance; 
                break;
            case Direction.Down:
                entity.Position.y += distance;
                break;
            case Direction.Left:
                entity.Position.x -= distance;
                break;
            case Direction.Right:
                entity.Position.x += distance;
                break;
            default:
                break;
        }
    }

    Direction GetDirectionFromPlayer(Vector2Int cellPosition)
    {
        if (cellPosition.x > Player.Position.x && cellPosition.y == Player.Position.y)
        {
            return Direction.Right;
        }
        else if (cellPosition.x < Player.Position.x && cellPosition.y == Player.Position.y)
        {
            return Direction.Left;
        }
        else if (cellPosition.x == Player.Position.x && cellPosition.y > Player.Position.y)
        {
            return Direction.Down;
        }
        else if (cellPosition.x == Player.Position.x && cellPosition.y < Player.Position.y)
        {
            return Direction.Up;
        }
        else
        {
            Debug.LogError("Cell was not a cardinal direction from player.");
            return Direction.Right;
        }
    }

    #region Cell helper functions
    int GetCellDistanceFromPlayer(Vector2Int cellPosition)
    {
        int xDifference = Mathf.Abs(cellPosition.x - Player.Position.x);

        return xDifference != 0 ? xDifference : Mathf.Abs(cellPosition.y - Player.Position.y);
    }

    public bool IsCellValid(Vector2Int position)
    {
        return position.x >= 0 && position.x < boardWidth && position.y >= 0 && position.y < boardWidth;
    }

    public bool IsCellOccupied(Vector2Int position)
    {
        return currentGameState.player.Position == position || currentGameState.enemies.Any<EntityData>(entityData => entityData.Position == position);
    }
    #endregion
}
