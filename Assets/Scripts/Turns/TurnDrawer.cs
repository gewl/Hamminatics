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

    public void DrawUpcomingStates(List<ProjectedGameState> upcomingStates)
    {
        Clear();
        EntityData lastActiveEntity = null;
        for (int i = 0; i < upcomingStates.Count; i++)
        {
            ProjectedGameState projectedState = upcomingStates[i];
            EntityData activeEntity = projectedState.activeEntity;

            ProjectedGameState nextState = i == upcomingStates.Count - 1 ?
                null :
                upcomingStates[i + 1];

            if (projectedState.cardType == CardCategory.Movement)
            {
                Vector2Int positionThisState = activeEntity.Position;
                Vector2Int positionLastState = projectedState
                    .gameState
                    .lastGamestate
                    .GetEntityWhere(e => e.ID == activeEntity.ID)
                    .Position;
                EntityData activeEntityTwoStatesAgo = projectedState
                    .gameState
                    .lastGamestate
                    .lastGamestate
                    .GetEntityWhere(e => e.ID == activeEntity.ID);

                bool isEntitysFirstMove = lastActiveEntity == null ||
                    activeEntity.ID != lastActiveEntity.ID ||
                    (activeEntity.ID == activeEntityTwoStatesAgo.ID &&
                    activeEntity.Position == activeEntityTwoStatesAgo.Position);

                bool isFailedBump = projectedState.bump != null &&
                    positionLastState == positionThisState;
                if (isEntitysFirstMove && !isFailedBump)
                {
                    DrawPath_Beginning(activeEntity, positionLastState, positionThisState);
                }

                if (projectedState.bump == null)
                {
                    DrawPath(activeEntity, lastActiveEntity, projectedState, nextState);
                }
                else
                {
                    DrawBump(activeEntity, lastActiveEntity, projectedState, nextState);
                }
            }

            lastActiveEntity = activeEntity;
        }
    }

    void DrawPath(EntityData activeEntity, EntityData lastActiveEntity, ProjectedGameState projectedState, ProjectedGameState nextState)
    {
        bool isEntitysLastMove = projectedState.cardType == CardCategory.Movement &&
            (nextState == null ||
            nextState.cardType != CardCategory.Movement ||
            nextState.activeEntity.ID != activeEntity.ID);

        Vector2Int positionLastState = projectedState
            .gameState
            .lastGamestate
            .GetEntityWhere(e => e.ID == activeEntity.ID)
            .Position;
        Vector2Int positionThisState = activeEntity.Position;

        if (isEntitysLastMove)
        {
            DrawPath_Ending(activeEntity, positionLastState, positionThisState);
            return;
        }

        bool isNextMoveFailedBump = nextState.bump != null &&
            nextState.activeEntity.Position == activeEntity.Position;
        if (projectedState.cardType == CardCategory.Movement && 
            !isEntitysLastMove &&
            !isNextMoveFailedBump)
        {
            Vector2Int positionNextState = nextState
                .gameState
                .GetEntityWhere(e => e.ID == activeEntity.ID)
                .Position;

            DrawPath_Between(activeEntity, positionLastState, positionThisState, positionNextState);
        }
    }

    void DrawPath_Beginning(EntityData entity, Vector2Int positionLastState, Vector2Int positionThisState)
    {
        if (positionThisState == positionLastState)
        {
            return;
        }

        Sprite pathSprite = ImageManager.GetPathSprite(PathType.Beginning);
        Direction fakeDirectionOfEntrance = BoardHelperFunctions.GetDirectionFromPosition(positionThisState, positionLastState);

        GenerateAndPositionPathImage(positionLastState, fakeDirectionOfEntrance, pathSprite, entity.IdentifyingColor);
    }

    void DrawPath_Between(EntityData entity, Vector2Int positionLastState, Vector2Int positionThisState, Vector2Int positionNextState)
    {
        PathType pathType = GetPathType(positionLastState, positionThisState, positionNextState);
        Sprite pathSprite = ImageManager.GetPathSprite(pathType);
        Direction directionOfEntrance = BoardHelperFunctions.GetDirectionFromPosition(positionThisState, positionLastState);

        GenerateAndPositionPathImage(positionThisState, directionOfEntrance, pathSprite, entity.IdentifyingColor);
    }

    PathType GetPathType(Vector2Int positionLastState, Vector2Int positionThisState, Vector2Int positionNextState)
    {
        if (BoardHelperFunctions.AreTwoPositionsLinear(positionLastState, positionNextState))
        {
            return PathType.Straight;
        }

        Vector2Int localVectorToLastPosition = positionLastState - positionThisState;
        Vector2Int localVectorToNextPosition = positionNextState - positionThisState;

        float angleBetween = Vector2.SignedAngle(localVectorToLastPosition, localVectorToNextPosition);

        if (angleBetween - 90f == 0f)
        {
            return PathType.LeftTurn;
        }
        else
        {
            return PathType.RightTurn;
        }
    }

    void DrawPath_Ending(EntityData entity, Vector2Int positionLastState, Vector2Int positionThisState)
    {
        if (positionThisState == positionLastState)
        {
            return;
        }

        Sprite pathSprite = ImageManager.GetPathSprite(PathType.Terminating);
        Direction directionOfEntrance = BoardHelperFunctions.GetDirectionFromPosition(positionThisState, positionLastState);

        GenerateAndPositionPathImage(positionThisState, directionOfEntrance, pathSprite, entity.IdentifyingColor);
    }

    void GenerateAndPositionPathImage(Vector2Int position, Direction directionOfEntrance, Sprite pathSprite, Color color)
    {
        GameObject instantiatedPathImage = ImageManager.GetPathImage(pathSprite);
        instantiatedPathImage.transform.SetParent(transform);
        instantiatedPathImage.transform.position = boardController.GetCellPosition(position);

        instantiatedPathImage.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(0f, 0f, GetImageRotation(directionOfEntrance)));
        instantiatedPathImage.GetComponent<Image>().color = color;
    }

    void DrawBump(EntityData activeEntity, EntityData lastActiveEntity, ProjectedGameState projectedState, ProjectedGameState nextState)
    {
        bool isEntitysFirstMove = lastActiveEntity == null || activeEntity.ID != lastActiveEntity.ID;
        Bump bump = projectedState.bump;
        Vector2Int positionThisState = activeEntity.Position;
        Vector2Int positionLastState = projectedState
            .gameState
            .lastGamestate
            .GetEntityWhere(e => e.ID == activeEntity.ID)
            .Position;

        bool bumpSucceeds = activeEntity.Position != positionLastState;

        if (bumpSucceeds)
        {
            DrawPath_Ending(activeEntity, positionLastState, positionThisState);
            DrawSuccessfulBumpEffect(positionThisState, BoardHelperFunctions.GetDirectionFromPosition(positionThisState, positionLastState));

            DrawPath_Beginning(bump.bumpedEntity, positionThisState, bump.bumpedEntity.Position);
        }
        else if (isEntitysFirstMove)
        {
            Vector2Int bumpedEntityPosition = bump.bumpedEntity.Position;
            Sprite pathSprite = ImageManager.GetPathSprite(PathType.Beginning);
            Direction directionOfEntrance = BoardHelperFunctions.GetDirectionFromPosition(bumpedEntityPosition, positionThisState);
            GenerateAndPositionPathImage(positionThisState, directionOfEntrance, pathSprite, activeEntity.IdentifyingColor);

            DrawFailedBumpEffect(positionThisState,
                BoardHelperFunctions.GetDirectionFromPosition(positionThisState, bumpedEntityPosition));
        }
        else
        {
            Vector2Int positionTwoStatesAgo = projectedState
                .gameState
                .lastGamestate
                .lastGamestate
                .GetEntityWhere(e => e.ID == activeEntity.ID)
                .Position;
            PathType pathType = GetFailedBumpPathType(positionTwoStatesAgo, positionThisState, bump.bumpedEntity.Position);
            Sprite pathSprite = ImageManager.GetPathSprite(pathType);
            Vector2Int bumpedEntityPosition = bump.bumpedEntity.Position;
            Direction directionOfEntrance = BoardHelperFunctions.GetDirectionFromPosition(positionThisState, positionTwoStatesAgo);

            GenerateAndPositionPathImage(positionThisState, directionOfEntrance, pathSprite, activeEntity.IdentifyingColor);
            DrawFailedBumpEffect(positionThisState,
                BoardHelperFunctions.GetDirectionFromPosition(positionThisState, bumpedEntityPosition));
        }
    }

    void DrawSuccessfulBumpEffect(Vector2Int position, Direction entranceDirectionOfBumper)
    {
        Sprite bumpEffectSprite = ImageManager.GetPathSprite(PathType.Bumped);
        GameObject instantiatedBumpImage = ImageManager.GetPathImage(bumpEffectSprite);
        instantiatedBumpImage.transform.SetParent(transform);
        instantiatedBumpImage.transform.position = boardController.GetCellPosition(position);
        float imageRotation = GetImageRotation(entranceDirectionOfBumper);
        instantiatedBumpImage.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(0f, 0f, imageRotation));
    }

    void DrawFailedBumpEffect(Vector2Int position, Direction entranceDirectionOfBumper)
    {
        Sprite bumpEffectSprite = ImageManager.GetPathSprite(PathType.Bumped);
        GameObject instantiatedBumpImage = ImageManager.GetPathImage(bumpEffectSprite);
        instantiatedBumpImage.transform.SetParent(transform);
        instantiatedBumpImage.transform.position = boardController.GetCellEdgePosition(position, entranceDirectionOfBumper);
        float imageRotation = GetImageRotation(entranceDirectionOfBumper);
        instantiatedBumpImage.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(0f, 0f, imageRotation));
    }

    PathType GetFailedBumpPathType(Vector2Int positionTwoStatesAgo, Vector2Int positionThisState, Vector2Int bumpeePosition)
    {
        if (BoardHelperFunctions.AreTwoPositionsLinear(positionTwoStatesAgo, bumpeePosition))
        {
            return PathType.FailedBumpStraight;
        }

        Vector2Int localVectorToLastPosition = positionTwoStatesAgo - positionThisState;
        Vector2Int localVectorToNextPosition = bumpeePosition - positionThisState;

        float angleBetween = Vector2.SignedAngle(localVectorToLastPosition, localVectorToNextPosition);

        if (angleBetween - 90f == 0f)
        {
            return PathType.FailedBumpLeft;
        }
        else
        {
            return PathType.FailedBumpRight;
        }
    }


    float GetImageRotation(Direction directionOfEntrance)
    {
        float result = 0f;

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
        // "Next position" refers to:
        // 1. IF last path step before failed bump, the position of the bumpee.
        // 2. ELSE the next position that the pathing entity will move into.
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
            Vector2Int nextPosition = step.IsLastStepBeforeFailedBump() ?
                step.nextStep.bumpedEntity.Position :
                step.GetNextPosition();

            Sprite resultSprite = ImageManager.GetPathSprite(PathType.Straight);

            if (BoardHelperFunctions.AreTwoPositionsLinear(step.GetLastPosition(), nextPosition))
            {
                return step.IsLastStepBeforeFailedBump() ? 
                    ImageManager.GetPathSprite(PathType.FailedBumpStraight) :
                    resultSprite;
            }

            Vector2Int localVectorToLastPosition = step.GetLastPosition() - step.newPosition;
            Vector2Int localVectorToNextPosition = nextPosition - step.newPosition;

            float angleBetween = Vector2.SignedAngle(localVectorToLastPosition, localVectorToNextPosition);

            if (angleBetween - 90f == 0f)
            {
                resultSprite = step.IsLastStepBeforeFailedBump() ?
                    ImageManager.GetPathSprite(PathType.FailedBumpLeft) :
                    ImageManager.GetPathSprite(PathType.LeftTurn);
            }
            else if (angleBetween + 90f == 0f)
            {
                resultSprite = step.IsLastStepBeforeFailedBump() ?
                    ImageManager.GetPathSprite(PathType.FailedBumpRight) :
                    ImageManager.GetPathSprite(PathType.RightTurn);
            }

            return resultSprite;
        }
    }

    Sprite GetPathSpriteFromCoordinates(PathStep step, Vector2Int nextPosition, Vector2Int lastPosition)
    {
        PathType pathDirection = PathType.Bumped;
        Vector2Int defaultVector = new Vector2Int(-1, -1);
        if (step.bumpedByStep != null)
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
