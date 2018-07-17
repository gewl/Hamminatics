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
    Canvas canvas;

    Color invisible;
    Color translucent;
    Color opaque;

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

        translucent = new Color(1f, 1f, 1f, 0f);
        translucent = new Color(1f, 1f, 1f, 0.5f);
        opaque = new Color(1f, 1f, 1f, 1f);

        instance = this;
    }

    private void OnEnable()
    {
        GameStateDelegates.OnRoundEnded += RecalculateTileDistances;   
        GameStateDelegates.OnEntitySelected += DrawBoard_SelectedEntity;   
    }

    private void OnDisable()
    {
        GameStateDelegates.OnRoundEnded -= RecalculateTileDistances;   
        GameStateDelegates.OnEntitySelected -= DrawBoard_SelectedEntity;   
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

        for (int y = 0; y < boardWidth; y++)
        {
            for (int x = 0; x < boardWidth; x++)
            {
                boardCellImages[x, y].sprite = DataManager.GetTileSprite(currentBoard.Tiles[x, y].ID);
                if (DebuggingTileDistances)
                {
                    GameObject thisText = Instantiate(debugText, boardCells[x, y], false);
                    thisText.GetComponent<Text>().text = currentBoard.Tiles[x, y].DistanceFromPlayer.ToString();
                }
            }
        }
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
    void ClearBoard()
    {
        for (int yCounter = 0; yCounter < boardWidth; yCounter++)
        {
            for (int xCounter = 0; xCounter < boardWidth; xCounter++)
            {
                Image contentsImage = cellContentImages[xCounter, yCounter];
                contentsImage.sprite = null;
                contentsImage.color = invisible;
            }
        }
    }

    public void DrawBoard_Standard(GameState currentGameState, GameState projectedGameState, bool isResolvingTurn)
    {
        ClearBoard();

        // Draw future player position.
        DrawSpriteAtPosition(projectedGameState.player.EntitySprite, projectedGameState.player.Position, translucent);

        // Draw future enemy positions.
        for (int i = 0; i < projectedGameState.enemies.Count; i++)
        {
            EntityData entity = projectedGameState.enemies[i];
            if (entity != null)
            {
                DrawSpriteAtPosition(entity.EntitySprite, entity.Position, translucent);
            }
        }

        // Draw current player at position.
        DrawSpriteAtPosition(currentGameState.player.EntitySprite, currentGameState.player.Position, opaque);

        // Draw current enemies at positions.
        for (int i = 0; i < currentGameState.enemies.Count; i++)
        {
            EntityData entity = currentGameState.enemies[i];
            if (entity != null)
            {
                DrawSpriteAtPosition(entity.EntitySprite, entity.Position, opaque);
            }
        }

        // Draw cells to be attacked in next round.
        foreach (CompletedAction completedAction in projectedGameState.actionsCompletedLastRound)
        {
            HighlightDamageCell(completedAction.TargetTile.Position);
        }
    }

    void DrawBoard_SelectedEntity(EntityData selectedEntity, GameState currentGameState, GameState projectedGameState)
    {
        ClearBoard();

        // Draw all current non-selected entities translucent.
        if (currentGameState.player != selectedEntity)
        {
            DrawSpriteAtPosition(currentGameState.player.EntitySprite, currentGameState.player.Position, translucent);
        }

        for (int i = 0; i < currentGameState.enemies.Count; i++)
        {
            EntityData entity = currentGameState.enemies[i];
            if (entity != null && entity != selectedEntity)
            {
                DrawSpriteAtPosition(entity.EntitySprite, entity.Position, translucent);
            }
        }

        GameStateHelperFunctions
            .GetAllPositionsThroughWhichEntityWillMove(selectedEntity, currentGameState)
            .ForEach(position => DrawSpriteAtPosition(selectedEntity.EntitySprite, position, translucent));

        DrawSpriteAtPosition(selectedEntity.EntitySprite, selectedEntity.Position, opaque);
    }

    void DrawSpriteAtPosition(Sprite sprite, Vector2Int position, Color color)
    {
        Image positionCellImage = cellContentImages[position.x, position.y];
        positionCellImage.sprite = sprite;
        positionCellImage.color = color;
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

    public Vector2 GetCellPosition(Vector2Int position)
    {
        RectTransform cellRectTransform = boardCells[position.x, position.y].GetComponent<RectTransform>();

        return cellRectTransform.position;
    }

    public Vector2 GetCellEdgePosition(Vector2Int position, Direction edgeDirection)
    {
        RectTransform cellRectTransform = boardCells[position.x, position.y].GetComponent<RectTransform>();
        Vector3[] worldCorners = new Vector3[4];
        cellRectTransform.GetWorldCorners(worldCorners);

        float averageX = cellRectTransform.position.x;
        float averageY = cellRectTransform.position.y;

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
