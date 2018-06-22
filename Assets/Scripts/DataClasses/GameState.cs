using System.Collections.Generic;
using UnityEngine;

public struct GameState {
    public EntityData player;
    public List<EntityData> enemies;

    public GameState(EntityData _player, List<EntityData> _enemies)
    {
        player = _player;
        enemies = _enemies;
    }

}
