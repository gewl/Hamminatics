using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class BoardController : MonoBehaviour {
    GameStateManager gameStateManager;
    static BoardController instance;

    static int boardWidth = 5;
    public static int BoardWidth { get { return boardWidth; } }
    Transform[,] boardCells;
    Image[,] boardCellImages;
    Image[,] cellContentImages;

    ImageManager spriteManager;
    [SerializeField]
    ActionImageDrawer actionImageDrawer;
    Canvas canvas;

    [SerializeField]
    GameObject debugText;

    public GameBoard currentBoard { get; private set;  }
    public static GameBoard CurrentBoard { get { return instance.currentBoard; } }
    public bool DebuggingTileDistances = false;

    #region Lifecycle
    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        gameStateManager = GetComponentInParent<GameStateManager>();
        spriteManager = GetComponentInParent<ImageManager>();

        InitializeBoard();
        gameStateManager.InitializeGameState(currentBoard);

        instance = this;
    }

    private void OnEnable()
    {
        gameStateManager.OnRoundEnded += RecalculateTileDistances;   
    }

    private void OnDisable()
    {
        gameStateManager.OnRoundEnded -= RecalculateTileDistances;   
    }
    #endregion

    void InitializeBoard()
    {
        boardCells = new Transform[boardWidth, boardWidth];
        boardCellImages = new Image[boardWidth, boardWidth];
        cellContentImages = new Image[boardWidth, boardWidth];

        int childCount = transform.childCount;

        int xCounter = 0, yCounter = 0; 

        for (int i = 0; i < childCount; i++)
        {
            Transform cell = transform.GetChild(i);
            boardCells[xCounter, yCounter] = cell;

            boardCellImages[xCounter, yCounter] = cell.GetComponent<Image>();

            Image cellContents = cell.GetChild(0).GetComponent<Image>();
            cellContentImages[xCounter, yCounter] = cellContents;
            cellContents.GetComponent<Button>().onClick.AddListener(GenerateCellClickListener(xCounter, yCounter));

            xCounter++;

            if (xCounter >= boardWidth)
            {
                yCounter++;
                xCounter = 0;
            }
        }

        currentBoard = new GameBoard();

#if UNITY_EDITOR
        if (DebuggingTileDistances)
        {
            for (int y = 0; y < boardWidth; y++)
            {
                for (int x = 0; x < boardWidth; x++)
                {
                    boardCellImages[x, y].sprite = DataManager.GetTileSprite(currentBoard.Tiles[x, y].ID);
                    GameObject thisText = Instantiate(debugText, boardCells[x,y], false);
                    thisText.GetComponent<Text>().text = currentBoard.Tiles[x, y].DistanceFromPlayer.ToString();
                }
            }
        }
#endif
    }

    void RecalculateTileDistances(GameState updatedGameState)
    {
        Vector2Int playerPosition = updatedGameState.player.Position;

        Tile playerTile = currentBoard.Tiles[playerPosition.x, playerPosition.y];

        currentBoard.ProcessTileDistancesToPlayer(playerTile);

#if UNITY_EDITOR
        if (DebuggingTileDistances)
        {
            for (int y = 0; y < boardWidth; y++)
            {
                for (int x = 0; x < boardWidth; x++)
                {
                    Text thisText = boardCellImages[x, y].GetComponentInChildren<Text>();
                    thisText.text = currentBoard.Tiles[x, y].DistanceFromPlayer.ToString();
                }
            }
        }
#endif
    }

    #region GUI manipulation
    public void DrawBoard(GameState currentGameState, GameState projectedGameState)
    {
        // Clear board.
        for (int yCounter = 0; yCounter < boardWidth; yCounter++)
        {
            for (int xCounter = 0; xCounter < boardWidth; xCounter++)
            {
                Image contentsImage = cellContentImages[xCounter, yCounter];
                contentsImage.sprite = null;
                contentsImage.color = new Color(1f, 1f, 1f, 0f);
            }
        }

        // Draw future player position.
        Vector2Int projectedPlayerPosition = projectedGameState.player.Position;
        Image projectedPlayerCellImage = cellContentImages[projectedPlayerPosition.x, projectedPlayerPosition.y];
        projectedPlayerCellImage.sprite = currentGameState.player.EntitySprite;
        projectedPlayerCellImage.color = new Color(1f, 1f, 1f, 0.5f);

        // Draw future enemy positions.
        for (int i = 0; i < projectedGameState.enemies.Count; i++)
        {
            EntityData entity = projectedGameState.enemies[i];
            if (entity != null)
            {
                Image contentsImage = cellContentImages[entity.Position.x, entity.Position.y];
                contentsImage.sprite = entity.EntitySprite;
                contentsImage.color = new Color(1f, 1f, 1f, 0.5f);
            }
        }

        // Draw current player position.
        Vector2Int playerPosition = currentGameState.player.Position;
        Image playerCellImage = cellContentImages[playerPosition.x, playerPosition.y];
        playerCellImage.sprite = currentGameState.player.EntitySprite;
        playerCellImage.color = new Color(1f, 1f, 1f, 1f);

        // Draw current enemy positions.
        for (int i = 0; i < currentGameState.enemies.Count; i++)
        {
            EntityData entity = currentGameState.enemies[i];
            if (entity != null)
            {
                Image contentsImage = cellContentImages[entity.Position.x, entity.Position.y];
                contentsImage.sprite = entity.EntitySprite;
                contentsImage.color = new Color(1f, 1f, 1f, 1f);
            }
        }

        // Draw cells to be attacked in next round.
        foreach (CompletedAction completedAction in projectedGameState.actionsCompletedLastRound)
        {
            HighlightDamageCell(completedAction.TargetTile.Position);
        }

        actionImageDrawer.Draw(projectedGameState.movesCompletedLastRound, projectedGameState.actionsCompletedLastRound);
    }

    public void HighlightSelectedCell(Vector2Int position)
    {
        cellContentImages[position.x, position.y].color = new Color(1f, 1f, 0f);
    }

    public void HighlightDamageCell(Vector2Int position)
    {
        Image cellImage = cellContentImages[position.x, position.y];
        // If cell is already opaque (because e.g. it contains an entity), it remains opaque.
        float damageCellAlpha = Mathf.Max(cellImage.color.a, 0.5f);
        cellImage.color = new Color(.8f, 0f, 0f, damageCellAlpha);
    }

    UnityAction GenerateCellClickListener(int x, int y)
    {
        Vector2Int cellPosition = new Vector2Int(x, y);

        return () =>
        {
            gameStateManager.RegisterCellClick(cellPosition);
        };
    }

    #endregion

    public Vector2 GetCellEdgePosition(Vector2Int position, Direction edgeDirection)
    {
        RectTransform cellRectTransform = boardCells[position.x, position.y].GetComponent<RectTransform>();
        Vector3[] worldCorners = new Vector3[4];
        cellRectTransform.GetWorldCorners(worldCorners);

        float averageX = (worldCorners[2].x + worldCorners[1].x) / 2f;
        float averageY = (worldCorners[3].y + worldCorners[2].y) / 2f;

        switch (edgeDirection)
        {
            case Direction.Up:
                return new Vector2(averageX, worldCorners[1].y);
            case Direction.Down:
                return new Vector2(averageX, worldCorners[0].y);
            case Direction.Left:
                return new Vector2(worldCorners[0].x, averageY);
            case Direction.Right:
                return new Vector2(worldCorners[2].x, averageY);
            default:
                return new Vector2(averageX, averageY);
        }
    }

    #region DEBUG ONLY
    public static void TurnTileColor(Tile tile, Color color)
    {
        instance.boardCellImages[tile.Position.x, tile.Position.y].color = color;
    }

    #endregion
}
