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

    void DrawEntityPath(EntityData entity, Path path)
    {
        if (path.IsEmpty())
        {
            return;
        }

        PathEnumerator pathEnumerator = path.GetEnumerator();

        // Iterate through 'bumped' steps until it:
        // A: Hits the end of the path (indicating that there are no moves, the entirety of the path is bumps), or,
        // B: Hits the first 'move' step.
        while (pathEnumerator.Current != null && pathEnumerator.Current.bumpedBy != null)
        {
            GenerateNewBumpImage(pathEnumerator.Current);
            pathEnumerator.MoveNext();
        }

        if (pathEnumerator.Current == null)
        {
            Debug.Log("returning in first current step check");
            return;
        }

        while (pathEnumerator.Current != null && pathEnumerator.Current.bumpedBy == null)
        {
            Sprite pathSprite = GeneratePathSprite(pathEnumerator.Current);

            GenerateNewPathStepImage(pathEnumerator.Current,
                pathSprite,
                entity.IdentifyingColor);

            pathEnumerator.MoveNext();
        }

        if (pathEnumerator.Current == null)
        {
            return;
        }

        while (pathEnumerator.Current != null && pathEnumerator.Current.bumpedBy != null)
        {
            GenerateNewBumpImage(pathEnumerator.Current);
            pathEnumerator.MoveNext();
        }

        if (pathEnumerator.Current == null)
        {
            return;
        }
    }

    void GenerateNewBumpImage(PathStep step)
    {
        GameObject instantiatedBumpImage = ImageManager.GetPathImage(ImageManager.GetPathSprite(PathType.Bumped));
        instantiatedBumpImage.transform.SetParent(transform);
        instantiatedBumpImage.transform.position = boardController.GetCellPosition(step.newPosition);
        instantiatedBumpImage.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(0f, 0f, GetImageRotation(step)));
        instantiatedBumpImage.GetComponent<Image>().color = step.pathingEntity.IdentifyingColor;
    }

    void GenerateNewPathStepImage(PathStep step, Sprite stepSprite, Color color)
    {
        GameObject instantiatedPathImage = ImageManager.GetPathImage(stepSprite);
        instantiatedPathImage.transform.SetParent(transform);
        instantiatedPathImage.transform.position = boardController.GetCellPosition(step.newPosition);
        instantiatedPathImage.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(0f, 0f, GetImageRotation(step)));
        instantiatedPathImage.GetComponent<Image>().color = step.pathingEntity.IdentifyingColor;
    }

    float GetImageRotation(PathStep step)
    {
        float result = 0f;

        Direction directionOfEntrance = default(Direction);

        if (step.IsFirstStep())
        {
            directionOfEntrance = BoardHelperFunctions.GetDirectionFromPosition(step.GetNextPosition(), step.newPosition);
        }
        else
        {
            directionOfEntrance = BoardHelperFunctions.GetDirectionFromPosition(step.newPosition, step.lastPosition);
        }

        switch (directionOfEntrance)
        {
            case Direction.Up:
                break;
            case Direction.Right:
                result = -90f;
                break;
            case Direction.Down:
                result = 180f;
                break;
            case Direction.Left:
                result = 90f;
                break;
            default:
                break;
        }

        return result;
    }

    Sprite GeneratePathSprite(PathStep step)
    {
        if (step.IsFirstStep())
        {
            return ImageManager.GetPathSprite(PathType.Beginning);
        }
        else if (step.IsLastStep())
        {
            return ImageManager.GetPathSprite(PathType.Terminating);
        }
        else
        {
            Sprite resultSprite = ImageManager.GetPathSprite(PathType.Straight);

            if (BoardHelperFunctions.AreTwoPositionsLinear(step.lastPosition, step.GetNextPosition()))
            {
                return resultSprite;
            }
            Vector2Int localVectorToLastPosition = step.lastPosition - step.newPosition;
            Vector2Int localVectorToNextPosition = step.GetNextPosition() - step.newPosition;

            float angleBetween = Vector2.SignedAngle(localVectorToLastPosition, localVectorToNextPosition);

            if (angleBetween - 90f == 0f)
            {
                resultSprite = ImageManager.GetPathSprite(PathType.LeftTurn);
            }
            else if (angleBetween + 90f == 0f)
            {
                resultSprite = ImageManager.GetPathSprite(PathType.RightTurn);
            }

            return resultSprite;
        }
    }

    Sprite GetPathSpriteFromCoordinates(PathStep step, Vector2Int nextPosition, Vector2Int lastPosition)
    {
        PathType pathDirection = PathType.Bumped;
        Vector2Int defaultVector = new Vector2Int(-1, -1);
        if (step.bumpedBy != null)
        {
            pathDirection = PathType.Bumped;
        }
        else if (nextPosition == defaultVector)
        {
            pathDirection = PathType.Terminating;
        }
        else if (lastPosition == defaultVector)
        {
            pathDirection = PathType.Beginning;
        }
        else
        {
            Vector2Int localVectorToLastPosition = lastPosition - step.newPosition;
            Vector2Int localVectorToNextPosition = nextPosition - step.newPosition;

            float angleBetween = Vector2.SignedAngle(localVectorToLastPosition, localVectorToNextPosition);

            if (Mathf.Abs(angleBetween) == 180)
            {
                pathDirection = PathType.Straight;
            }
            else if (angleBetween - 90f == 0f)
            {
                pathDirection = PathType.LeftTurn;
            }
            else if (angleBetween + 90f == 0f)
            {
                pathDirection = PathType.RightTurn;
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
