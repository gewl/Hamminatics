using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TurnDrawer : MonoBehaviour {

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

    public void DrawAllPaths(GameState state)
    {
        Clear();

        state
            .GetAllEntities()
            .ForEach(entity => DrawEntityPath(entity, state.entityPathsMap[entity]));
    }

    void DrawEntityPath(EntityData entity, List<PathStep> path)
    {
        if (path.Count == 0)
        {
            return;
        }

        for (int i = 0; i < path.Count; i++)
        {
            PathStep step = path[i];

            Vector2Int nextPosition = i == path.Count - 1 ? new Vector2Int(-1, -1) : path[i + 1].position;
            Vector2Int lastPosition = i == 0 ? new Vector2Int(-1, -1) : path[i - 1].position;

            GenerateNewPathStepImage(step, 
                GetPathSpriteFromCoordinates(step, nextPosition, lastPosition), 
                BoardHelperFunctions.GetDirectionFromPosition(step.position, lastPosition),
                entity.IdentifyingColor);
        }
    }

    void GenerateNewPathStepImage(PathStep step, Sprite stepSprite, Direction entranceDirection, Color color)
    {
        GameObject instantiatedPathImage = ImageManager.GetPathImage(stepSprite, entranceDirection);
        instantiatedPathImage.transform.SetParent(transform);
        instantiatedPathImage.transform.position = boardController.GetCellPosition(step.position);
        instantiatedPathImage.GetComponent<Image>().color = color;
    }

    Sprite GetPathSpriteFromCoordinates(PathStep step, Vector2Int nextPosition, Vector2Int lastPosition)
    {
        PathDirection pathDirection = PathDirection.Bumped;
        Vector2Int defaultVector = new Vector2Int(-1, -1);
        if (step.bumpedBy != null)
        {
            pathDirection = PathDirection.Bumped;
        }
        else if (nextPosition == defaultVector)
        {
            pathDirection = PathDirection.Terminating;
        }
        else if (lastPosition == defaultVector)
        {
            pathDirection = PathDirection.Beginning;
        }
        else
        {
            Vector2Int localVectorToLastPosition = lastPosition - step.position;
            Vector2Int localVectorToNextPosition = nextPosition - step.position;

            float angleBetween = Vector2.SignedAngle(localVectorToLastPosition, localVectorToNextPosition);

            if (Mathf.Abs(angleBetween) == 180)
            {
                pathDirection = PathDirection.Straight;
            }
            else if (angleBetween - 90f <= 0.5f)
            {
                pathDirection = PathDirection.Left;
            }
            else if (angleBetween + 90f <= 0.5f)
            {
                pathDirection = PathDirection.Right;
            }
        }

        return ImageManager.GetPathSprite(pathDirection);
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
