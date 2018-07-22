using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectedGameState {

    public EntityData activeEntity;
    public GameState gameState;
    public CardCategory cardType;

    public Bump bump;
    public List<Vector2Int> attackedPositions;

    public ProjectedGameState(EntityData _activeEntity, GameState _gameState, CardCategory _cardType, Bump _bump = null)
    {
        activeEntity = _activeEntity;
        gameState = _gameState;
        bump = _bump;
        cardType = _cardType;
        attackedPositions = new List<Vector2Int>();
    }

    public void AddAttackedPosition(Vector2Int attackedPosition)
    {
        attackedPositions.Add(attackedPosition);
    }
}
