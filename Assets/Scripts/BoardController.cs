using UnityEngine;
using UnityEngine.UI;

public class BoardController : MonoBehaviour {

    int boardWidth = 5;
    public int BoardWidth { get { return boardWidth; } }
    Transform[,] boardCells;
    Image[,] cellContentImages;

    SpriteManager spriteManager;

    private void Awake()
    {
        InitializeBoardStructures();

        spriteManager = GetComponentInParent<SpriteManager>();
    }

    void InitializeBoardStructures()
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

            xCounter++;

            if (xCounter >= boardWidth)
            {
                yCounter++;
                xCounter = 0;
            }
        }
    }

    public void DrawBoard(CellContents[,] boardState)
    {
        if (boardState.Length > BoardWidth * BoardWidth)
        {
            Debug.LogError("BoardController asked to draw board state larger than current board.");
        }

        for (int yCounter = 0; yCounter < boardWidth; yCounter++)
        {
            for (int xCounter = 0; xCounter < boardWidth; xCounter++)
            {
                Image contentsImage = cellContentImages[xCounter, yCounter];
                if (boardState[xCounter, yCounter] != CellContents.None)
                {
                    contentsImage.sprite = spriteManager.GetCellSprite(boardState[xCounter, yCounter]);
                    contentsImage.color = new Color(1f, 1f, 1f, 1f);
                }
                else
                {
                    contentsImage.sprite = null;
                    contentsImage.color = new Color(1f, 1f, 1f, 0f);
                }
            }
        }
    }

    public void HighlightCell(Vector2Int position)
    {
        cellContentImages[position.x, position.y].color = new Color(1f, 1f, 0f);
    }
}
