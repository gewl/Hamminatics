using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard {

    public int Width { get; private set; }

    int minNumberOfWalls = 8;
    int maxNumberOfWalls = 12;

    public Tile[,] Tiles { get; private set; }

    public Tile Entrance { get; private set; }
    public Tile Exit { get; private set; } 
    List<Wall> walls;

    public GameBoard(bool generateExit = true)
    {
        Width = BoardController.BoardWidth;

        Tile[,] _tiles = new Tile[Width, Width];
        walls = new List<Wall>();

        for (int y = 0; y < Width; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                _tiles[x, y] = new Tile(x, y);
            }
        }

        Tiles = _tiles;

        for (int y = 0; y < Width; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Tile tile = Tiles[x, y];

                if (x < Width - 1)
                {
                    Tile rightNeighbor = Tiles[x + 1, y];
                    tile.AddNeighbor(rightNeighbor);
                }
                if (y < Width - 1)
                {
                    Tile bottomNeighbor = Tiles[x, y + 1];
                    tile.AddNeighbor(bottomNeighbor);
                }
            }
        }

        Entrance = GenerateEntrance(Width);
        if (generateExit)
        {
            Exit = GenerateExit(Width, Entrance.Position);
        }

        GenerateWalls();

        ProcessTileDistancesToPlayer(Entrance);

        while (AnyTilesNotReachable())
        {
            for (int i = 0; i < walls.Count; i++)
            {
                walls[i].DestroyWall();
            }
            walls.Clear();

            GenerateWalls();

            Entrance = GenerateEntrance(Width);

            ProcessTileDistancesToPlayer(Entrance);
        }
    }

    bool AnyTilesNotReachable()
    {
        return Tiles.Cast<Tile>().Any<Tile>(tile => tile.VisitedByPathfinding == false);
    }

    void GenerateWalls()
    {
        System.Random rand = new System.Random();

        int numberOfWalls = rand.Next(minNumberOfWalls, maxNumberOfWalls);

        for (int i = 0; i < numberOfWalls; i++)
        {
            int x = rand.Next(0, Width);
            int y = rand.Next(0, Width);

            Tile neighborOne = Tiles[x, y];
            Tile neighborTwo = neighborOne.GetRandomNeighbor(true);

            if (neighborTwo == null || neighborOne == Entrance || neighborTwo == Entrance)
            {
                continue;
            }
            walls.Add(new Wall(neighborOne, neighborTwo));
        }
    }

    Tile GenerateEntrance(int boardWidth)
    {
        Vector2Int position = new Vector2Int(-1, -1);

        System.Random random = new System.Random();
        int roomSide = random.Next(0, 4);

        switch (roomSide)
        {
            case 0:
                position[1] = 0;
                break;
            case 1:
                position[0] = boardWidth - 1;
                break;
            case 2:
                position[1] = boardWidth - 1;
                break;
            case 3:
                position[0] = 0;
                break;
            default:
                break;
        }

        int otherValue = random.Next(1, boardWidth - 2);

        if (position[0] == -1)
        {
            position[0] = otherValue;
        }
        else
        {
            position[1] = otherValue;
        }

        Tile entranceTile = Tiles[position.x, position.y];

        return entranceTile;
    }

    Tile GenerateExit(int boardWidth, Vector2Int entrance)
    {
        Vector2Int position = new Vector2Int(-1, -1);

        // Place exit position against wall opposite entrance wall. 
        if (entrance.x == 0)
        {
            position.x = boardWidth - 1;
        }
        else if (entrance.x == boardWidth - 1)
        {
            position.x = 0;
        }
        else if (entrance.y == 0)
        {
            position.y = boardWidth - 1;
        }
        else if (entrance.y == boardWidth - 1)
        {
            position.y = 0;
        }

        System.Random random = new System.Random();
        int otherValue = random.Next(1, boardWidth - 2);

        if (position.x == -1)
        {
            position.x = otherValue;
        }
        else
        {
            position.y = otherValue;
        }

        Tile exitTile = Tiles[position.x, position.y];

        return exitTile;
    }

    public void ProcessTileDistancesToPlayer(Tile playerTile, bool initializeTileDistances = true)
    {
        if (initializeTileDistances)
        {
            for (int y = 0; y < Width; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Tiles[x, y].DistanceFromPlayer = int.MaxValue;
                    Tiles[x, y].VisitedByPathfinding = false;
                }
            }
        }

        playerTile.DistanceFromPlayer = 0;
        playerTile.VisitedByPathfinding = true;

        Queue<Tile> tilesToProcess = new Queue<Tile>();

        for (int i = 0; i < playerTile.Neighbors.Count; i++)
        {
            Tile playerNeighbor = playerTile.Neighbors[i];
            tilesToProcess.Enqueue(playerNeighbor);
            playerNeighbor.VisitedByPathfinding = true;
        }

        while (tilesToProcess.Count > 0)
        {
            Tile nextTile = tilesToProcess.Dequeue();
            List<Tile> nextTileNeighbors = nextTile.Neighbors;

            int closestNeighborDistance = int.MaxValue;

            for (int i = 0; i < nextTileNeighbors.Count; i++)
            {
                Tile thisNeighbor = nextTileNeighbors[i];

                if (!thisNeighbor.VisitedByPathfinding)
                {
                    thisNeighbor.VisitedByPathfinding = true;
                    tilesToProcess.Enqueue(thisNeighbor);
                }
                else if (thisNeighbor.DistanceFromPlayer < closestNeighborDistance)
                {
                    closestNeighborDistance = thisNeighbor.DistanceFromPlayer;
                }
            }

            nextTile.DistanceFromPlayer = closestNeighborDistance + 1;
        }
    }
}

class Wall
{
    Tile neighborOne;
    Tile neighborTwo;

    public Wall(Tile _neighborOne, Tile _neighborTwo)
    {
        neighborOne = _neighborOne;
        neighborTwo = _neighborTwo;

        neighborOne.RemoveNeighbor(neighborTwo);
    }

    public void DestroyWall()
    {
        neighborOne.AddNeighbor(neighborTwo);
    }
}