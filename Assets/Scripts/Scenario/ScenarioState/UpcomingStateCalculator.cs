using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Predicates;

public static class UpcomingStateCalculator
{
    static CardData GenericMovementCard { get { return DataRetriever.GetGenericMovementCard(); } }

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

                if (updatedState.bumps.Count > 0)
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

            UpdateEntityModifiers(entity, mostRecentState);
        }

        return projectedGameStates;
    }

    static void UpdateEntityModifiers(EntityData entity, ScenarioState gameState)
    {
        for (int i = 0; i < entity.activeModifiers.Count; i++)
        {
            ModifierData modifier = entity.activeModifiers[i];

            if (modifier.modifierCategory == ModifierCategory.DamageOverTime)
            {
                entity.DealDamage(modifier.value, gameState);
            }
            else if (modifier.modifierCategory == ModifierCategory.HealOverTime)
            {
                entity.ChangeHealthValue(modifier.value);
            }

            modifier.duration--;
        }

        entity.activeModifiers = entity.activeModifiers.Where(modifier => modifier.duration > 0).ToList();
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
            List<ModifierData> modifiersToResolve = action.card.modifiers
                .Where(m => 
                    m.modifierCategory == ModifierCategory.Blowback || 
                    m.modifierCategory == ModifierCategory.FollowUp)
                .ToList();

            for (int i = 0; i < modifiersToResolve.Count; i++)
            {
                ModifierData modifier = modifiersToResolve[i];
                ApplyModifierToAttack_BlowbackFollowUp(modifier, entity, newState, action.direction, modifier.modifierCategory);
            }

            return newProjectedState;
        }

        EntityData targetEntity = newState.GetTileOccupant(targetTile);

        int cardDamage = card.damage + entity.GetAttackModifierValue();
        targetEntity.DealDamage(cardDamage, newState);

        List<ModifierData> attackModifiers = action.card.modifiers;
        if (attackModifiers != null && attackModifiers.Count > 0)
        {
            for (int i = 0; i < attackModifiers.Count; i++)
            {
                ApplyModifierToAttack(targetEntity, attackModifiers[i], entity, newState, action.direction);
            }
        }

        return newProjectedState;
    }

    static void ApplyModifierToAttack(EntityData target, ModifierData modifier, EntityData attacker, ScenarioState gameState, Direction direction)
    {
        switch (modifier.modifierCategory)
        {
            case ModifierCategory.Push:
                ApplyModifierToAttack_PushPull(target, modifier, attacker, gameState, ModifierCategory.Push);
                return;
            case ModifierCategory.Pull:
                ApplyModifierToAttack_PushPull(target, modifier, attacker, gameState, ModifierCategory.Pull);
                return;
            case ModifierCategory.Blowback:
                ApplyModifierToAttack_BlowbackFollowUp(modifier, attacker, gameState, direction, ModifierCategory.Blowback);
                return;
            case ModifierCategory.FollowUp:
                ApplyModifierToAttack_BlowbackFollowUp(modifier, attacker, gameState, direction, ModifierCategory.FollowUp);
                return;
            default:
                if (target.activeModifiers.Count >= Constants.MAX_MODIFIERS)
                {
                    return;
                }
                target.activeModifiers.Add(Object.Instantiate(modifier));
                return;
        }
    }

    static void ApplyModifierToAttack_PushPull(EntityData target, ModifierData modifier, EntityData attacker, ScenarioState gameState, ModifierCategory pushOrPull)
    {
        if (attacker.Position == target.Position)
        {
            return;
        }
        // Default to push, check for pull.
        Direction forceDirection = BoardHelperFunctions.GetDirectionFromPosition(attacker.Position, target.Position);
        if (pushOrPull == ModifierCategory.Pull)
        {
            forceDirection = BoardHelperFunctions.GetDirectionFromPosition(target.Position, attacker.Position);
        }
        int pushMagnitude = modifier.value;

        while (pushMagnitude > 0)
        {
            Tile currentTargetTile = BoardController
                .CurrentBoard
                .GetTileAtPosition(target.Position);
            bool canPushTarget = currentTargetTile
                .ConnectsToNeighbor(forceDirection);

            if (canPushTarget)
            {
                Tile nextTile = currentTargetTile.GetDirectionalNeighbor(forceDirection);
                bool isNextTileOccupied = nextTile.IsOccupied(gameState);

                if (isNextTileOccupied)
                {
                    ResolveBump(target, gameState.GetTileOccupant(nextTile), forceDirection, gameState);
                    break;
                }
                else
                {
                    target.SetPosition(currentTargetTile.GetDirectionalNeighbor(forceDirection).Position, gameState);
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

    static void ApplyModifierToAttack_BlowbackFollowUp(ModifierData modifier, EntityData entity, ScenarioState gameState, Direction actionDirection, ModifierCategory blowbackOrFollowUp)
    {
        Direction forceDirection = actionDirection;

        if (blowbackOrFollowUp == ModifierCategory.Blowback)
        {
            forceDirection = ReverseDirection(forceDirection);
        }
        int pushMagnitude = modifier.value;

        while (pushMagnitude > 0)
        {
            Tile currentEntityTile = BoardController
                .CurrentBoard
                .GetTileAtPosition(entity.Position);
            bool canPushTarget = currentEntityTile
                .ConnectsToNeighbor(forceDirection);

            if (canPushTarget)
            {
                Tile nextTile = currentEntityTile.GetDirectionalNeighbor(forceDirection);
                bool isNextTileOccupied = nextTile.IsOccupied(gameState);

                if (isNextTileOccupied)
                {
                    ResolveBump(entity, gameState.GetTileOccupant(nextTile), forceDirection, gameState);
                    break;
                }
                else
                {
                    entity.SetPosition(currentEntityTile.GetDirectionalNeighbor(forceDirection).Position, gameState);
                    pushMagnitude--;
                }
            }
            else
            {
                entity.DealDamage(1, gameState);
                break;
            }

        }
    }
    
    static Direction ReverseDirection(Direction inputDirection)
    {
        switch (inputDirection)
        {
            case Direction.Up:
                return Direction.Down;
            case Direction.Right:
                return Direction.Left;
            case Direction.Down:
                return Direction.Up;
            case Direction.Left:
                return Direction.Right;
            default:
                Debug.LogError("Bad input to ReverseDirection method: " + inputDirection);
                return Direction.Up;
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
