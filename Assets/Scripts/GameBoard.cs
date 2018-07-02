using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard {

    int boardWidth;

    int minNumberOfWalls = 8;
    int maxNumberOfWalls = 12;

    public Tile[,] Tiles { get; private set; }

    public Tile Entrance { get; private set; }
    List<Wall> walls;

    public GameBoard()
    {
        boardWidth = BoardController.BoardWidth;

        Tile[,] _tiles = new Tile[boardWidth, boardWidth];
        walls = new List<Wall>();

        for (int y = 0; y < boardWidth; y++)
        {
            for (int x = 0; x < boardWidth; x++)
            {
                _tiles[x, y] = new Tile(x, y);
            }
        }

        Tiles = _tiles;

        for (int y = 0; y < boardWidth; y++)
        {
            for (int x = 0; x < boardWidth; x++)
            {
                Tile tile = Tiles[x, y];

                if (x < boardWidth - 1)
                {
                    Tile rightNeighbor = Tiles[x + 1, y];
                    tile.AddNeighbor(rightNeighbor);
                }
                if (y < boardWidth - 1)
                {
                    Tile bottomNeighbor = Tiles[x, y + 1];
                    tile.AddNeighbor(bottomNeighbor);
                }
            }
        }

        Entrance = GenerateEntrance(boardWidth);

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

            Entrance = GenerateEntrance(boardWidth);

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
            int x = rand.Next(0, boardWidth);
            int y = rand.Next(0, boardWidth);

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

        int otherValue = random.Next(0, boardWidth - 1);

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

    public void ProcessTileDistancesToPlayer(Tile playerTile, bool initializeTileDistances = true)
    {
        if (initializeTileDistances)
        {
            for (int y = 0; y < boardWidth; y++)
            {
                for (int x = 0; x < boardWidth; x++)
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

    public Tile GetTileAt(Vector2Int position)
    {
        return GetTileAt(position.x, position.y);
    }

    public Tile GetTileAt(int x, int y)
    {
        return Tiles[x, y];
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