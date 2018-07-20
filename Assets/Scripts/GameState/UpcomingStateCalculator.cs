using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public static class UpcomingStateCalculator
{
    public static List<ProjectedGameState> CalculateUpcomingStates(GameState currentState)
    {
        List<ProjectedGameState> projectedGameStates = new List<ProjectedGameState>();

        GameState mostRecentState = currentState.DeepCopy();
        mostRecentState.lastGamestate = currentState;
        while (mostRecentState.turnStack.Count > 0)
        {
            Turn turn = mostRecentState.turnStack.Pop();
            EntityData entity = turn.Entity;

            foreach (Direction move in turn.moves)
            {
                ProjectedGameState updatedState = GetNextGameStateFromMove(mostRecentState, entity, move);
                entity = updatedState.activeEntity;
                projectedGameStates.Add(updatedState);

                mostRecentState = updatedState.gameState;
            }

            if (turn.action.card != null)
            {
                ProjectedGameState updatedState = GetNextGameStateFromAction(mostRecentState, entity, turn.action);
                entity = updatedState.activeEntity;
                projectedGameStates.Add(updatedState);

                mostRecentState = updatedState.gameState;
            }
        }

        for (int i = 0; i < projectedGameStates.Count; i++)
        {
            ProjectedGameState projectedState = projectedGameStates[i];
        }

        return projectedGameStates;
    }

    static ProjectedGameState GetNextGameStateFromMove(GameState lastState, EntityData entity, Direction move)
    {
        GameState newState = lastState.DeepCopy();
        newState.lastGamestate = lastState;
        EntityData entityCopy = newState.GetAllEntities().Find(e => entity.ID == e.ID);
        Tile currentEntityTile = BoardController.CurrentBoard.GetTileAtPosition(entityCopy.Position);

        // If entity can't move that direction, return state as-is.
        if (!currentEntityTile.ConnectsToNeighbor(move))
        {
            return new ProjectedGameState(entityCopy, newState);
        }

        Tile nextTile = currentEntityTile.GetDirectionalNeighbor(move);

        // If tile is empty, move entity there and return.
        if (!newState.IsTileOccupied(nextTile))
        {
            entityCopy.Position = nextTile.Position;
            return new ProjectedGameState(entityCopy, newState);
        }

        EntityData tileOccupant = newState.GetTileOccupant(nextTile.Position);
        Tile projectedBumpTile = nextTile.GetDirectionalNeighbor(move);

        bool bumpCanPush = projectedBumpTile != null && !newState.IsTileOccupied(projectedBumpTile);

        if (bumpCanPush)
        {
            Vector2Int projectedBumpPosition = projectedBumpTile.Position;
            tileOccupant.Position = projectedBumpPosition;
            entityCopy.Position = nextTile.Position;
        }

        tileOccupant.Health--;
        entityCopy.Health--;

        Bump bump = new Bump(entityCopy, tileOccupant);

        return new ProjectedGameState(entityCopy, newState, bump);
    }

    static ProjectedGameState GetNextGameStateFromAction(GameState lastState, EntityData entity, Action action)
    {
        GameState newState = lastState.DeepCopy();
        newState.lastGamestate = lastState;
        EntityData entityCopy = newState.GetAllEntities().Find(e => entity.ID == e.ID);

        // TODO: This is pretty bare-bones right now because it isn't clear how other card categories will be
        // implemented--need to get it more set up once the basic structure of the state list is running.
        switch (action.card.Category)
        {
            case CardCategory.Movement:
                return GetNextGameStateFromMove(newState, entityCopy, action.direction);
            case CardCategory.Attack:
                return GetNextStateFromAction_Attack(newState, 
                    entityCopy, 
                    action.card as AttackCardData, 
                    action.direction, 
                    action.distance);
            default:
                return GetNextGameStateFromMove(newState, entityCopy, action.direction);
        }
    }

    static ProjectedGameState GetNextStateFromAction_Attack(GameState newState, EntityData entity, AttackCardData card, Direction direction, int distance)
    {
        GameBoard testBoard = BoardController.CurrentBoard;

        Tile originTile = BoardController.CurrentBoard.GetTileAtPosition(new Vector2Int(0, 0));
        Tile attackOriginTile = BoardController.CurrentBoard.GetTileAtPosition(entity.Position);
        Tile targetTile = newState.FindFirstOccupiedTileInDirection(attackOriginTile, direction, distance);

        if (!newState.IsTileOccupied(targetTile))
        {
            return new ProjectedGameState(entity, newState);
        }

        EntityData targetEntity = newState.GetTileOccupant(targetTile);

        targetEntity.Health -= card.Damage;

        return new ProjectedGameState(entity, newState);
    }
}
