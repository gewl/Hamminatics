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

        PathStep firstMove = path.First(p => p.bumpedBy == null);
        PathStep lastMove = path.Last(p => p.bumpedBy == null);

        Vector2Int entityLastPosition = entity.Position;
        List<PathStep>.Enumerator pathEnumerator = path.GetEnumerator();
        pathEnumerator.MoveNext();

        // Iterate through 'bumped' steps until it:
        // A: Hits the end of the path (indicating that there are no moves, the entirety of the path is bumps), or,
        // B: Hits the first 'move' step.
        while (pathEnumerator.Current != null && pathEnumerator.Current != firstMove)
        {
            PathStep step = pathEnumerator.Current;
            GenerateNewBumpImage(step.position, entityLastPosition);

            entityLastPosition = step.position;
            pathEnumerator.MoveNext();
        }

        if (pathEnumerator.Current == null)
        {
            return;
        }

        while (pathEnumerator.Current != null && pathEnumerator.Current.bumpedBy == null)
        {
            PathStep step = pathEnumerator.Current;

            if (step.position == entityLastPosition)
            {
                pathEnumerator.MoveNext();
                continue;
            }

            GenerateNewPathStepImage(step,
                GetPathSpriteFromCoordinates(step, step.position, entityLastPosition),
                BoardHelperFunctions.GetDirectionFromPosition(entityLastPosition, step.position),
                entity.IdentifyingColor);

            if (entity.ID == Constants.PLAYER_ID)
            {
                Debug.Log("Moved " + BoardHelperFunctions.GetDirectionFromPosition(entityLastPosition, step.position));
                Debug.Log("Player moved to: " + step.position + " from " + entityLastPosition);
            }
            entityLastPosition = step.position;

            pathEnumerator.MoveNext();
        }

        if (pathEnumerator.Current == null)
        {
            return;
        }

        while (pathEnumerator.Current != null)
        {
            PathStep step = pathEnumerator.Current;
            GenerateNewBumpImage(step.position, entityLastPosition);

            entityLastPosition = step.position;
            pathEnumerator.MoveNext();
        }
    }

    void GenerateNewBumpImage(Vector2Int newPosition, Vector2Int bumpedFromPosition)
    {
        GameObject instantiatedBumpImage = ImageManager.GetPathImage(
            ImageManager.GetPathSprite(PathDirection.Bumped),
            BoardHelperFunctions.GetDirectionFromPosition(newPosition, bumpedFromPosition)
            );
        instantiatedBumpImage.transform.SetParent(transform);
        instantiatedBumpImage.transform.position = boardController.GetCellPosition(newPosition);
    }

    void GenerateNewPathStepImage(PathStep step, Sprite stepSprite, Direction entranceDirection, Color color)
    {
        GameObject instantiatedPathImage = ImageManager.GetPathImage(stepSprite, entranceDirection);
        instantiatedPathImage.transform.SetParent(transform);
        instantiatedPathImage.transform.position = boardController.GetCellPosition(step.position);
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
            else if (angleBetween - 90f == 0f)
            {
                pathDirection = PathDirection.LeftTurn;
            }
            else if (angleBetween + 90f == 0f)
            {
                pathDirection = PathDirection.RightTurn;
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
