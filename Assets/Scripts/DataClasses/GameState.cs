using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct GameState {
    public EntityData player;
    public List<EntityData> enemies;

    public GameState(EntityData _player, List<EntityData> _enemies)
    {
        player = _player;
        enemies = _enemies;
    }

    public GameState Duplicate()
    {
        return new GameState(player, enemies);
    }

    public EntityData GetOccupantOfCell(Vector2Int position)
    {
        if (player.Position == position)
        {
            return player;  
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            EntityData entity = enemies[i];

            if (entity.Position == position)
            {
                return entity;
            }
        }

        Debug.LogError("Occupant of cell not found.");
        return null;
    }
}
