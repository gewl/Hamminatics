using UnityEngine;
using UnityEngine.UI;

public class BoardController : MonoBehaviour {

    int boardWidth = 5;
    public int BoardWidth { get { return boardWidth; } }
    Transform[,] boardSpaces;
    Image[,] boardContents;

    SpriteManager spriteManager;

    private void Awake()
    {
        InitializeBoardStructures();

        spriteManager = GetComponentInParent<SpriteManager>();
    }

    void InitializeBoardStructures()
    {
        boardSpaces = new Transform[boardWidth, boardWidth];
        boardContents = new Image[boardWidth, boardWidth];

        int childCount = transform.childCount;

        int xCounter = 0, yCounter = 0; 

        for (int i = 0; i < childCount; i++)
        {
            Transform space = transform.GetChild(i);
            boardSpaces[xCounter, yCounter] = space;

            Image spaceContents = space.GetChild(0).GetComponent<Image>();
            boardContents[xCounter, yCounter] = spaceContents;

            xCounter++;

            if (xCounter >= boardWidth)
            {
                yCounter++;
                xCounter = 0;
            }
        }
    }

    public void DrawBoard(SpaceContents[,] boardState)
    {
        if (boardState.Length > BoardWidth * BoardWidth)
        {
            Debug.LogError("BoardController asked to draw board state larger than current board.");
        }

        for (int yCounter = 0; yCounter < boardWidth; yCounter++)
        {
            for (int xCounter = 0; xCounter < boardWidth; xCounter++)
            {
                Image contentsImage = boardContents[xCounter, yCounter];
                if (boardState[xCounter, yCounter] != SpaceContents.None)
                {
                    contentsImage.sprite = spriteManager.GetSpaceSprite(boardState[xCounter, yCounter]);
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
}
