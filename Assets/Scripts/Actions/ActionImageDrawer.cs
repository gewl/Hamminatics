﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionImageDrawer : MonoBehaviour {

    [SerializeField]
    TurnStackController actionStackController;
    [SerializeField]
    BoardController boardController;

    public void Clear()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void DrawEntireTurn(List<CompletedMove> completedMoves, List<CompletedAction> completedActions)
    {
        Clear();

        for (int i = 0; i < completedMoves.Count; i++)
        {
            CompletedMove move = completedMoves[i];
            if (move.Moves.Count > 0)
            {
                GenerateNewMoveImage(move.OriginTile.Position, move.Moves[0]);
            }
        }

        for (int i = 0; i < completedActions.Count; i++)
        {
            CompletedAction completedAction = completedActions[i];

            GenerateNewActionImage(completedAction.Category, completedAction.OriginTile.Position, completedAction.Direction);
        }
    }

    public void DrawSingleMove(Vector2Int position, Direction direction, bool shouldClearFirst = true)
    {
        if (shouldClearFirst)
        {
            Clear();
        }

        GenerateNewMoveImage(position, direction);
    }

    public void DrawSingleAction(Action action)
    {
        Clear();

        GenerateNewActionImage(action.card.Category, action.entity.Position, action.direction);
    }

    void GenerateNewMoveImage(Vector2Int position, Direction direction)
    {
        GameObject instantiatedActionImage = ImageManager.GetAbilityPointer(CardCategory.Movement, direction);

        instantiatedActionImage.transform.SetParent(transform);
        Vector2 cellEdgePosition = boardController.GetCellEdgePosition(position, direction);
        instantiatedActionImage.transform.position = cellEdgePosition;
    }

    void GenerateNewActionImage(CardCategory category, Vector2Int position, Direction direction)
    {
        GameObject instantiatedActionImage = ImageManager.GetAbilityPointer(category, direction);

        instantiatedActionImage.transform.SetParent(transform);
        Vector2 cellEdgePosition = boardController.GetCellEdgePosition(position, direction);
        instantiatedActionImage.transform.position = cellEdgePosition;
    }

}
