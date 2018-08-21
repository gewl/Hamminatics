using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[ExecuteInEditMode]
public class BoardController : MonoBehaviour {
    ScenarioStateManager gameStateManager;
    static BoardController instance;

    bool boardInitialized = false;
    static int boardWidth = 5;
    public static int BoardWidth { get { return boardWidth; } }
    Transform[,] boardTiles;
    Image[,] boardTileImages;
    Image[,] tileOccupantImages;
    Image[,] tileItemImages;

    Color invisible;
    Color translucent;
    Color opaque;

    [SerializeField]
    GameObject debugText;

    public GameBoard currentBoard { get; private set;  }
    public static GameBoard CurrentBoard { get { return instance.currentBoard; } }
    public bool debuggingTileDistances = false;
    public bool debuggingExitPosition = false;
    [SerializeField]
    GameObject debugExitTextPrefab;
    GameObject instantiatedExitText;

    #region Lifecycle
    private void Awake()
    {
        gameStateManager = GetComponentInParent<ScenarioStateManager>();

        invisible = new Color(1f, 1f, 1f, 0f);
        translucent = new Color(1f, 1f, 1f, 0.5f);
        opaque = new Color(1f, 1f, 1f, 1f);

        instance = this;
    }

    private void OnEnable()
    {
        GameStateDelegates.OnRoundEnded += RecalculateTileDistances;   
        //GameStateDelegates.OnEntitySelected += DrawBoard_SelectedEntity;   
    }

    private void OnDisable()
    {
        GameStateDelegates.OnRoundEnded -= RecalculateTileDistances;   
        //GameStateDelegates.OnEntitySelected -= DrawBoard_SelectedEntity;   
    }
    #endregion

    void InitializeBoard()
    {
        boardTiles = new Transform[boardWidth, boardWidth];
        boardTileImages = new Image[boardWidth, boardWidth];
        tileOccupantImages = new Image[boardWidth, boardWidth];
        tileItemImages = new Image[boardWidth, boardWidth];

        int childCount = transform.childCount;

        int xCounter = 0, yCounter = 0; 

        for (int i = 0; i < childCount; i++)
        {
            Transform cell = transform.GetChild(i);
            boardTiles[xCounter, yCounter] = cell;

            boardTileImages[xCounter, yCounter] = cell.GetComponent<Image>();

            Image tileOccupant = cell.GetChild(0).GetComponent<Image>();
            tileOccupantImages[xCounter, yCounter] = tileOccupant;
            tileOccupant.GetComponent<Button>().onClick.AddListener(GenerateCellClickListener(xCounter, yCounter));

            Image tileItem = cell.GetChild(1).GetComponent<Image>();
            tileItemImages[xCounter, yCounter] = tileItem;

            xCounter++;

            if (xCounter >= boardWidth)
            {
                yCounter++;
                xCounter = 0;
            }
        }
        boardInitialized = true;
    }

    public GameBoard GenerateBoard()
    {
        if (!boardInitialized)
        {
            InitializeBoard();
        }
        currentBoard = new GameBoard();

        for (int y = 0; y < boardWidth; y++)
        {
            for (int x = 0; x < boardWidth; x++)
            {
                boardTileImages[x, y].sprite = DataRetriever.GetTileSprite(currentBoard.Tiles[x, y].ID);

                if (debuggingTileDistances)
                {
                    GameObject thisText = Instantiate(debugText, boardTiles[x, y], false);
                    thisText.GetComponent<Text>().text = currentBoard.Tiles[x, y].DistanceFromPlayer.ToString();
                }
            }
        }

        if (debuggingExitPosition)
        {
            if (instantiatedExitText != null)
            {
                Destroy(instantiatedExitText);
            }
            instantiatedExitText = Instantiate(debugExitTextPrefab, transform);
            instantiatedExitText.transform.position = GetCellPosition(CurrentBoard.Exit.Position);
        }

        return currentBoard;
    }

