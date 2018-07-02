using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class BoardController : MonoBehaviour {
    GameStateManager gameStateManager;

    static int boardWidth = 5;
    public static int BoardWidth { get { return boardWidth; } }
    Transform[,] boardCells;
    Image[,] boardCellImages;
    Image[,] cellContentImages;

    ImageManager spriteManager;
    Canvas canvas;

    [SerializeField]
    GameObject debugText;

    public GameBoard CurrentBoard { get; private set;  }
    public bool DebuggingTileDistances = false;

    #region Lifecycle
    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        gameStateManager = GetComponentInParent<GameStateManager>();
        spriteManager = GetComponentInParent<ImageManager>();

        InitializeBoard();
        gameStateManager.InitializeGameState(CurrentBoard);
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

        CurrentBoard = new GameBoard();

#if UNITY_EDITOR
        if (DebuggingTileDistances)
        {
            for (int y = 0; y < boardWidth; y++)
            {
                for (int x = 0; x < boardWidth; x++)
                {
                    boardCellImages[x, y].sprite = DataManager.GetTileSprite(CurrentBoard.Tiles[x, y].ID);
                    GameObject thisText = Instantiate(debugText, boardCells[x,y], false);
                    thisText.GetComponent<Text>().text = CurrentBoard.Tiles[x, y].DistanceFromPlayer.ToString();
                }
            }
        }
#endif
    }

    void RecalculateTileDistances(GameState updatedGameState)
    {
        Vector2Int playerPosition = updatedGameState.player.Position;

        Tile playerTile = CurrentBoard.Tiles[playerPosition.x, playerPosition.y];

        CurrentBoard.ProcessTileDistancesToPlayer(playerTile);

#if UNITY_EDITOR
        if (DebuggingTileDistances)
        {
            for (int y = 0; y < boardWidth; y++)
            {
                for (int x = 0; x < boardWidth; x++)
                {
                    Text thisText = boardCellImages[x, y].GetComponentInChildren<Text>();
                    thisText.text = CurrentBoard.Tiles[x, y].DistanceFromPlayer.ToString();
                }
            }
        }
#endif
    }

    #region GUI manipulation
    public void DrawBoard(GameState currentGameState, GameState projectedGameState, List<Vector2Int> projectedDamagePositions)
    {
        for (int yCounter = 0; yCounter < boardWidth; yCounter++)
        {
            for (int xCounter = 0; xCounter < boardWidth; xCounter++)
            {
                Image contentsImage = cellContentImages[xCounter, yCounter];
                contentsImage.sprite = null;
                contentsImage.color = new Color(1f, 1f, 1f, 0f);
            }
        }

        Vector2Int projectedPlayerPosition = projectedGameState.player.Position;
        Image projectedPlayerCellImage = cellContentImages[projectedPlayerPosition.x, projectedPlayerPosition.y];
        projectedPlayerCellImage.sprite = currentGameState.player.EntitySprite;
        projectedPlayerCellImage.color = new Color(1f, 1f, 1f, 0.5f);

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

        Vector2Int playerPosition = currentGameState.player.Position;
        Image playerCellImage = cellContentImages[playerPosition.x, playerPosition.y];
        playerCellImage.sprite = currentGameState.player.EntitySprite;
        playerCellImage.color = new Color(1f, 1f, 1f, 1f);

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

        foreach (Vector2Int position in projectedDamagePositions)
        {
            HighlightDamageCell(position);
        }
    }

    public void HighlightSelectedCell(Vector2Int position)
    {
        cellContentImages[position.x, position.y].color = new Color(1f, 1f, 0f);
    }

    public void HighlightDamageCell(Vector2Int position)
    {
        cellContentImages[position.x, position.y].color = new Color(1f, 0f, 0f, 0.5f);
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

    #region Exposed methods for data retrieval
    public Tile GetTileAtPosition(Vector2Int position)
    {
        return CurrentBoard.Tiles[position.x, position.y];
    }

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

    // TODO: Currently only supports 1-range moves, need to refactor for longer/more complex moves.
    public List<Tile> GetPotentialMoves(Vector2Int startingPosition, int range)
    {
        Tile startingTile = GetTileAtPosition(startingPosition);

        return GetPotentialMoves(startingTile, range);
    }

    public List<Tile> GetPotentialMoves(Tile startingTile, int range, List<Tile> checkedTiles = null)
    {
        List<Tile> potentialTargets = new List<Tile>();
        if (range == 0)
        {
            return potentialTargets;
        }

        if (checkedTiles == null)
        {
            checkedTiles = new List<Tile> { startingTile };
        }

        for (int i = 0; i < startingTile.Neighbors.Count; i++)
        {
            Tile potentialTarget = startingTile.Neighbors[i];

            if (checkedTiles.Contains(potentialTarget))
            {
                continue; 
            }

            potentialTargets.Add(potentialTarget);
            checkedTiles.Add(potentialTarget);
        }

        for (int i = 0; i < potentialTargets.Count; i++)
        {
            potentialTargets.AddRange(GetPotentialMoves(potentialTargets[i], range - 1, checkedTiles));
        }

        return potentialTargets;
    }

    public bool IsTileDirectlyReachable(Vector2Int position1, Vector2Int position2)
    {
        return IsTileDirectlyReachable(CurrentBoard.GetTileAt(position1), CurrentBoard.GetTileAt(position2));
    }

    public bool IsTileDirectlyReachable(Tile tile1, Tile tile2)
    {
        if (tile1.Position.x != tile2.Position.x && tile1.Position.y != tile2.Position.y)
        {
            return false;
        }

        return true;
    }

    public List<Tile> GetDirectlyReachableTiles(Vector2Int position)
    {
        return GetDirectlyReachableTiles(CurrentBoard.GetTileAt(position));
    }

    public List<Tile> GetDirectlyReachableTiles(Tile startingTile)
    {
        List<Tile> reachableTiles = new List<Tile>();

        Tile leftNeighbor = startingTile.GetDirectionalNeighbor(Direction.Left);
        while (leftNeighbor != null)
        {
            reachableTiles.Add(leftNeighbor);

            leftNeighbor = leftNeighbor.GetDirectionalNeighbor(Direction.Left);
        }
            
        Tile upNeighbor = startingTile.GetDirectionalNeighbor(Direction.Up);
        while (upNeighbor != null)
        {
            reachableTiles.Add(upNeighbor);

            upNeighbor = upNeighbor.GetDirectionalNeighbor(Direction.Up);
        }

        Tile downNeighbor = startingTile.GetDirectionalNeighbor(Direction.Down);
        while (downNeighbor != null)
        {
            reachableTiles.Add(downNeighbor);

            downNeighbor = downNeighbor.GetDirectionalNeighbor(Direction.Down);
        }

        Tile rightNeighbor = startingTile.GetDirectionalNeighbor(Direction.Right);
        while (rightNeighbor != null)
        {
            reachableTiles.Add(rightNeighbor);

            rightNeighbor = rightNeighbor.GetDirectionalNeighbor(Direction.Right);
        }

        return reachableTiles;
    }

    #endregion
}
