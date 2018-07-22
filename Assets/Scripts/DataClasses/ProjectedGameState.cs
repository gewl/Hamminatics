using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectedGameState {

    public EntityData activeEntity;
    public GameState gameState;
    public Action action;

    public Bump bump;
    public List<Vector2Int> attackedPositions;

    public ProjectedGameState(EntityData _activeEntity, GameState _gameState, Action _action, Bump _bump = null)
    {
        activeEntity = _activeEntity;
        gameState = _gameState;
        bump = _bump;
        action = _action;
        attackedPositions = new List<Vector2Int>();
    }

    public void AddAttackedPosition(Vector2Int attackedPosition)
    {
        attackedPositions.Add(attackedPosition);
    }
}