    void RecalculateTileDistances(ScenarioState updatedGameState)
    {
        Vector2Int playerPosition = updatedGameState.player.Position;

        Tile playerTile = currentBoard.Tiles[playerPosition.x, playerPosition.y];

        currentBoard.ProcessTileDistancesToPlayer(playerTile);

#if UNITY_EDITOR
        if (debuggingTileDistances)
        {
            for (int y = 0; y < boardWidth; y++)
            {
                for (int x = 0; x < boardWidth; x++)
                {
                    Text thisText = boardTileImages[x, y].GetComponentInChildren<Text>();
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
                Image occupantImage = tileOccupantImages[xCounter, yCounter];
                occupantImage.sprite = null;
                occupantImage.color = invisible;

                Image itemImage = tileItemImages[xCounter, yCounter];
                itemImage.sprite = null;
                itemImage.color = invisible;
            }
        }
    }

    public void DrawBoard(ScenarioState currentGameState, bool isResolvingTurn)
    {
        ClearBoard();

        // Draw current player at position.
        DrawEntityAtPosition(currentGameState.player, opaque);

        // Draw current enemies at positions.
        for (int i = 0; i < currentGameState.enemies.Count; i++)
        {
            EntityData entity = currentGameState.enemies[i];
            if (entity != null)
            {
                DrawEntityAtPosition(entity, opaque);
            }
        }

        for (int i = 0; i < currentGameState.items.Count; i++)
        {
            ItemData item = currentGameState.items[i];
            if (item != null)
            {
                DrawItemAtPosition(item, opaque);
            }
        }
    }

    public void DEBUG_DrawUpcomingEntitySprite(EntityData entity)
    {
        Debug.Log("drawing entity sprite at position:" + entity.Position);
        DrawEntityAtPosition(entity, translucent);
    }

    public void DrawEntityAtPosition(EntityData entity, Color color)
    {
        Vector2Int entityPosition = entity.Position;
        Image entityImage = tileOccupantImages[entityPosition.x, entityPosition.y];
        entityImage.sprite = entity.EntitySprite;
        entityImage.color = color;
    }

    public void DrawItemAtPosition(ItemData item, Color color)
    {
        Vector2Int itemPosition = item.Position;
        Image itemImage = tileItemImages[itemPosition.x, itemPosition.y];
        itemImage.sprite = item.sprite;
        itemImage.color = color;
    }

    public void HighlightSelectedCell(Vector2Int position)
    {
        tileOccupantImages[position.x, position.y].color = new Color(1f, 1f, 0f);
    }

    public void HighlightDamageCell(Vector2Int position)
    {
        Image cellImage = tileOccupantImages[position.x, position.y];
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
        RectTransform cellRectTransform = boardTiles[position.x, position.y].GetComponent<RectTransform>();

        return cellRectTransform.position;
    }

    public Vector2 GetCellEdgePosition(Vector2Int position, Direction edgeDirection)
    {
        RectTransform cellRectTransform = boardTiles[position.x, position.y].GetComponent<RectTransform>();
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

    public Vector2 GetCellCornerPosition(Vector2Int cellPosition, Direction verticalDirection, Direction horizontalDirection)
    {
        if (verticalDirection == Direction.Right ||
            verticalDirection == Direction.Left ||
            horizontalDirection == Direction.Up ||
            horizontalDirection == Direction.Down)
        {
            Debug.LogError("Bad directional input for cell corner position retrieval.");
            return Vector2.zero;
        }

        RectTransform cellRectTransform = boardTiles[cellPosition.x, cellPosition.y].GetComponent<RectTransform>();
        Vector3[] worldCorners = new Vector3[4];
        cellRectTransform.GetWorldCorners(worldCorners);

        if (verticalDirection == Direction.Down && horizontalDirection == Direction.Left)
        {
            return worldCorners[0];
        }
        else if (verticalDirection == Direction.Up && horizontalDirection == Direction.Left)
        {
            return worldCorners[1];
        }
        else if (verticalDirection == Direction.Up && horizontalDirection == Direction.Right)
        {
            return worldCorners[2];
        }
        else
        {
            return worldCorners[3];
        }
    }

    public float GetTileWidth()
    {
        RectTransform cellRectTransform = boardTiles[0,0].GetComponent<RectTransform>();
        return cellRectTransform.rect.width;
    }

    #region DEBUG ONLY
    public static void TurnTileColor(Tile tile, Color color)
    {
        instance.boardTileImages[tile.Position.x, tile.Position.y].color = color;
    }
    #endregion
}
