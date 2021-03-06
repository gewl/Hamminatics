﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScenarioStateGenerator {

    static System.Random rnd;
    private static readonly object syncLock = new object();

    const string PLAYER_ID = "Player";

    public static ScenarioState GenerateNewScenarioState(GameBoard board, EnemySpawnGroupData enemySpawnGroup, bool isBossScenario = false)
    {
        List<EntityData> enemies = enemySpawnGroup.EnemiesToSpawn.Select(e => ScriptableObject.Instantiate(e)).ToList();

        List<ItemData> items = enemySpawnGroup.ItemsToSpawn.Select(t => ScriptableObject.Instantiate(t)).ToList();

        SpeedComparer comparer = new SpeedComparer();
        enemies.Sort(comparer);

        Direction stagnationDirection = GetStagnationDirectionFromEntrance(board.Entrance.Position, board.Width);

        ScenarioState generatedState = new ScenarioState(enemies, items, enemySpawnGroup.scenarioReward, stagnationDirection, isBossScenario);
        EntityData player = generatedState.player;
        player.SetPosition(board.Entrance.Position, generatedState);
        generatedState = RandomizeEntityStartingCoordinates(generatedState, enemies, board.Width, board.GetTileAtPosition(player.Position), board);

        Vector2Int exitPosition = board.Exit == null ? Vector2Int.zero : board.Exit.Position;
        generatedState = RandomizeItemStartingCoordinates(generatedState, items, exitPosition, board.Width);

        return generatedState;
    }

    static Direction GetStagnationDirectionFromEntrance(Vector2Int entrancePosition, int boardWidth)
    {

        if (entrancePosition.x == 0)
        {
            return Direction.Right;
        }
        else if (entrancePosition.x == boardWidth - 1)
        {
            return Direction.Left;
        }
        else if (entrancePosition.y == 0)
        {
            return Direction.Down;
        }
        else
        {
            return Direction.Up;
        }
    }

    static ScenarioState RandomizeEntityStartingCoordinates(ScenarioState state, List<EntityData> entities, int boardWidth, Tile playerTile, GameBoard board)
    {
        for (int i = 0; i < entities.Count; i++)
        {
            EntityData entity = entities[i];
            int entityRange = entity.attackCard.range;
            int minimumRange = entityRange + 1;
            int maximumRange = entityRange + 3;

            Tile entityTile = board.GetTileAtPosition(entity.Position);

            // Upper bound is exclusive.
            int spawnRange = GenerateSpawnRange(minimumRange, maximumRange + 1);
            List<Tile> possibleSpawnTiles = playerTile.GetAllTilesAtDistance(spawnRange);

            Tile spawnTile = possibleSpawnTiles.GetAndRemoveRandomElement();

            while (playerTile == spawnTile || entities.Any(e => e.Position == spawnTile.Position))
            {
                if (possibleSpawnTiles.Count == 0)
                {
                    spawnRange++;
                    possibleSpawnTiles = playerTile.GetAllTilesAtDistance(spawnRange);
                }
                spawnTile = playerTile.GetAllTilesAtDistance(spawnRange).GetRandomElement();
            }

            entities[i].SetPosition(spawnTile.Position, state);
        }

        return state;
    }

    static ScenarioState RandomizeItemStartingCoordinates(ScenarioState state, List<ItemData> items, Vector2Int exit, int boardWidth)
    {
        for (int i = 0; i < items.Count; i++)
        {
            ItemData item = items[i];

            Vector2Int newPosition = GenerateRandomVector2IntInBounds(boardWidth);
            while (state.IsTileOccupied(newPosition) || state.DoesPositionContainItem(newPosition) || exit == newPosition)
            {
                newPosition = GenerateRandomVector2IntInBounds(boardWidth);
            }

            item.Position = newPosition;
        }

        return state;
    }

    static int GenerateSpawnRange(int minimumRange, int maximumRange)
    {
        if (rnd == null)
        {
            rnd = new System.Random();
        }

        lock (syncLock)
        {
            return rnd.Next(minimumRange, maximumRange);
        }
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