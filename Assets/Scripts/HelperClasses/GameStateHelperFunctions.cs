using System;   
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static Helpers.Operators;

public static class GameStateHelperFunctions {
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

    public static EntityData GetTileOccupant(this GameState state, Tile tile)
    {
        return state.GetTileOccupant(tile.Position);
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

    //public static Dictionary<EntityData, Path> GenerateAllEntityPaths(this GameState state)
    //{
    //    GameState copiedState = state.DeepCopy();
    //    Dictionary<EntityData, Path> entityPathsMap = new Dictionary<EntityData, Path>();

    //    copiedState.GetAllEntities().ForEach(e => entityPathsMap.Add(e, new Path()));

    //    while (copiedState.turnStack.Count > 0)
    //    {
    //        Turn turn = copiedState.turnStack.Pop();
    //        EntityData thisEntity = turn.Entity;
    //        if (turn.ContainsMoves())
    //        {
    //            Path thisPath = turn.GetPathFromTurn(copiedState);
    //            entityPathsMap[thisEntity].AddPath(thisPath);

    //            // If this path terminated with bumping an entity,
    //            // add a "bumpedBy" pathstep to the impacted entity's path.
    //            if (thisPath.PeekLast() != null && thisPath.PeekLast().bumpedEntity != null)
    //            {
    //                PathStep bumpStep = thisPath.PeekLast();
    //                EntityData bumpedEntity = bumpStep.bumpedEntity;

    //                // Copies current state of entity so bump calculations can work off of
    //                // entity's position, health, etc., at time of bump
    //                entityPathsMap[bumpedEntity].AddStep(bumpedEntity, bumpedEntity.Position, null, bumpStep);
    //            }
    //        }

    //        if (turn.ContainsAction())
    //        {
    //            ProcessAction(turn.action, copiedState);

    //        }
    //    }

    //    return entityPathsMap;
    //}

    //static void ProcessAsMuchOfTurnAsPossible(this GameState state, Turn turn)
    //{
    //    if (turn.IsComplete())
    //    {
    //        ProcessTurn(turn, state);
    //    }
    //    else if (turn.ContainsMoves())
    //    {
    //        ProcessMoves(turn.moves, turn.Entity, state);

    //    }
    //    else if (turn.ContainsAction())
    //    {
    //        ProcessAction(turn.action, state);
    //    }
    //}

    static Path GetPathFromTurn(this Turn turn, GameState state)
    {
        Path path = new Path(turn.Entity, turn.Entity.Position);
        List<Direction> turnMoves = turn.moves;

        EntityData entity = turn.Entity;

        if (turn.moves.Count == 0)
        {
            return path;
        }

        // Add path steps until they end or a bump occurs.
        for (int i = 0; i < turnMoves.Count; i++)
        {
            Direction move = turnMoves[i];

            AddToPathFromMove(ref path, entity, move, state);

            if (path.PeekLast() != null && path.PeekLast().bumpedEntity != null)
            {
                Debug.Log("bumped, breaking");
                break;
            }

        }
        return path;
    }

    static void AddToPathFromMove(ref Path path, EntityData entity, Direction direction, GameState state)
    {
        Tile currentTile = BoardController.CurrentBoard.GetTileAtPosition(entity.Position);
        Vector2Int startingPosition = entity.Position;
        EntityData bumpedEntity = null;

        if (!currentTile.ConnectsToNeighbor(direction))
        {
            return;
        }

        Tile nextTile = currentTile.GetDirectionalNeighbor(direction);

        if (state.IsTileOccupied(nextTile))
        {
            EntityData tileOccupant = state.GetTileOccupant(nextTile.Position);

            Tile projectedBumpTile = nextTile.GetDirectionalNeighbor(direction);

            bool canBump = projectedBumpTile != null && !state.IsTileOccupied(projectedBumpTile);

            if (canBump)
            {
                Vector2Int projectedBumpPosition = projectedBumpTile.Position;
                tileOccupant.Position = projectedBumpPosition;
                entity.Position = nextTile.Position;
            }

            tileOccupant.Health -= 1;
            entity.Health -= 1;
            bumpedEntity = tileOccupant;
        }
        else
        {
            entity.Position = nextTile.Position;
        }

        path.AddStep(entity, entity.Position, bumpedEntity);
    }

    //public static List<Vector2Int> GetAllPositionsThroughWhichEntityWillMove(EntityData entity, GameState currentGameState)
    //{
    //    List<Vector2Int> results = new List<Vector2Int>();

    //    GameState copiedGameState = currentGameState.DeepCopy();
    //    EntityData copiedEntity =
    //        entity == currentGameState.player ?
    //        copiedGameState.player :
    //        copiedGameState.enemies.Find(enemy => enemy.ID == entity.ID && enemy.Position == entity.Position);
    //    Vector2Int lastPosition = copiedEntity.Position;

    //    Turn entityTurn = copiedGameState.turnStack.First(t => t.Entity == copiedEntity);

    //    Turn nextTurn = copiedGameState.turnStack.Pop();

    //    // Fast-forward to entity's turn.
    //    while (nextTurn != entityTurn)
    //    {
    //        ProcessAsMuchOfTurnAsPossible(nextTurn, copiedGameState);
    //        // If entity gets bumped around at all, add their new positions to the list.
    //        if (copiedEntity.Position != lastPosition)
    //        {
    //            lastPosition = copiedEntity.Position;
    //            results.Add(lastPosition);
    //        }
    //        nextTurn = copiedGameState.turnStack.Pop();
    //    }

    //    // Add every single position occupied to the list.
    //    for (int i = 0; i < entityTurn.moves.Count; i++)
    //    {
    //        Direction move = entityTurn.moves[i];

    //        ProcessMove(move, copiedEntity, copiedGameState);
    //        results.Add(copiedEntity.Position);
    //    }


    //    return results;
    //}

    //static void ProcessAsMuchOfTurnAsPossible(Turn turn, GameState state)
    //{
    //    if (turn.IsComplete())
    //    {

    //        ProcessTurn(turn, state);
    //    }
    //    else if (turn.ContainsMoves())
    //    {
    //        ProcessMoves(turn.moves, turn.Entity, state);

    //    }
    //    else if (turn.ContainsAction())
    //    {
    //        ProcessAction(turn.action, state);
    //    }
    //}

    public static bool IsTileValid(Vector2Int position)
    {
        return position.x >= 0 && position.x < BoardController.BoardWidth && position.y >= 0 && position.y < BoardController.BoardWidth;
    }

    // Currently, this silently fails if the turn is 'empty' so that the game can extrapolate
    // following gamestates regardless of whether the player has chosen their turn yet.
    // If there's ever an empty turn for any other reason, this is going to be a problem—
    // but there *should* (lol) never be an empty turn for any other reason.
    //public static void ProcessTurn(Turn turn, GameState state)
    //{
    //    if (turn.moves.Count > 0)
    //    {
    //        ProcessMoves(turn.moves, turn.Entity, state);
    //    }
    //    if (turn.action != null && turn.action.card != null)
    //    {
    //        ProcessAction(turn.action, state);
    //    }
    //}

    // Used for incremental updating during actual-turn resolution.
    //public static void ProcessMove(Direction move, EntityData entity, GameState state)
    //{
    //    TryToMoveEntityInDirection(entity, move, state);
    //}

    //// Used for extrapolating next turn.
    //public static void ProcessMoves(List<Direction> moves, EntityData entity, GameState state)
    //{
    //    Tile originTile = BoardController
    //        .CurrentBoard
    //        .GetTileAtPosition(entity.Position);
    //    for (int i = 0; i < moves.Count; i++)
    //    {
    //        Direction nextMove = moves[i];
    //        TryToMoveEntityInDirection(entity, nextMove, state);
    //    }

    //    Tile destinationTile = BoardController
    //        .CurrentBoard
    //        .GetTileAtPosition(entity.Position);

    //    CompletedMove completedMove = new CompletedMove(moves, entity, originTile, destinationTile);
    //    state.movesCompletedLastRound.Add(completedMove);
    //}

    static void TryToMoveEntityInDirection(EntityData entity, Direction direction, GameState state)
    {
        Tile currentTile = BoardController
            .CurrentBoard
            .GetTileAtPosition(entity.Position);

        if (!currentTile.ConnectsToNeighbor(direction))
        { 
            return;
        }

        Tile nextTile = currentTile.GetDirectionalNeighbor(direction);

        if (state.IsTileOccupied(nextTile))
        {
            EntityData tileOccupant = state.GetTileOccupant(nextTile.Position);

            Tile projectedBumpTile = nextTile.GetDirectionalNeighbor(direction);

            bool canBump = projectedBumpTile != null && !state.IsTileOccupied(projectedBumpTile);

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

    //public static void ProcessAction(Action action, GameState state)
    //{
    //    switch (action.card.Category)
    //    {
    //        case CardCategory.Movement:
    //            HandleMovementAction(action.entity, action.direction, action.distance, state);
    //            break;
    //        case CardCategory.Attack:
    //            HandleAttackAction(action.entity, action.card as AttackCardData, action.direction, action.distance, state);
    //            break;
    //        default:
    //            break;
    //    }
    //}

    static void HandleMovementAction(EntityData entity, Direction direction, int distance, GameState gameState)
    {
        Vector2Int projectedPosition = GetTilePosition(entity.Position, direction, distance);

        if (!IsTileValid(projectedPosition))
        {
            return;
        }

        if (gameState.IsTileOccupied(projectedPosition))
        {
            EntityData tileOccupant = gameState.GetTileOccupant(projectedPosition);

            Vector2Int projectedBumpPosition = GetTilePosition(projectedPosition, direction, 1);

            Predicate<Tile> IsAtProjectedBumpPosition = (Tile t) => t.Position == projectedBumpPosition;

            bool canBump = BoardController
                .CurrentBoard
                .GetTileAtPosition(projectedPosition)
                .CheckThat(And(
                    (Tile t) => t.HasNeighborWhere(IsAtProjectedBumpPosition),
                    (Tile t) => t.GetNeighborWhere(IsAtProjectedBumpPosition).IsUnoccupied(gameState)
                    ));

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

    //static void HandleAttackAction(EntityData entity, AttackCardData card, Direction direction, int distance, GameState gameState)
    //{
    //    Tile originTile = BoardController.CurrentBoard
    //        .GetTileAtPosition(entity.Position);
    //    Tile targetTile = FindFirstOccupiedTileInDirection(originTile, direction, distance, gameState);

    //    CompletedAction completedAction = new CompletedAction(originTile, targetTile, direction, card.Category);
    //    gameState.actionsCompletedLastRound.Add(completedAction);

    //    if (!gameState.IsTileOccupied(targetTile))
    //    {
    //        return;
    //    }

    //    EntityData targetEntity = gameState.GetTileOccupant(targetTile);

    //    targetEntity.Health -= card.Damage;
    //}

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

    public static GameState DeepCopy(this GameState originalState)
    {
        EntityData playerCopy = originalState.player.Copy();

        List<EntityData> enemyCopies = new List<EntityData>();

        for (int i = 0; i < originalState.enemies.Count; i++)
        {
            EntityData enemyCopy = originalState.enemies[i].Copy();
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

        return new GameState(playerCopy, enemyCopies, newTurnStack);
    }
}
