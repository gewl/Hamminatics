using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameStateHelperFunctions {
    public static Direction GetDirectionFromEntity(EntityData entity, Vector2Int cellPosition)
    {
        Vector2Int entityPosition = entity.Position;
        if (cellPosition.x > entityPosition.x && cellPosition.y == entityPosition.y)
        {
            return Direction.Right;
        }
        else if (cellPosition.x < entityPosition.x && cellPosition.y == entityPosition.y)
        {
            return Direction.Left;
        }
        else if (cellPosition.x == entityPosition.x && cellPosition.y > entityPosition.y)
        {
            return Direction.Down;
        }
        else if (cellPosition.x == entityPosition.x && cellPosition.y < entityPosition.y)
        {
            return Direction.Up;
        }
        else
        {
            Debug.LogError("Cell was not a cardinal direction from entity.");
            return Direction.Right;
        }
    }

    public static EntityData GetOccupantOfCell(Vector2Int position, GameState state)
    {
        if (state.player.Position == position)
        {
            return state.player;  
        }

        for (int i = 0; i < state.enemies.Count; i++)
        {
            EntityData entity = state.enemies[i];

            if (entity.Position == position)
            {
                return entity;
            }
        }

        Debug.LogError("Occupant of cell not found.");
        return null;
    }

    public static int GetCellDistanceFromPlayer(Vector2Int cellPosition, GameState state)
    {
        int xDifference = Mathf.Abs(cellPosition.x - state.player.Position.x);

        return xDifference != 0 ? xDifference : Mathf.Abs(cellPosition.y - state.player.Position.y);
    }

    public static bool IsCellOccupied(int x, int y, GameState state)
    {
        return IsCellOccupied(new Vector2Int(x, y), state);
    }

    public static bool IsCellOccupied(Vector2Int position, GameState state)
    {
        return state.player.Position == position || state.enemies.Any<EntityData>(entityData => entityData.Position == position);
    }

    public static bool IsCellOccupied(Vector2Int originPosition, Direction directionFromOrigin, int distanceFromOrigin, GameState state)
    {
        Vector2Int updatedPosition = originPosition;

        switch (directionFromOrigin)
        {
            case Direction.Up:
                updatedPosition.y -= distanceFromOrigin;
                break;
            case Direction.Down:
                updatedPosition.y += distanceFromOrigin;
                break;
            case Direction.Left:
                updatedPosition.x -= distanceFromOrigin;
                break;
            case Direction.Right:
                updatedPosition.x += distanceFromOrigin;
                break;
            default:
                break;
        }

        return IsCellOccupied(updatedPosition.x, updatedPosition.y, state);
    }

    public static GameState CalculateFollowingGameState(GameState currentState)
    {
        GameState projectedState = GameStateHelperFunctions.DeepCopyGameState(currentState);

        while (projectedState.actionStack.Count > 0)
        {
            Action nextAction = projectedState.actionStack.Pop();

            ProcessAction(nextAction, projectedState);

            if (nextAction.card.Category == CardCategory.Attack)
            {
                Vector2Int targetCellPosition = GetCellPosition(nextAction.entity.Position, nextAction.direction, nextAction.distance);
            }
        }

        return projectedState;
    }

    public static bool IsCellValid(Vector2Int position)
    {
        return position.x >= 0 && position.x < BoardController.BoardWidth && position.y >= 0 && position.y < BoardController.BoardWidth;
    }

    public static void ProcessAction(Action action, GameState state)
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

    static void HandleMovementAction(EntityData entity, Direction direction, int distance, GameState gameState)
    {
        Vector2Int projectedPosition = GetCellPosition(entity.Position, direction, distance);

        if (!IsCellValid(projectedPosition))
        {
            return;
        }

        if (IsCellOccupied(projectedPosition, gameState))
        {
            EntityData cellOccupant = GetOccupantOfCell(projectedPosition, gameState);

            Vector2Int projectedBumpPosition = GetCellPosition(projectedPosition, direction, 1);

            bool canBump = IsCellValid(projectedBumpPosition) && !IsCellOccupied(projectedBumpPosition, gameState);

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

    static void HandleAttackAction(EntityData entity, AttackCardData card, Direction direction, int distance, GameState gameState)
    {
        Vector2Int targetCellPosition = GetCellPosition(entity.Position, direction, distance);
        if (!IsCellValid(targetCellPosition) || !GameStateHelperFunctions.IsCellOccupied(targetCellPosition, gameState))
        {
            return;
        }

        EntityData targetEntity = GameStateHelperFunctions.GetOccupantOfCell(targetCellPosition, gameState);

        targetEntity.Health -= card.Damage;
    }

    public static Vector2Int GetCellPosition(Vector2Int origin, Direction direction, int distance)
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


    public static GameState DeepCopyGameState(GameState originalState)
    {
        EntityData playerCopy = ScriptableObject.Instantiate(originalState.player);

        List<EntityData> enemyCopies = new List<EntityData>();

        for (int i = 0; i < originalState.enemies.Count; i++)
        {
            EntityData enemyCopy = ScriptableObject.Instantiate(originalState.enemies[i]);
            enemyCopies.Add(enemyCopy);
        }

        List<Action> newActionList = new List<Action>();

        foreach (Action action in originalState.actionStack)
        {
            Action newAction = action;
            EntityData actionSubject = action.entity;

            if (actionSubject == originalState.player)
            {
                newAction.entity = playerCopy;
            }
            else
            {
                int originalActionSubjectIndex = originalState.enemies.FindIndex(enemy => enemy == actionSubject);
                newAction.entity = enemyCopies[originalActionSubjectIndex];
            }

            newActionList.Add(newAction);
        }

        Stack<Action> newActionStack = new Stack<Action>();

        for (int i = newActionList.Count - 1; i >= 0; i--)
        {
            newActionStack.Push(newActionList[i]);
        }

        return new GameState(playerCopy, enemyCopies, newActionStack);
    }
}
