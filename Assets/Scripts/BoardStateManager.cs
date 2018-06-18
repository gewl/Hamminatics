using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardStateManager : MonoBehaviour {

    SpaceContents[,] currentBoardState;

    BoardController boardController;
    int boardWidth;

    private void Awake()
    {
        boardController = GetComponentInChildren<BoardController>();
        boardWidth = boardController.BoardWidth;
        currentBoardState = new SpaceContents[boardWidth, boardWidth];

        currentBoardState[0, 3] = SpaceContents.Player;

        boardController.DrawBoard(currentBoardState);
    }
}
