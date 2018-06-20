using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardStateManager : MonoBehaviour {

    CellContents[,] currentBoardState;

    BoardController boardController;
    int boardWidth;

    Vector2Int playerPosition;
    public Vector2Int PlayerPosition
    {
        get { return playerPosition; }
    }

    private void Awake()
    {
        boardController = GetComponentInChildren<BoardController>();
        boardWidth = boardController.BoardWidth;
        currentBoardState = new CellContents[boardWidth, boardWidth];

        playerPosition = new Vector2Int(0, 3);
    }

    private void Start()
    {
        currentBoardState[playerPosition.x, playerPosition.y] = CellContents.Player;
        boardController.DrawBoard(currentBoardState);
    }

    public void HighlightCardMoves(CardData card)
    {
        boardController.DrawBoard(currentBoardState);

        int cardRange = card.Range;

        int leftMaximumX = playerPosition.x - cardRange;
        int rightMaximumX = playerPosition.x + cardRange;
        int topMaximumY = playerPosition.y - cardRange;
        int bottomMaximumY = playerPosition.y + cardRange;

        int testX = leftMaximumX;
        int testY = topMaximumY;

        while (testX <= rightMaximumX)
        {
            Vector2Int testPosition = new Vector2Int(testX, playerPosition.y);
            if (IsCellValid(testPosition) && !IsCellOccupied(testPosition))
            {
                boardController.HighlightCell(testPosition);
            }

            testX++;
        }

        while (testY <= bottomMaximumY)
        {
            Vector2Int testPosition = new Vector2Int(playerPosition.x, testY);
            if (IsCellValid(testPosition) && !IsCellOccupied(testPosition))
            {
                boardController.HighlightCell(testPosition);
            }

            testY++;
        }
    }

    #region Cell helper functions
    public bool IsCellValid(Vector2Int position)
    {
        return position.x >= 0 && position.x < boardWidth && position.y >= 0 && position.y < boardWidth;
    }

    public bool IsCellOccupied(Vector2Int position)
    {
        return currentBoardState[position.x, position.y] != CellContents.None;
    }
    #endregion
}
