using System;   
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static Helpers.Operators;

public static class GameStateHelperFunctions {
    #region tile info
    public static bool IsTileValid(Vector2Int position)
    {
        return position.x >= 0 && position.x < BoardController.BoardWidth && position.y >= 0 && position.y < BoardController.BoardWidth;
    }

    public static Vector2Int GetTilePosition(Vector2Int origin, Direction direction, int distance)
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

    #region entity info/manip
    public static EntityData GetTileOccupant(this GameState state, Tile tile)
    {
        return state.GetTileOccupant(tile.Position);
    }

    public static Direction GetDirectionFromEntity(EntityData entity, Vector2Int targetPosition)
    {
        Vector2Int entityPosition = entity.Position;
        return BoardHelperFunctions.GetDirectionFromPosition(entityPosition, targetPosition);
    }

    public static EntityData GetTileOccupant(this GameState state, Vector2Int position)
    {
        if (state.player.Position == position)
        {
            return state.player;  
        }

        return state.enemies.Find(enemy => enemy.Position == position);
    }

    public static int GetTileDistanceFromPlayer(Vector2Int cellPosition, GameState state)
    {
        int xDifference = Mathf.Abs(cellPosition.x - state.player.Position.x);

        return xDifference != 0 ? xDifference : Mathf.Abs(cellPosition.y - state.player.Position.y);
    }

    public static bool IsTileOccupied(this GameState state, int x, int y)
    {
        return state.IsTileOccupied(new Vector2Int(x, y));
    }

    public static bool IsTileOccupied(this GameState state, Tile tile)
    {
        return state.IsTileOccupied(tile.Position);
    }

    public static bool IsTileOccupied(this GameState state, Vector2Int position)
    {
        return state.player.Position == position || state.enemies.Any<EntityData>(entityData => entityData.Position == position);
    }

    public static bool IsTileOccupied(this GameState state, Vector2Int originPosition, Direction directionFromOrigin, int distanceFromOrigin)
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

        return state.IsTileOccupied(updatedPosition.x, updatedPosition.y);
    }

    public static List<EntityData> GetAllEntities(this GameState state)
    {
        return state.enemies.Append(state.player).ToList();
    }

    public static EntityData GetEntityWhere(this GameState state, Predicate<EntityData> predicate)
    {
        return state.GetAllEntities().Find(e => predicate(e));
    }

    public static Tile FindFirstOccupiedTileInDirection(this GameState state, Tile originTile, Direction direction, int distance)
    {
        Tile currentTargetTile = originTile;
        Tile testTargetTile = originTile.GetDirectionalNeighbor(direction);

        while (distance > 0 && testTargetTile != null)
        {
            currentTargetTile = testTargetTile;
            testTargetTile = currentTargetTile.GetDirectionalNeighbor(direction);

            if (state.IsTileOccupied(currentTargetTile)) 
            {
                break;
            }

            distance--;
        }

        return currentTargetTile;
    }
    #endregion

    #region item info/manip
    public static bool DoesPositionContainItem(this GameState state, Vector2Int position)
    {
        return state.items.Any(item => item.Position == position);
    }

    public static ItemData GetItemInPosition(this GameState state, Vector2Int position)
    {
        return state.items.First(item => item.Position == position);
    }
    #endregion

    public static GameState DeepCopy(this GameState originalState)
    {
        EntityData playerCopy = originalState.player.Copy();

        List<EntityData> enemyCopies = originalState.enemies.Select(entity => entity.Copy()).ToList();
        List<ItemData> itemCopies = originalState.items.Select(item => item.Copy()).ToList();

        List<Turn> newTurnList = new List<Turn>();
        foreach (Turn turn in originalState.turnStack)
        {
            Turn newTurn = new Turn(turn.Entity, turn.moves.GetRange(0, turn.moves.Count), turn.action);
            EntityData turnSubject = newTurn.Entity;

            if (turnSubject == originalState.player)
            {
                newTurn.UpdateEntity(playerCopy);
            }
            else
            {
                int originalTurnSubjectIndex = originalState.enemies.FindIndex(enemy => enemy.ID == turnSubject.ID);
                newTurn.UpdateEntity(enemyCopies[originalTurnSubjectIndex]);
            }

            newTurnList.Add(newTurn);
        }
        Stack<Turn> newTurnStack = new Stack<Turn>();

        for (int i = newTurnList.Count - 1; i >= 0; i--)
        {
            newTurnStack.Push(newTurnList[i]);
        }

        return new GameState(playerCopy, enemyCopies, itemCopies, newTurnStack);
    }
}
