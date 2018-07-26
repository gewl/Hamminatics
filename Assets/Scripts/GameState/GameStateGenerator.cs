﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameStateGenerator {

    static System.Random rnd;
    private static readonly object syncLock = new object();

    const string PLAYER_ID = "Player";
    const string SQUID_ID = "Squid";
    const string WASP_ID = "Wasp";

    const string SPIKE_TRAP_ID = "SimpleSpikeTrap";

    public static GameState GenerateNewGameState(Vector2Int entrance, int boardWidth)
    {
        EntityData player = DataManager.GetEntityData(PLAYER_ID);

        //EntityData squid = DataManager.GetEntityData(SQUID_ID);
        //EntityData squid2 = DataManager.GetEntityData(SQUID_ID);
        EntityData wasp = DataManager.GetEntityData(WASP_ID);
        List<EntityData> enemies = new List<EntityData>()
        {
            //squid,
            //squid2,
            //wasp
        };

        TrapData spikeTrap = DataManager.GetTrapData(SPIKE_TRAP_ID);

        List<ItemData> items = new List<ItemData>()
        {
            spikeTrap
        };

        SpeedComparer comparer = new SpeedComparer();
        enemies.Sort(comparer);

        GameState generatedState = new GameState(player, enemies, items);
        player.SetPosition(entrance, generatedState);
        generatedState = RandomizeEntityStartingCoordinates(generatedState, enemies, boardWidth, player);
        generatedState = RandomizeItemStartingCoordinates(generatedState, items, boardWidth);

        return generatedState;
    }

    static GameState RandomizeEntityStartingCoordinates(GameState state, List<EntityData> entities, int boardWidth, EntityData player)
    {
        for (int i = 0; i < entities.Count; i++)
        {
            Vector2Int newPosition = GenerateRandomVector2IntInBounds(boardWidth);

            while (player.Position == newPosition || entities.Any<EntityData>(entity => entity.Position == newPosition))
            {
                newPosition = GenerateRandomVector2IntInBounds(boardWidth);
            }

            entities[i].SetPosition(newPosition, state);
        }

        return state;
    }

    static GameState RandomizeItemStartingCoordinates(GameState state, List<ItemData> items, int boardWidth)
    {
        for (int i = 0; i < items.Count; i++)
        {
            ItemData item = items[i];

            Vector2Int newPosition = GenerateRandomVector2IntInBounds(boardWidth);
            while (state.IsTileOccupied(newPosition) || state.DoesPositionContainItem(newPosition))
            {
                newPosition = GenerateRandomVector2IntInBounds(boardWidth);
            }

            item.Position = newPosition;
        }

        return state;
    }

    static Vector2Int GenerateRandomVector2IntInBounds(int maxValue)
    {
        if (rnd == null)
        {
            rnd = new System.Random();
        }

        lock(syncLock)
        {
            Vector2Int results = new Vector2Int(rnd.Next(0, maxValue - 1), rnd.Next(0, maxValue - 1));

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