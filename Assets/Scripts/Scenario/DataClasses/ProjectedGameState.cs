using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;  

public class ProjectedGameState {

    public EntityData activeEntity;
    public ScenarioState scenarioState;
    public Action action;

    public List<Bump> bumps;
    public List<Vector2Int> attackedPositions;

    private bool isDummyState = false;
    public bool IsDummyState { get { return isDummyState; } }

    public ProjectedGameState(ScenarioState _gameState)
    {
        activeEntity = null;
        scenarioState = _gameState;
        isDummyState = true;
    }

    public ProjectedGameState(EntityData _activeEntity, ScenarioState _gameState, Action _action, Bump bump)
    {
        activeEntity = _activeEntity;
        scenarioState = _gameState;
        bumps = new List<Bump>()
        {
            bump
        };
        action = _action;
        attackedPositions = new List<Vector2Int>();
    }

    public ProjectedGameState(EntityData _activeEntity, ScenarioState _gameState, Action _action, List<Bump> _bumps = null)
    {
        activeEntity = _activeEntity;
        scenarioState = _gameState;
        if (_bumps == null)
        {
            bumps = new List<Bump>();
        }
        else
        {
            bumps = _bumps;
        }
        action = _action;
        attackedPositions = new List<Vector2Int>();
    }

    public List<EntityData> GetMovedEntities()
    {
        return scenarioState.GetAllEntitiesWhere(e => {
            EntityData lastStateEntity = scenarioState.lastGameState.GetEntityWhere(ent => ent == e);
            return lastStateEntity != null && lastStateEntity.Position != e.Position;
            });
    }

    public void AddAttackedPosition(Vector2Int attackedPosition)
    {
        attackedPositions.Add(attackedPosition);
    }

    public void AddAttackedPositions(IEnumerable<Vector2Int> _attackedPositions)
    {
        attackedPositions.AddRange(_attackedPositions);
    }

    public bool IsEntityBumped(EntityData entity)
    {
        return bumps.Any(b => b.bumpedEntity == entity);
    }

    public bool DoesEntityBump(EntityData entity)
    {
        return bumps.Any(b => b.bumpingEntity == entity);
    }
}
