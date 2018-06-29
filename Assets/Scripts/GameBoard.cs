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

    public Vector2Int Entrance { get; private set; }

    public GameBoard()
    {
        boardWidth = BoardController.BoardWidth;

        Tile[,] _tiles = new Tile[boardWidth, boardWidth];

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

        GenerateWalls();

        Entrance = GenerateEntrance(boardWidth);
    }

    Vector2Int GenerateEntrance(int boardWidth)
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

        return position;
    }

    void GenerateWalls()
    {
        System.Random rand = new System.Random();

        int numberOfWalls = rand.Next(minNumberOfWalls, maxNumberOfWalls);

        for (int i = 0; i < numberOfWalls; i++)
        {
            int x = rand.Next(0, boardWidth);
            int y = rand.Next(0, boardWidth);

            Tiles[x, y].RemoveRandomNeighbor();
        }
    }
}