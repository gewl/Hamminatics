﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;  
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class BoardController : MonoBehaviour {
    ScenarioStateManager scenarioStateManager;
    static BoardController instance;

    float _canvasScale = -1f;
    float CanvasScale
    {
        get
        {
            if (_canvasScale == -1f)
            {
                _canvasScale = GetComponentInParent<CanvasScaler>().GetComponent<RectTransform>().localScale.x;
            }

            return _canvasScale;
        }
    }

    bool boardInitialized = false;
    static int boardWidth = 5;
    public static int BoardWidth { get { return boardWidth; } }
    Transform[,] boardTiles;
    Image[,] boardTileImages;
    Image[,] tileOccupantImages;
    Image[,] tileItemImages;
    EventTrigger[,] tileEventTriggers;

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

    List<EventTrigger> lastListeningMovementTriggers;

    #region Lifecycle
    private void Awake()
    {
        scenarioStateManager = GetComponentInParent<ScenarioStateManager>();

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
        tileEventTriggers = new EventTrigger[boardWidth, boardWidth];

        int childCount = transform.childCount;

        int xCounter = 0, yCounter = 0; 

        for (int i = 0; i < childCount; i++)
        {
            Transform cell = transform.GetChild(i);
            boardTiles[xCounter, yCounter] = cell;

            boardTileImages[xCounter, yCounter] = cell.GetComponent<Image>();

            Image tileOccupant = cell.GetChild(0).GetComponent<Image>();
            tileOccupantImages[xCounter, yCounter] = tileOccupant;
            EventTrigger tileEventTrigger = tileOccupant.GetComponent<EventTrigger>();
            tileEventTriggers[xCounter, yCounter] = tileEventTrigger;
            AddPermanentTileEventTriggers(tileEventTrigger, xCounter, yCounter);

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

    public GameBoard GenerateBoard(bool isBossRoom = false)
    {
        if (!boardInitialized)
        {
            InitializeBoard();
        }
        bool doesBoardHaveExit = !isBossRoom;
        currentBoard = new GameBoard(doesBoardHaveExit);

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

        if (debuggingExitPosition && doesBoardHaveExit)
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

    #region tile event triggers
    void AddPermanentTileEventTriggers(EventTrigger trigger, int x, int y)
    {
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        pointerDownEntry.callback.AddListener(GeneratePointerDownListener(x, y));
        trigger.triggers.Add(pointerDownEntry);

        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        pointerUpEntry.callback.AddListener(GeneratePointerUpListener());
        trigger.triggers.Add(pointerUpEntry);

        EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        pointerEnterEntry.callback.AddListener(GeneratePointerEnterListener(x, y));
        trigger.triggers.Add(pointerEnterEntry);
    }

    UnityAction<BaseEventData> GeneratePointerDownListener(int x, int y)
    {
        Vector2Int cellPosition = new Vector2Int(x, y);

        return (BaseEventData e) =>
        {
            scenarioStateManager.RegisterPointerDown(cellPosition);
        };
    }

    UnityAction<BaseEventData> GeneratePointerEnterListener(int x, int y)
    {
        Vector2Int cellPosition = new Vector2Int(x, y);

        return (BaseEventData e) =>
        {
            scenarioStateManager.AddTileToMove(currentBoard.GetTileAtPosition(cellPosition));
        };
    }

    UnityAction<BaseEventData> GeneratePointerUpListener()
    {
        return (BaseEventData e) =>
        {
            scenarioStateManager.StopMovementPathing();
        };
    }

    #endregion

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
        entityImage.sprite = entity.entitySprite;
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

    #endregion

    #region info retrieval
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
        return cellRectTransform.rect.width * CanvasScale;
    }

    /// <summary>
    /// Get positions of outermost corners of corner tiles in world space.
    /// </summary>
    /// <returns>Returns a 4-element array of positions, starting at the upper-right, proceeding clockwise.</returns>
    public Vector2[] GetBoardCorners()
    {
        return new Vector2[4]
        {
            GetCellCornerPosition(new Vector2Int(boardWidth - 1, 0), Direction.Up, Direction.Right),
            GetCellCornerPosition(new Vector2Int(boardWidth - 1, boardWidth - 1), Direction.Down, Direction.Right),
            GetCellCornerPosition(new Vector2Int(0, boardWidth - 1), Direction.Down, Direction.Left),
            GetCellCornerPosition(new Vector2Int(0, 0), Direction.Up, Direction.Left)
        };
    }
    #endregion

    #region DEBUG ONLY
    public static void TurnTileColor(Tile tile, Color color)
    {
        instance.boardTileImages[tile.Position.x, tile.Position.y].color = color;
    }
    #endregion
}
