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

    [SerializeField]
    Sprite deadEntitySprite;
    [SerializeField]
    GameObject durationClockPrefab;
    [SerializeField]
    GameObject entityHealthDisplayPrefab;

    bool drawingSelectedEntity = true;
    float deselectedEntityActionOpacity = 0.4f;

    private void OnEnable()
    {
        GameStateDelegates.ReturnToDefaultBoard += DrawUpcomingStates;
        GameStateDelegates.OnCurrentGameStateChange += DrawUpcomingStates;
        GameStateDelegates.OnEntitySelected += HighlightSelectedEntityStates;
    }

    private void OnDisable()
    {
        GameStateDelegates.ReturnToDefaultBoard -= DrawUpcomingStates;
        GameStateDelegates.OnCurrentGameStateChange -= DrawUpcomingStates;
        GameStateDelegates.OnEntitySelected -= HighlightSelectedEntityStates;
    }

    public void Clear()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void DrawUpcomingStates(GameState currentState, List<ProjectedGameState> upcomingStates)
    {
        Clear();
        drawingSelectedEntity = true;
        EntityData lastActiveEntity = null;
        for (int i = 0; i < upcomingStates.Count; i++)
        {
            ProjectedGameState projectedState = upcomingStates[i];

            ProjectedGameState nextState = i == upcomingStates.Count - 1 ?
                null :
                upcomingStates[i + 1];

            DrawState(projectedState, nextState, lastActiveEntity);

            lastActiveEntity = projectedState.activeEntity;
        }

        DrawItemDurations(currentState.items);
        DrawEntityHealths(currentState.GetAllEntities());
    }

    void HighlightSelectedEntityStates(EntityData selectedEntity, GameState currentGameState, List<ProjectedGameState> upcomingStates)
    {
        Clear();
        Color translucent = new Color(1f, 1f, 1f, deselectedEntityActionOpacity);
        EntityData lastActiveEntity = null;
        for (int i = 0; i < upcomingStates.Count; i++)
        {
            ProjectedGameState projectedState = upcomingStates[i];
            ProjectedGameState nextState = i == upcomingStates.Count - 1 ?
                null :
                upcomingStates[i + 1];

            bool isEntityDeadThisState = !projectedState.gameState.HasEntityWhere(e => e == selectedEntity);
            bool didEntityDieThisState = projectedState.gameState.lastGameState.HasEntityWhere(e => selectedEntity == e);

            if (isEntityDeadThisState)
            {
                if (!didEntityDieThisState)
                {
                    drawingSelectedEntity = false;
                    DrawState(projectedState, nextState, lastActiveEntity);
                    lastActiveEntity = projectedState.activeEntity;
                    continue;
                }
                else
                {
                    drawingSelectedEntity = true;
                    DrawState(projectedState, nextState, lastActiveEntity);
                    lastActiveEntity = projectedState.activeEntity;
                    Vector2Int entityPositionLastState = projectedState.gameState
                        .lastGameState
                        .GetEntityWhere(e => e == selectedEntity)
                        .Position;
                    if (!projectedState.gameState.HasEntityWhere(e => e.Position == selectedEntity.Position) && selectedEntity.Position != entityPositionLastState)
                    {
                        GenerateAndPositionCellImage(entityPositionLastState, 0f, selectedEntity.EntitySprite, translucent);
                    }
                    GenerateAndPositionCellImage(entityPositionLastState, 0f, deadEntitySprite, Color.white);
                    continue;
                }
            }

            Vector2Int selectedEntityPositionThisState = projectedState
                .gameState
                .GetEntityWhere(e => e == selectedEntity)
                .Position;

            bool isSelectedEntityState = projectedState.activeEntity == selectedEntity;
            bool isSelectedEntityBumped = projectedState.bump != null && projectedState.bump.bumpedEntity == selectedEntity;
            bool isSelectedEntityAttacked = projectedState.attackedPositions.Contains(selectedEntityPositionThisState);

            if (isSelectedEntityState || isSelectedEntityBumped || isSelectedEntityAttacked)
            {
                drawingSelectedEntity = true;

                // Draw entity under attack targeting reticule if
                // (A) selected entity is hit OR
                // (B) selected entity is attacking & hits someone.
                projectedState
                    .attackedPositions
                    .Select(position => projectedState
                        .gameState
                        .GetTileOccupant(position))
                    .Where(attackTarget => attackTarget != null && isSelectedEntityState || attackTarget == selectedEntity)
                    .ToList()
                    .ForEach(entity => boardController.DrawEntityAtPosition(entity, translucent));

                // Draw entities killed this turn.
                projectedState
                    .gameState
                    .lastGameState
                    .GetAllEntities()
                    .ForEach(e =>
                    {
                        if (!projectedState.gameState.HasEntityWhere(projectedEntity => e == projectedEntity))
                        {
                            if (!currentGameState.HasEntityWhere(currentEntity => currentEntity.Position == e.Position))
                            {
                                GenerateAndPositionCellImage(e.Position, 0f, e.EntitySprite, translucent);
                            }
                            GenerateAndPositionCellImage(e.Position, 0f, deadEntitySprite, Color.white);
                        }
                    });
            }
            else
            {
                drawingSelectedEntity = false;
            }
            DrawState(projectedState, nextState, lastActiveEntity);
            lastActiveEntity = projectedState.activeEntity;
        }

        DrawEntityHealth(selectedEntity);
        DrawItemDurations(currentGameState.items);
    }

    void DrawState(ProjectedGameState projectedState, ProjectedGameState nextState, EntityData lastActiveEntity)
    {
        CardCategory actionCardCategory = projectedState.action.card.Category;
        if (actionCardCategory == CardCategory.Movement)
        {
            DrawMoveState(projectedState, nextState, lastActiveEntity);
        }
        else if (actionCardCategory == CardCategory.Attack)
        {
            DrawAttackState(projectedState);
        }
    }

    void DrawItemDurations(List<ItemData> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            ItemData item = items[i];

            Vector2Int itemPosition = item.Position;

            Vector2 cellCornerPosition = boardController.GetCellCornerPosition(itemPosition, Direction.Up, Direction.Right);
            GameObject durationClock = Instantiate(durationClockPrefab, transform);
            durationClock.GetComponentInChildren<Text>().text = item.Duration.ToString();

            durationClock.transform.position = cellCornerPosition;
        }
    }

    void DrawEntityHealth(EntityData entity)
    {
        GameObject entityHealthDisplay = Instantiate(entityHealthDisplayPrefab, transform);
        entityHealthDisplay.transform.position = boardController.GetCellPosition(entity.Position);

        entityHealthDisplay.GetComponent<EntityHealthDisplay>().UpdateHealthDisplay(entity.MaxHealth, entity.CurrentHealth);
    }

    void DrawEntityHealths(List<EntityData> entities)
    {
        for (int i = 0; i < entities.Count; i++)
        {
            EntityData entity = entities[i];
            DrawEntityHealth(entity);
        }
    }

    #region Movement drawing
    void DrawMoveState(ProjectedGameState projectedState, ProjectedGameState nextState, EntityData lastActiveEntity)
    {
        EntityData activeEntity = projectedState.activeEntity;
        Vector2Int positionThisState = activeEntity.Position;
        Vector2Int positionLastState = projectedState
            .gameState
            .lastGameState
            .GetEntityWhere(e => e.ID == activeEntity.ID)
            .Position;
        EntityData activeEntityTwoStatesAgo = projectedState
            .gameState
            .lastGameState
            .lastGameState
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
        bool isEntityAliveNextState = nextState == null || nextState
            .gameState
            .HasEntityWhere(e => e == activeEntity);
        bool isEntitysLastMove = projectedState.action.card.Category == CardCategory.Movement &&
            (nextState == null ||
            nextState.action.card.Category != CardCategory.Movement ||
            nextState.activeEntity.ID != activeEntity.ID);
            
        Vector2Int positionLastState = projectedState
            .gameState
            .lastGameState
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
            !isNextMoveFailedBump && 
            isEntityAliveNextState)
        {
            Vector2Int positionNextState = nextState
                .gameState
                .GetEntityWhere(e => e == activeEntity)
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

        GenerateAndPositionCellImage(positionLastState, GetImageRotation(fakeDirectionOfEntrance), pathSprite, entity.IdentifyingColor);
    }

    void DrawPath_Between(EntityData entity, Vector2Int positionLastState, Vector2Int positionThisState, Vector2Int positionNextState)
    {
        PathType pathType = GetPathType(positionLastState, positionThisState, positionNextState);
        Sprite pathSprite = ImageManager.GetPathSprite(pathType);
        Direction directionOfEntrance = BoardHelperFunctions.GetDirectionFromPosition(positionThisState, positionLastState);

        GenerateAndPositionCellImage(positionThisState, GetImageRotation(directionOfEntrance), pathSprite, entity.IdentifyingColor);
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

        GenerateAndPositionCellImage(positionThisState, GetImageRotation(directionOfEntrance), pathSprite, entity.IdentifyingColor);
    }

    void DrawBump(EntityData activeEntity, EntityData lastActiveEntity, ProjectedGameState projectedState, ProjectedGameState nextState)
    {
        bool isEntitysFirstMove = lastActiveEntity == null || activeEntity.ID != lastActiveEntity.ID;
        Bump bump = projectedState.bump;
        Vector2Int positionThisState = activeEntity.Position;
        Vector2Int positionLastState = projectedState
            .gameState
            .lastGameState
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
            GenerateAndPositionCellImage(positionThisState, GetImageRotation(directionOfEntrance), pathSprite, activeEntity.IdentifyingColor);

            DrawFailedBumpEffect(positionThisState,
                BoardHelperFunctions.GetDirectionFromPosition(positionThisState, bumpedEntityPosition));
        }
        else
        {
            Vector2Int positionTwoStatesAgo = projectedState
                .gameState
                .lastGameState
                .lastGameState
                .GetEntityWhere(e => e.ID == activeEntity.ID)
                .Position;
            PathType pathType = GetFailedBumpPathType(positionTwoStatesAgo, positionThisState, bump.bumpedEntity.Position);
            Sprite pathSprite = ImageManager.GetPathSprite(pathType);
            Vector2Int bumpedEntityPosition = bump.bumpedEntity.Position;
            Direction directionOfEntrance = BoardHelperFunctions.GetDirectionFromPosition(positionThisState, positionTwoStatesAgo);

            GenerateAndPositionCellImage(positionThisState, GetImageRotation(directionOfEntrance), pathSprite, activeEntity.IdentifyingColor);
            DrawFailedBumpEffect(positionThisState,
                BoardHelperFunctions.GetDirectionFromPosition(positionThisState, bumpedEntityPosition));
        }
    }

    void DrawSuccessfulBumpEffect(Vector2Int position, Direction entranceDirectionOfBumper)
    {
        Sprite bumpEffectSprite = ImageManager.GetPathSprite(PathType.Bumped);
        GenerateAndPositionCellImage(position, GetImageRotation(entranceDirectionOfBumper), bumpEffectSprite, Color.white);
    }

    void DrawFailedBumpEffect(Vector2Int position, Direction entranceDirectionOfBumper)
    {
        Sprite bumpEffectSprite = ImageManager.GetPathSprite(PathType.Bumped);
        GenerateAndPositionCellEdgeImage(position, entranceDirectionOfBumper, bumpEffectSprite, Color.white);
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
            EntityData activeEntity = projectedState.activeEntity;
            Vector2Int attackedPosition = projectedState.attackedPositions[i];
            GenerateAndPositionCellImage(attackedPosition, 0f, tileTargetedSprite, activeEntity.IdentifyingColor);

            AttackCardData cardData = projectedState.action.card as AttackCardData;
            Sprite pointerSprite = cardData.PointerSprite;

            float rotation = GetImageRotation(projectedState.action.direction);
            GameObject abilityPointer = ImageManager.GetAbilityPointer(pointerSprite, rotation);

            abilityPointer.transform.SetParent(transform);
            abilityPointer.transform.position = GetPointerImagePosition(activeEntity.Position, projectedState.action.direction);
            if (!drawingSelectedEntity)
            {
                Color pointerColor = Color.white;
                pointerColor.a = deselectedEntityActionOpacity;
                abilityPointer.GetComponent<Image>().color = pointerColor;
            }
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

    void GenerateAndPositionCellImage(Vector2Int position, float rotation, Sprite sprite, Color color)
    {
        GameObject instantiatedPathImage = ImageManager.GetOverlayImage(sprite);
        instantiatedPathImage.transform.SetParent(transform);
        instantiatedPathImage.transform.position = boardController.GetCellPosition(position);

        instantiatedPathImage.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(0f, 0f, rotation));

        if (!drawingSelectedEntity)
        {
            color = new Color(color.r, color.g, color.b, deselectedEntityActionOpacity);
        }

        instantiatedPathImage.GetComponent<Image>().color = color;
    }

    void GenerateAndPositionCellEdgeImage(Vector2Int position, Direction direction, Sprite pathSprite, Color color)
    {
        GameObject instantiatedBumpImage = ImageManager.GetOverlayImage(pathSprite);
        instantiatedBumpImage.transform.SetParent(transform);
        instantiatedBumpImage.transform.position = boardController.GetCellEdgePosition(position, direction);
        float imageRotation = GetImageRotation(direction);
        instantiatedBumpImage.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(0f, 0f, imageRotation));

        if (!drawingSelectedEntity)
        {
            color = new Color(color.r, color.g, color.b, deselectedEntityActionOpacity);
        }

        instantiatedBumpImage.GetComponent<Image>().color = color;
    }
}
