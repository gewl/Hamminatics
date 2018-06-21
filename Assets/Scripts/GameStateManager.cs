using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour {

    const string PLAYER_ID = "Player";

    CellContents[,] currentBoardState;

    ActionQueueController _actionQueueController;
    ActionQueueController actionQueueController
    {
        get
        {
            if (_actionQueueController == null)
            {
                _actionQueueController = GetComponent<ActionQueueController>();
            }

            return _actionQueueController;
        }
    }
    BoardController _boardController;
    BoardController boardController
    {
        get
        {
            if (_boardController == null)
            {
                _boardController = GetComponentInChildren<BoardController>();
            }

            return _boardController;
        }
    }
    EquippedCardsManager _equippedCardsManager;
    EquippedCardsManager equippedCardsManager
    {
        get
        {
            if (_equippedCardsManager == null)
            {
                _equippedCardsManager = GetComponentInChildren<EquippedCardsManager>();
            }

            return _equippedCardsManager;
        }
    }

    int boardWidth;

    EntityData[] entitiesOnBoard;
    EntityData Player { get { return entitiesOnBoard[0]; } }
    List<Vector2Int> potentialCardTargets;

    private void Awake()
    {
        potentialCardTargets = new List<Vector2Int>();
        boardWidth = boardController.BoardWidth;
        currentBoardState = new CellContents[boardWidth, boardWidth];

        entitiesOnBoard = new EntityData[5];
        entitiesOnBoard[0] = DataManager.GetEntityData(PLAYER_ID);
    }

    private void Start()
    {
        Player.Position = new Vector2Int(3, 3);
        GenerateBoardState();
        ResetBoard();
    }

    void GenerateBoardState()
    {
        currentBoardState = new CellContents[boardWidth, boardWidth];
        currentBoardState[Player.Position.x, Player.Position.y] = CellContents.Player;

        for (int i = 1; i < entitiesOnBoard.Length; i++)
        {
            EntityData entity = entitiesOnBoard[i];
            if (entity != null)
            {
                currentBoardState[entity.Position.x, entity.Position.y] = CellContents.Enemy;
            }
        }
    }

    public void HighlightPotentialCardTargets(CardData card)
    {
        ResetBoard();

        int cardRange = card.Range;

        int leftMaximumX = Player.Position.x - cardRange;
        int rightMaximumX = Player.Position.x + cardRange;
        int topMaximumY = Player.Position.y - cardRange;
        int bottomMaximumY = Player.Position.y + cardRange;

        int testX = leftMaximumX;
        int testY = topMaximumY;

        while (testX <= rightMaximumX)
        {
            Vector2Int testPosition = new Vector2Int(testX, Player.Position.y);

            AttemptToHighlightCell(testPosition);

            testX++;
        }

        while (testY <= bottomMaximumY)
        {
            Vector2Int testPosition = new Vector2Int(Player.Position.x, testY);

            AttemptToHighlightCell(testPosition);

            testY++;
        }
    }

    void AttemptToHighlightCell(Vector2Int position)
    {
        if (IsCellValid(position) && !IsCellOccupied(position))
        {
            boardController.HighlightCell(position);
            potentialCardTargets.Add(position);
        }
    }

    public void ResetBoard()
    {
        boardController.DrawBoard(entitiesOnBoard);
        potentialCardTargets.Clear();
    }

    public void RegisterCellInteraction(Vector2Int cellPosition)
    {
        if (potentialCardTargets.Contains(cellPosition))
        {
            //actionQueueController.AddNewAction(equippedCardsManager.GetSelectedCard(), )
        }
        else
        {
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
