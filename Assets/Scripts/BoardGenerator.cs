using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour {

    public static Tile[,] GenerateBoard()
    {
        int boardWidth = BoardController.BoardWidth;

        Tile[,] newBoard = new Tile[boardWidth, boardWidth];

        for (int y = 0; y < boardWidth; y++)
        {
            for (int x = 0; x < boardWidth; x++)
            {
                newBoard[x, y] = new Tile(x, y);
            }
        }

        return newBoard;
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
