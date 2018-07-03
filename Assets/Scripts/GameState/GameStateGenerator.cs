using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateGenerator {

    static System.Random rnd;
    private static readonly object syncLock = new object();

    const string PLAYER_ID = "Player";
    const string SQUID_ID = "Squid";
    const string WASP_ID = "Wasp";

    public static GameState GenerateNewGameState(Vector2Int entrance, int boardWidth)
    {
        EntityData player = DataManager.GetEntityData(PLAYER_ID);
        player.Position = entrance;

        EntityData squid = DataManager.GetEntityData(SQUID_ID);
        EntityData squid2 = DataManager.GetEntityData(SQUID_ID);
        EntityData wasp = DataManager.GetEntityData(WASP_ID);
        List<EntityData> enemies = new List<EntityData>()
        {
            squid,
            squid2,
            wasp
        };

        squid.Position = GenerateRandomStartingCoordinates(boardWidth);
        squid2.Position = GenerateRandomStartingCoordinates(boardWidth);
        wasp.Position = GenerateRandomStartingCoordinates(boardWidth);

        SpeedComparer comparer = new SpeedComparer();

        enemies.Sort(comparer);

        return new GameState(player, enemies);
    }

    static Vector2Int GenerateRandomStartingCoordinates(int boardWidth)
    {
        if (rnd == null)
        {
            rnd = new System.Random();
        }

        lock(syncLock)
        {
            Vector2Int results = new Vector2Int(rnd.Next(0, boardWidth - 1), rnd.Next(0, boardWidth - 1));
            return results;
        }
    }
}

public class SpeedComparer : IComparer<EntityData>
{
    public int Compare(EntityData entity1, EntityData entity2)
    {
        return entity1.Speed > entity2.Speed ? 1 : -1;
    }
}