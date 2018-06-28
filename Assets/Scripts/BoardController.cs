using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class BoardController : MonoBehaviour {
    GameStateManager gameStateManager;

    int boardWidth = 5;
    public int BoardWidth { get { return boardWidth; } }
    Transform[,] boardCells;
    Image[,] cellContentImages;

    ImageManager spriteManager;
    Canvas canvas;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();

        InitializeBoard();

        gameStateManager = GetComponentInParent<GameStateManager>();

        spriteManager = GetComponentInParent<ImageManager>();
    }

    void InitializeBoard()
    {
        boardCells = new Transform[boardWidth, boardWidth];
        cellContentImages = new Image[boardWidth, boardWidth];

        int childCount = transform.childCount;

        int xCounter = 0, yCounter = 0; 

        for (int i = 0; i < childCount; i++)
        {
            Transform cell = transform.GetChild(i);
            boardCells[xCounter, yCounter] = cell;

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
    }

    public void DrawBoard(GameState currentGameState, GameState projectedGameState)
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

        Vector2Int projectedPlayerPosition = projectedGameState.player.Position;
        if (playerPosition != projectedPlayerPosition)
        {
            Image projectedPlayerCellImage = cellContentImages[projectedGameState.player.Position.x, projectedGameState.player.Position.y];
            projectedPlayerCellImage.sprite = currentGameState.player.EntitySprite;
            projectedPlayerCellImage.color = new Color(1f, 1f, 1f, 0.5f);
        }
    }

    public void HighlightCell(Vector2Int position)
    {
        cellContentImages[position.x, position.y].color = new Color(1f, 1f, 0f);
    }

    UnityAction GenerateCellClickListener(int x, int y)
    {
        Vector2Int cellPosition = new Vector2Int(x, y);

        return () =>
        {
            gameStateManager.RegisterCellInteraction(cellPosition);
        };
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
}
