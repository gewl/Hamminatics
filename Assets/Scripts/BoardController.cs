using System;
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

    SpriteManager spriteManager;

    private void Awake()
    {
        InitializeBoard();

        gameStateManager = GetComponentInParent<GameStateManager>();

        spriteManager = GetComponentInParent<SpriteManager>();
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

    public void DrawBoard(List<EntityData> entities)
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

        for (int i = 0; i < entities.Count; i++)
        {
            EntityData entity = entities[i];
            if (entity != null)
            {
                Image contentsImage = cellContentImages[entity.Position.x, entity.Position.y];
                contentsImage.sprite = entity.EntitySprite;
                contentsImage.color = new Color(1f, 1f, 1f, 1f);
            }
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
}
