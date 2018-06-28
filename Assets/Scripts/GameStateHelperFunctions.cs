﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameStateHelperFunctions {
    public static Direction GetDirectionFromPlayer(Vector2Int cellPosition, GameState state)
    {
        if (cellPosition.x > state.player.Position.x && cellPosition.y == state.player.Position.y)
        {
            return Direction.Right;
        }
        else if (cellPosition.x < state.player.Position.x && cellPosition.y == state.player.Position.y)
        {
            return Direction.Left;
        }
        else if (cellPosition.x == state.player.Position.x && cellPosition.y > state.player.Position.y)
        {
            return Direction.Down;
        }
        else if (cellPosition.x == state.player.Position.x && cellPosition.y < state.player.Position.y)
        {
            return Direction.Up;
        }
        else
        {
            Debug.LogError("Cell was not a cardinal direction from player.");
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
