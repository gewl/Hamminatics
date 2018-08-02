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
    public static EntityData GetTileOccupant(this ScenarioState state, Tile tile)
    {
        return state.GetTileOccupant(tile.Position);
    }

    public static Direction GetDirectionFromEntity(EntityData entity, Vector2Int targetPosition)
    {
        Vector2Int entityPosition = entity.Position;
        return BoardHelperFunctions.GetDirectionFromPosition(entityPosition, targetPosition);
    }

    public static EntityData GetTileOccupant(this ScenarioState state, Vector2Int position)
    {
        if (state.player.Position == position)
        {
            return state.player;  
        }

        return state.enemies.Find(enemy => enemy.Position == position);
    }

    public static int GetTileDistanceFromPlayer(Vector2Int cellPosition, ScenarioState state)
    {
        int xDifference = Mathf.Abs(cellPosition.x - state.player.Position.x);

        return xDifference != 0 ? xDifference : Mathf.Abs(cellPosition.y - state.player.Position.y);
    }

    public static bool IsTileOccupied(this ScenarioState state, int x, int y)
    {
        return state.IsTileOccupied(new Vector2Int(x, y));
    }

    public static bool IsTileOccupied(this ScenarioState state, Tile tile)
    {
        return state.IsTileOccupied(tile.Position);
    }

    public static bool IsTileOccupied(this ScenarioState state, Vector2Int position)
    {
        return state.player.Position == position || state.enemies.Any<EntityData>(entityData => entityData.Position == position);
    }

    public static bool IsTileOccupied(this ScenarioState state, Vector2Int originPosition, Direction directionFromOrigin, int distanceFromOrigin)
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

    public static List<EntityData> GetAllEntities(this ScenarioState state)
    {
        return state.enemies.Append(state.player).ToList();
    }

    public static EntityData GetEntityWhere(this ScenarioState state, Predicate<EntityData> predicate)
    {
        return state.GetAllEntities().Find(e => predicate(e));
    }

    public static bool HasEntityWhere(this ScenarioState state, Predicate<EntityData> predicate)
    {
        return state.GetAllEntities().Any(e => predicate(e));
    }

    public static Tile FindFirstOccupiedTileInDirection(this ScenarioState state, Tile originTile, Direction direction, int distance)
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
    public static bool DoesPositionContainItem(this ScenarioState state, Vector2Int position)
    {
        return state.items.Any(item => item.Position == position);
    }

    public static bool DoesPositionContainItemWhere(this ScenarioState state, Vector2Int position, Predicate<ItemData> predicate)
    {
        return state.DoesPositionContainItem(position) &&
            predicate(state.GetItemInPosition(position));
    }

    public static ItemData GetItemInPosition(this ScenarioState state, Vector2Int position)
    {
        return state.items.FirstOrDefault(item => item.Position == position);
    }

    public static ScenarioState CollectItem(this ScenarioState state, ItemData item, EntityData collector)
    {
        switch (item.itemCategory)
        {
            case ItemCategory.Treasure:
                if (collector != state.player)
                {
                    break;
                }
                TreasureData treasure = item as TreasureData;
                state.inventory.gold += treasure.value;
                state.items.Remove(item);
                break;
            case ItemCategory.Trap:
                TrapData trap = item as TrapData;
                ApplyTrapToEntity(state, trap, collector);
                break;
            default:
                break;
        }

        return state;
    }

    public static void CollectFinishMoveItems(this ScenarioState state)
    {
        List<EntityData> allEntities = state.GetAllEntities();
        for (int i = 0; i < allEntities.Count; i++)
        {
            EntityData entity = allEntities[i];

            ItemData itemAtEntityPosition = state.GetItemInPosition(entity.Position);

            if (itemAtEntityPosition == null)
            {
                continue;
            }

            state.CollectItem(itemAtEntityPosition, entity);
        }
    }

    static void ApplyTrapToEntity(ScenarioState state, TrapData trap, EntityData entity)
    {
        state.items.Remove(trap);

        switch (trap.trapCategory)
        {
            case TrapCategory.InstantDamage:
                entity.DealDamage(trap.value, state);
                break;
            case TrapCategory.Warp:
                break;
            default:
                break;
        }

        // TODO: Add handling for if trap has a modifier (buff/debuff to apply)
    }

    #endregion

    public static ScenarioState DeepCopy(this ScenarioState originalState)
    {
        EntityData playerCopy = originalState.player.Copy();

        List<EntityData> enemyCopies = originalState.enemies.Select(entity => entity.Copy()).ToList();
        List<ItemData> itemCopies = originalState.items.Select(item => item.Copy()).ToList();
        Inventory inventoryCopy = originalState.inventory.Copy();

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
                if (originalTurnSubjectIndex > -1)
                {
                    newTurn.UpdateEntity(enemyCopies[originalTurnSubjectIndex]);
                }
            }

            newTurnList.Add(newTurn);
        }
        Stack<Turn> newTurnStack = new Stack<Turn>();

        for (int i = newTurnList.Count - 1; i >= 0; i--)
        {
            newTurnStack.Push(newTurnList[i]);
        }

        return new ScenarioState(playerCopy, enemyCopies, itemCopies, newTurnStack, inventoryCopy);
    }
}
