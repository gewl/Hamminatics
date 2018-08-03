using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public static class UpcomingStateCalculator
{
    static CardData GenericMovementCard { get { return DataManager.GetGenericMovementCard(); } }

    public static List<ProjectedGameState> CalculateUpcomingStates(ScenarioState currentState)
    {
        List<ProjectedGameState> projectedGameStates = new List<ProjectedGameState>();

        ScenarioState mostRecentState = currentState.DeepCopy();
        mostRecentState.lastGameState = currentState;

        Vector2Int lastPlayerPosition = currentState.player.Position;

        while (mostRecentState.turnStack.Count > 0)
        {
            Turn turn = mostRecentState.turnStack.Pop();
            EntityData entity = turn.Entity;

            foreach (Direction move in turn.moves)
            {
                bool entityIsAliveToMove = mostRecentState.HasEntityWhere(e => e == entity);

                if (!entityIsAliveToMove)
                {
                    break;
                }

                ProjectedGameState updatedState = GetNextGameStateFromMove(mostRecentState, entity, move);
                entity = updatedState.activeEntity;

                projectedGameStates.Add(updatedState);

                mostRecentState = updatedState.scenarioState;

                if (updatedState.bump != null)
                {
                    break;
                }
            }

            if (projectedGameStates.Count > 0)
            {
                projectedGameStates.Last().scenarioState.CollectFinishMoveItems();
            }

            bool entityIsStillAlive = mostRecentState.HasEntityWhere(e => e == entity);

            if (!entityIsStillAlive)
            {
                break;
            }

            if (turn.action.card != null)
            {
                ProjectedGameState updatedState = GetNextGameStateFromAction(mostRecentState, entity, turn.action);
                entity = updatedState.activeEntity;
                projectedGameStates.Add(updatedState);

                mostRecentState = updatedState.scenarioState;
            }
        }

        return projectedGameStates;
    }

    #region state generation
    static ProjectedGameState GetNextGameStateFromMove(ScenarioState lastState, EntityData entity, Direction move)
    {
        ScenarioState newState = lastState.DeepCopy();
        newState.lastGameState = lastState;
        EntityData entityCopy = newState.GetAllEntities().Find(e => entity.ID == e.ID);
        Tile currentEntityTile = BoardController.CurrentBoard.GetTileAtPosition(entityCopy.Position);

        Action stateAction = new Action(GenericMovementCard, entity, move, 1);

        // If entity can't move that direction, return state as-is.
        if (!currentEntityTile.ConnectsToNeighbor(move))
        {
            return new ProjectedGameState(entityCopy, newState, stateAction);
        }

        Tile nextTile = currentEntityTile.GetDirectionalNeighbor(move);

        // If tile is empty, move entity there and return.
        if (!newState.IsTileOccupied(nextTile))
        {
            entityCopy.SetPosition(nextTile.Position, newState);
            return new ProjectedGameState(entityCopy, newState, stateAction);
        }

        EntityData tileOccupant = newState.GetTileOccupant(nextTile.Position);
        Tile projectedBumpTile = nextTile.GetDirectionalNeighbor(move);

        bool bumpCanPush = projectedBumpTile != null && !newState.IsTileOccupied(projectedBumpTile);

        if (bumpCanPush)
        {
            Vector2Int projectedBumpPosition = projectedBumpTile.Position;
            tileOccupant.SetPosition(projectedBumpPosition, newState);
            entityCopy.SetPosition(nextTile.Position, newState);
        }

        tileOccupant.DealDamage(1, newState);
        entityCopy.DealDamage(1, newState);

        Bump bump = new Bump(entityCopy, tileOccupant);

        return new ProjectedGameState(entityCopy, newState, stateAction, bump);
    }

    static ProjectedGameState GetNextGameStateFromAction(ScenarioState lastState, EntityData entity, Action action)
    {
        ScenarioState newState = lastState.DeepCopy();
        newState.lastGameState = lastState;
        EntityData entityCopy = newState.GetAllEntities().Find(e => entity.ID == e.ID);

        // TODO: This is pretty bare-bones right now because it isn't clear how other card categories will be
        // implemented--need to get it more set up once the basic structure of the state list is running.
        switch (action.card.category)
        {
            case CardCategory.Movement:
                return GetNextGameStateFromMove(newState, entityCopy, action.direction);
            case CardCategory.Attack:
                return GetNextStateFromAction_Attack(newState,
                    entityCopy,
                    action);
            default:
                return GetNextGameStateFromMove(newState, entityCopy, action.direction);
        }
    }

    static ProjectedGameState GetNextStateFromAction_Attack(ScenarioState newState, EntityData entity, Action action)
    {
        GameBoard testBoard = BoardController.CurrentBoard;
        AttackCardData card = action.card as AttackCardData;

        Tile attackOriginTile = BoardController.CurrentBoard.GetTileAtPosition(entity.Position);
        Tile targetTile = newState.FindFirstOccupiedTileInDirection(attackOriginTile, action.direction, action.distance);

        ProjectedGameState newProjectedState = new ProjectedGameState(entity, newState, action);

        newProjectedState.AddAttackedPosition(targetTile.Position);
        if (!newState.IsTileOccupied(targetTile))
        {
            return newProjectedState;
        }

        EntityData targetEntity = newState.GetTileOccupant(targetTile);

        targetEntity.DealDamage(card.Damage, newState);

        return newProjectedState;
    }
    #endregion

}
