using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard {

    int minNumberOfWalls = 8;
    int maxNumberOfWalls = 12;

    public Tile[,] Tiles { get; private set; }

    public Vector2Int Entrance { get; private set; }

    public GameBoard()
    {
        int boardWidth = BoardController.BoardWidth;

        Tile[,] _tiles = new Tile[boardWidth, boardWidth];

        for (int y = 0; y < boardWidth; y++)
        {
            for (int x = 0; x < boardWidth; x++)
            {
                _tiles[x, y] = new Tile(x, y);
            }
        }

        Tiles = _tiles;

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
    }
}

public struct Tile
{
    bool[] openPathways;
    public string ID { get; private set; }

    public Tile(int x, int y)
    {
        openPathways = new bool[4]
        {
            true,
            true,
            true,
            true
        };

        if (x == 0)
        {
            openPathways[3] = false;
        }
        else if (x == BoardController.BoardWidth - 1)
        {
            openPathways[1] = false;
        }

        if (y == 0)
        {
            openPathways[0] = false;
        }
        else if (y == BoardController.BoardWidth - 1)
        {
            openPathways[2] = false;
        }

        string generatedID = "";

        for (int i = 0; i < openPathways.Length; i++)
        {
            if (openPathways[i])
            {
                generatedID += "1";
            }
            else
            {
                generatedID += "0";
            }
        }

        ID = generatedID;
    }

}
