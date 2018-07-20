using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectedGameState {

    public EntityData activeEntity;
    public GameState gameState;

    public Bump bump; 

    public ProjectedGameState(EntityData _activeEntity, GameState _gameState, Bump _bump = null)
    {
        activeEntity = _activeEntity;
        gameState = _gameState;
        bump = _bump;
    }
}
