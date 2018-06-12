using UnityEngine;

public class BoardController : MonoBehaviour {

    int boardWidth = 5;
    Transform[,] boardSpaces;

    private void Awake()
    {
        boardSpaces = GetSpaces();
    }

    Transform[,] GetSpaces()
    {
        Transform[,] spaces = new Transform[boardWidth, boardWidth];

        int childCount = transform.childCount;

        int xCounter = 0, yCounter = 0; 

        for (int i = 0; i < childCount; i++)
        {
            Transform space = transform.GetChild(i);

            spaces[xCounter, yCounter] = space;

            xCounter++;

            if (xCounter >= boardWidth)
            {
                yCounter++;
                xCounter = 0;
            }
        }

        return spaces;
    }
}
