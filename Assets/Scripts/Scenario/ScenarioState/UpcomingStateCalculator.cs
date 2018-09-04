using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Predicates;

public static class UpcomingStateCalculator
{
    static CardData GenericMovementCard { get { return DataRetriever.GetGenericMovementCard(); } }

    public static List<ProjectedGameState> CalculateUpcomingStates(ScenarioState currentState, GameBoard board)
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
                continue;
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

        ScenarioState lastCalculatedState = projectedGameStates.Last().scenarioState;

        lastCalculatedState.UpdateStagnation(board);
        lastCalculatedState.stagnatedPositions.ForEach(p =>
        {
            EntityData stagnatedEntity = lastCalculatedState.GetTileOccupant(p);
            if (stagnatedEntity != null)
            {
                stagnatedEntity.DealDamage(1, lastCalculatedState);
            }
        });

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
                entity.ChangeHealthValue_Campaign(modifier.value);
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
            case CardCategory.Self:
                return GetNextStateFromAction_Self(newState,
                    entityCopy,
                    action);
            default:
                return GetNextGameStateFromMove(newState, entityCopy, action.direction);
        }
    }

    #region Attack action resolution
    static ProjectedGameState GetNextStateFromAction_Attack(ScenarioState newState, EntityData entity, Action action)
    {
        GameBoard testBoard = BoardController.CurrentBoard;
        AttackCardData card = action.card as AttackCardData;
        TargetType attackTargetType = card.targetType;

        Tile attackOriginTile = BoardController.CurrentBoard.GetTileAtPosition(entity.Position);

        List<Tile> targetTiles = new List<Tile>();

        switch (attackTargetType)
        {
            case TargetType.Single:
                targetTiles.Add(newState.FindFirstOccupiedTileInDirection(attackOriginTile, action.direction, action.distance));
                break;
            case TargetType.AreaOfEffect:
                Tile impactTile = newState.FindFirstOccupiedTileInDirection(attackOriginTile, action.direction, action.distance);
                targetTiles.Add(impactTile);
                targetTiles.AddRange(impactTile.Neighbors);
                break;
            case TargetType.Line:
                targetTiles.AddRange(attackOriginTile.GetAllTilesInDirection(action.direction, action.card.range));
                break;
            default:
                break;
        }

        ProjectedGameState newProjectedState = new ProjectedGameState(entity, newState, action);

        newProjectedState.AddAttackedPositions(targetTiles.Select(t => t.Position));

        List<EntityData> affectedEntities = targetTiles
            .Select(t => newState.GetTileOccupant(t))
            .Where(o => o != null)
            .ToList();
        if (affectedEntities.Count == 0)
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

        for (int i = 0; i < affectedEntities.Count; i++)
        {
            EntityData affectedEntity = affectedEntities[i];

            int cardDamage = card.damage + entity.GetAttackModifierValue();
            affectedEntity.DealDamage(cardDamage, newState);

            List<ModifierData> attackModifiers = action.card.modifiers;
            if (attackModifiers != null && attackModifiers.Count > 0)
            {
                for (int j = 0; i < attackModifiers.Count; i++)
                {
                    ApplyModifierToAttack(affectedEntity, attackModifiers[j], entity, newState, action.direction);
                }
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
            forceDirection = ScenarioStateHelperFunctions.ReverseDirection(forceDirection);
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
    
    #endregion

    static ProjectedGameState GetNextStateFromAction_Self(ScenarioState newState, EntityData entity, Action action)
    {
        SelfCardData card = action.card as SelfCardData;

        ProjectedGameState newProjectedState = new ProjectedGameState(entity, newState, action);

        entity.ChangeHealthValue_Scenario(card.healthChange, newState);

        card.modifiers.ForEach(m => entity.activeModifiers.Add(Object.Instantiate(m)));

        return newProjectedState;
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
