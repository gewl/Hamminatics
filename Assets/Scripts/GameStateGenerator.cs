using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateGenerator {

    const string PLAYER_ID = "Player";
    const string SQUID_ID = "Squid";
    const string WASP_ID = "Wasp";

    public static GameState GenerateNewGameState()
    {
        EntityData player = DataManager.GetEntityData(PLAYER_ID);
        player.Position = new Vector2Int(3, 3);

        EntityData squid = DataManager.GetEntityData(SQUID_ID);
        EntityData squid2 = DataManager.GetEntityData(SQUID_ID);
        EntityData wasp = DataManager.GetEntityData(WASP_ID);
        squid.Position = new Vector2Int(2, 2);
        squid2.Position = new Vector2Int(2, 0);
        wasp.Position = new Vector2Int(4, 4);
        List<EntityData> enemies = new List<EntityData>()
        {
            squid,
            squid2,
            wasp
        };

        SpeedComparer comparer = new SpeedComparer();

        enemies.Sort(comparer);

        return new GameState(player, enemies);
    }
}

public class SpeedComparer : IComparer<EntityData>
{
    public int Compare(EntityData entity1, EntityData entity2)
    {
        return entity1.Speed > entity2.Speed ? 1 : -1;
    }
}