using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectedGameState {

    public EntityData activeEntity;
    public ScenarioState scenarioState;
    public Action action;

    public Bump bump;
    public List<Vector2Int> attackedPositions;

    public ProjectedGameState(EntityData _activeEntity, ScenarioState _gameState, Action _action, Bump _bump = null)
    {
        activeEntity = _activeEntity;
        scenarioState = _gameState;
        bump = _bump;
        action = _action;
        attackedPositions = new List<Vector2Int>();
    }

    public void AddAttackedPosition(Vector2Int attackedPosition)
    {
        attackedPositions.Add(attackedPosition);
    }
}
