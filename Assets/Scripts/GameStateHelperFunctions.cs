using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameStateHelperFunctions {
    public static Direction GetDirectionFromEntity(EntityData entity, Vector2Int targetPosition)
    {
        Vector2Int entityPosition = entity.Position;
        return GetDirectionFromPosition(entityPosition, targetPosition);
    }

    public static Direction GetDirectionFromPosition(Vector2Int startingPosition, Vector2Int targetPosition)
    {
        if (targetPosition.x > startingPosition.x && targetPosition.y == startingPosition.y)
        {
            return Direction.Right;
        }
        else if (targetPosition.x < startingPosition.x && targetPosition.y == startingPosition.y)
        {
            return Direction.Left;
        }
        else if (targetPosition.x == startingPosition.x && targetPosition.y > startingPosition.y)
        {
            return Direction.Down;
        }
        else if (targetPosition.x == startingPosition.x && targetPosition.y < startingPosition.y)
        {
            return Direction.Up;
        }
        else
        {
            Debug.LogError("Cell was not a cardinal direction from entity.");
            return Direction.Right;
        }

    }

    public static EntityData GetTileOccupant(Vector2Int position, GameState state)
    {
        if (state.player.Position == position)
        {
            return state.player;  
        }

        return state.enemies.Find(enemy => enemy.Position == position);
    }

    public static EntityData GetTileOccupant(Tile tile, GameState state)
    {
        return GetTileOccupant(tile.Position, state);
    }

    public static int GetTileDistanceFromPlayer(Vector2Int cellPosition, GameState state)
    {
        int xDifference = Mathf.Abs(cellPosition.x - state.player.Position.x);

        return xDifference != 0 ? xDifference : Mathf.Abs(cellPosition.y - state.player.Position.y);
    }

    public static bool IsTileOccupied(int x, int y, GameState state)
    {
        return IsTileOccupied(new Vector2Int(x, y), state);
    }

    public static bool IsTileOccupied(Tile tile, GameState state)
    {
        return IsTileOccupied(tile.Position, state);
    }

    public static bool IsTileOccupied(Vector2Int position, GameState state)
    {
        return state.player.Position == position || state.enemies.Any<EntityData>(entityData => entityData.Position == position);
    }

    public static bool IsTileOccupied(Vector2Int originPosition, Direction directionFromOrigin, int distanceFromOrigin, GameState state)
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

        return IsTileOccupied(updatedPosition.x, updatedPosition.y, state);
    }

    public static GameState CalculateFollowingGameState(GameState currentState)
    {
        GameState projectedState = DeepCopyGameState(currentState);
        projectedState.actionsCompletedLastRound.Clear();

        while (projectedState.turnStack.Count > 0)
        {
            Turn nextTurn = projectedState.turnStack.Pop();

            ProcessTurn(nextTurn, projectedState);
        }

        return projectedState;
    }

    public static List<Vector2Int> GetAllPositionsThroughWhichEntityWillMove(EntityData entity, GameState currentGameState)
    {
        List<Vector2Int> results = new List<Vector2Int>();

        GameState copiedGameState = DeepCopyGameState(currentGameState);
        EntityData copiedEntity =
            entity == currentGameState.player ?
            copiedGameState.player :
            copiedGameState.enemies.First(enemy => enemy == entity);
        Vector2Int lastPosition = copiedEntity.Position;

        Turn entityTurn = copiedGameState.turnStack.First(t => t.Entity == copiedEntity);

        Turn nextTurn = copiedGameState.turnStack.Pop();

        // Fast-forward to entity's turn.
        while (nextTurn != entityTurn)
        {
            ProcessTurn(nextTurn, copiedGameState);
            // If entity gets bumped around at all, add their new positions to the list.
            if (copiedEntity.Position != lastPosition)
            {
                lastPosition = copiedEntity.Position;
                results.Add(lastPosition);
            }
            nextTurn = copiedGameState.turnStack.Pop();
        }

        // Add every single position occupied to the list.
        for (int i = 0; i < entityTurn.moves.Count; i++)
        {
            Direction move = entityTurn.moves[i];

            ProcessMove(move, copiedEntity, copiedGameState);
            results.Add(copiedEntity.Position);
        }

        return results;
    }

    public static bool IsTileValid(Vector2Int position)
    {
        return position.x >= 0 && position.x < BoardController.BoardWidth && position.y >= 0 && position.y < BoardController.BoardWidth;
    }

    // Currently, this silently fails if the turn is 'empty' so that the game can extrapolate
    // following gamestates regardless of whether the player has chosen their turn yet.
    // If there's ever an empty turn for any other reason, this is going to be a problem—
    // but there *should* (lol) never be an empty turn for any other reason.
    public static void ProcessTurn(Turn turn, GameState state)
    {
        if (turn.moves.Count > 0)
        {
            ProcessMoves(turn.moves, turn.Entity, state);
        }
        if (turn.action != null && turn.action.card != null)
        {
            ProcessAction(turn.action, state);
        }
    }

    // Used for incremental updating during actual-turn resolution.
    public static void ProcessMove(Direction move, EntityData entity, GameState state)
    {
        TryToMoveEntityInDirection(entity, move, state);
    }

    // Used for extrapolating next turn.
    public static void ProcessMoves(List<Direction> moves, EntityData entity, GameState state)
    {
        Tile originTile = BoardHelperFunctions.GetTileAtPosition(entity.Position);
        for (int i = 0; i < moves.Count; i++)
        {
            Direction nextMove = moves[i];
            TryToMoveEntityInDirection(entity, nextMove, state);
        }

        Tile destinationTile = BoardHelperFunctions.GetTileAtPosition(entity.Position);

        CompletedMove completedMove = new CompletedMove(moves, entity, originTile, destinationTile);
        state.movesCompletedLastRound.Add(completedMove);
    }

    static void TryToMoveEntityInDirection(EntityData entity, Direction direction, GameState state)
    {
        Tile currentTile = BoardHelperFunctions.GetTileAtPosition(entity.Position);

        if (!currentTile.ConnectsToNeighbor(direction))
        { 
            return;
        }

        Tile nextTile = currentTile.GetDirectionalNeighbor(direction);

        if (IsTileOccupied(nextTile, state))
        {
            EntityData tileOccupant = GetTileOccupant(nextTile.Position, state);

            Tile projectedBumpTile = nextTile.GetDirectionalNeighbor(direction);

            bool canBump = projectedBumpTile != null && !IsTileOccupied(projectedBumpTile, state);

            if (canBump)
            {
                Vector2Int projectedBumpPosition = projectedBumpTile.Position;
                tileOccupant.Position = projectedBumpPosition;
                entity.Position = nextTile.Position;
            }

            tileOccupant.Health -= 1;
            entity.Health -= 1;
        }
        else
        {
            entity.Position = nextTile.Position;
        }
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
        Vector2Int projectedPosition = GetTilePosition(entity.Position, direction, distance);

        if (!IsTileValid(projectedPosition))
        {
            return;
        }

        if (IsTileOccupied(projectedPosition, gameState))
        {
            EntityData tileOccupant = GetTileOccupant(projectedPosition, gameState);

            Vector2Int projectedBumpPosition = GetTilePosition(projectedPosition, direction, 1);

            bool canBump = BoardHelperFunctions.GetTileAtPosition(projectedPosition).HasNeighborWhere(neighb => neighb.Position == projectedBumpPosition) && !IsTileOccupied(projectedBumpPosition, gameState);

            if (canBump)
            {
                tileOccupant.Position = projectedBumpPosition;
                entity.Position = projectedPosition;
            }

            tileOccupant.Health -= 1;
            entity.Health -= 1;
        }
        else
        {
            entity.Position = projectedPosition;
        }
    }

    static void HandleAttackAction(EntityData entity, AttackCardData card, Direction direction, int distance, GameState gameState)
    {
        Tile originTile = BoardHelperFunctions.GetTileAtPosition(entity.Position);
        Tile targetTile = FindFirstOccupiedTileInDirection(originTile, direction, distance, gameState);

        CompletedAction completedAction = new CompletedAction(originTile, targetTile, direction, card.Category);
        gameState.actionsCompletedLastRound.Add(completedAction);

        if (!IsTileOccupied(targetTile, gameState))
        {
            return;
        }

        EntityData targetEntity = GetTileOccupant(targetTile, gameState);

        targetEntity.Health -= card.Damage;
    }

    static Tile FindFirstOccupiedTileInDirection(Tile originTile, Direction direction, int distance, GameState state)
    {
        Tile currentTargetTile = originTile;
        Tile testTargetTile = originTile.GetDirectionalNeighbor(direction);

        while (distance > 0 && testTargetTile != null)
        {
            currentTargetTile = testTargetTile;
            testTargetTile = currentTargetTile.GetDirectionalNeighbor(direction);

            if (IsTileOccupied(currentTargetTile, state)) 
            {
                break;
            }

            distance--;
        }

        return currentTargetTile;
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

    public static GameState DeepCopyGameState(GameState originalState)
    {
        EntityData playerCopy = ScriptableObject.Instantiate(originalState.player);

        List<EntityData> enemyCopies = new List<EntityData>();

        for (int i = 0; i < originalState.enemies.Count; i++)
        {
            EntityData enemyCopy = ScriptableObject.Instantiate(originalState.enemies[i]);
            enemyCopies.Add(enemyCopy);
        }

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
                int originalTurnSubjectIndex = originalState.enemies.FindIndex(enemy => enemy == turnSubject);
                newTurn.UpdateEntity(enemyCopies[originalTurnSubjectIndex]);
            }

            newTurnList.Add(newTurn);
        }

        Stack<Turn> newTurnStack = new Stack<Turn>();

        for (int i = newTurnList.Count - 1; i >= 0; i--)
        {
            newTurnStack.Push(newTurnList[i]);
        }

        return new GameState(playerCopy, enemyCopies, newTurnStack);
    }
}
