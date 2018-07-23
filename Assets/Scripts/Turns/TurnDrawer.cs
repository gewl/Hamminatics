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

    private void OnEnable()
    {
        GameStateDelegates.ReturnToDefaultBoard += DrawUpcomingStates;
        GameStateDelegates.OnEntitySelected += HighlightSelectedEntityStates;
    }

    private void OnDisable()
    {
        GameStateDelegates.ReturnToDefaultBoard -= DrawUpcomingStates;
        GameStateDelegates.OnEntitySelected -= HighlightSelectedEntityStates;
    }

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

            ProjectedGameState nextState = i == upcomingStates.Count - 1 ?
                null :
                upcomingStates[i + 1];

            CardCategory actionCardCategory = projectedState.action.card.Category;
            if (actionCardCategory == CardCategory.Movement)
            {
                DrawMoveState(projectedState, nextState, lastActiveEntity);
            }
            else if (actionCardCategory == CardCategory.Attack)
            {
                DrawAttackState(projectedState);
            }

            lastActiveEntity = projectedState.activeEntity;
        }
    }

    void HighlightSelectedEntityStates(EntityData entity, GameState currentGameState, List<ProjectedGameState> upcomingStates)
    {
        Clear();
    }

    #region Movement drawing
    void DrawMoveState(ProjectedGameState projectedState, ProjectedGameState nextState, EntityData lastActiveEntity)
    {
        EntityData activeEntity = projectedState.activeEntity;
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

    void DrawPath(EntityData activeEntity, EntityData lastActiveEntity, ProjectedGameState projectedState, ProjectedGameState nextState)
    {
        bool isEntitysLastMove = projectedState.action.card.Category == CardCategory.Movement &&
            (nextState == null ||
            nextState.action.card.Category != CardCategory.Movement ||
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
        if (projectedState.action.card.Category == CardCategory.Movement && 
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
        GameObject instantiatedPathImage = ImageManager.GetOverlayImage(pathSprite);
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
        GameObject instantiatedBumpImage = ImageManager.GetOverlayImage(bumpEffectSprite);
        instantiatedBumpImage.transform.SetParent(transform);
        instantiatedBumpImage.transform.position = boardController.GetCellPosition(position);
        float imageRotation = GetImageRotation(entranceDirectionOfBumper);
        instantiatedBumpImage.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(0f, 0f, imageRotation));
    }

    void DrawFailedBumpEffect(Vector2Int position, Direction entranceDirectionOfBumper)
    {
        Sprite bumpEffectSprite = ImageManager.GetPathSprite(PathType.Bumped);
        GameObject instantiatedBumpImage = ImageManager.GetOverlayImage(bumpEffectSprite);
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

    #endregion

    #region Attack drawing

    void DrawAttackState(ProjectedGameState projectedState)
    {
        Sprite tileTargetedSprite = ImageManager.GetTileTargetedSprite();
        for (int i = 0; i < projectedState.attackedPositions.Count; i++)
        {
            Vector2Int attackedPosition = projectedState.attackedPositions[i];
            GameObject instantiatedTargetImage = ImageManager.GetOverlayImage(tileTargetedSprite);
            instantiatedTargetImage.transform.SetParent(transform);
            instantiatedTargetImage.transform.position = boardController.GetCellPosition(attackedPosition);

            instantiatedTargetImage.GetComponent<Image>().color = Color.red;

            AttackCardData cardData = projectedState.action.card as AttackCardData;
            Sprite pointerSprite = cardData.PointerSprite;

            float rotation = GetImageRotation(projectedState.action.direction);
            GameObject abilityPointer = ImageManager.GetAbilityPointer(pointerSprite, rotation);
            abilityPointer.transform.SetParent(transform);
            abilityPointer.transform.position = GetPointerImagePosition(projectedState.activeEntity.Position, projectedState.action.direction);
        }
    }

    #endregion

    #region Image helper funcs
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

    Vector3 GetPointerImagePosition(Vector2Int tilePosition, Direction pointingDirection)
    {
        Vector3 cellEdgePosition = boardController.GetCellEdgePosition(tilePosition, pointingDirection);
        float pointerBumpAmount = boardController.GetTileWidth() / 3f;

        switch (pointingDirection)
        {
            case Direction.Up:
                cellEdgePosition.x += pointerBumpAmount;
                break;
            case Direction.Right:
                cellEdgePosition.y -= pointerBumpAmount;
                break;
            case Direction.Down:
                cellEdgePosition.x -= pointerBumpAmount;
                break;
            case Direction.Left:
                cellEdgePosition.y += pointerBumpAmount;
                break;
            default:
                break;
        }

        return cellEdgePosition;
    }

    #endregion
}
