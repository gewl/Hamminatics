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

        ResolveBump(entity, tileOccupant, move, newState);

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

        targetEntity.DealDamage(card.damage, newState);

        List<ModifierData> attackModifiers = action.card.modifiers;
        if (attackModifiers != null && attackModifiers.Count > 0)
        {
            for (int i = 0; i < attackModifiers.Count; i++)
            {
                ApplyModifierToAttack(targetEntity, attackModifiers[i], entity, newState);
            }
        }

        return newProjectedState;
    }

    static void ApplyModifierToAttack(EntityData target, ModifierData modifier, EntityData attacker, ScenarioState gameState)
    {
        switch (modifier.modifierCategory)
        {
            case ModifierCategory.Slow:
                Debug.Log("Applying modifier of type: " + modifier.modifierCategory);
                break;
            case ModifierCategory.Speed:
                Debug.Log("Applying modifier of type: " + modifier.modifierCategory);
                break;
            case ModifierCategory.Weaken:
                Debug.Log("Applying modifier of type: " + modifier.modifierCategory);
                break;
            case ModifierCategory.Strength:
                Debug.Log("Applying modifier of type: " + modifier.modifierCategory);
                break;
            case ModifierCategory.DamageOverTime:
                Debug.Log("Applying modifier of type: " + modifier.modifierCategory);
                break;
            case ModifierCategory.HealOverTime:
                Debug.Log("Applying modifier of type: " + modifier.modifierCategory);
                break;
            case ModifierCategory.Push:
                ApplyModifierToAttack_Push(target, modifier, attacker, gameState);
                break;
            case ModifierCategory.Pull:
                Debug.Log("Applying modifier of type: " + modifier.modifierCategory);
                break;
            default:
                break;
        }
    }

    static void ApplyModifierToAttack_Push(EntityData target, ModifierData modifier, EntityData attacker, ScenarioState gameState)
    {
        Direction fromAttackerToTarget = BoardHelperFunctions.GetDirectionFromPosition(attacker.Position, target.Position);
        int pushMagnitude = modifier.value;

        while (pushMagnitude > 0)
        {
            Tile currentTargetTile = BoardController
                .CurrentBoard
                .GetTileAtPosition(target.Position);
            bool canPushTarget = currentTargetTile
                .ConnectsToNeighbor(fromAttackerToTarget);

            if (canPushTarget)
            {
                Tile nextTile = currentTargetTile.GetDirectionalNeighbor(fromAttackerToTarget);
                bool isNextTileOccupied = nextTile.IsOccupied(gameState);

                if (isNextTileOccupied)
                {
                    ResolveBump(target, gameState.GetTileOccupant(nextTile), fromAttackerToTarget, gameState);
                    break;
                }
                else
                {
                    target.SetPosition(currentTargetTile.GetDirectionalNeighbor(fromAttackerToTarget).Position, gameState);
                    pushMagnitude--;
                }
            }
            else
            {
                target.DealDamage(1, gameState);
                break;
            }
        }
    }
    #endregion

    static void ResolveBump(EntityData bumper, EntityData bumpee, Direction bumpDirection, ScenarioState state)
    {
        Tile projectedBumpTile = BoardController
            .CurrentBoard
            .GetTileAtPosition(bumpee.Position);

        bool bumpCanPush = projectedBumpTile != null && !state.IsTileOccupied(projectedBumpTile);

        if (bumpCanPush)
        {
            Vector2Int projectedBumpPosition = projectedBumpTile.Position;
            bumper.SetPosition(bumpee.Position, state);
            bumpee.SetPosition(projectedBumpPosition, state);
        }

        bumpee.DealDamage(1, state);
        bumper.DealDamage(1, state);
    }

}
