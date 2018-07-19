using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathStep {

    public EntityData pathingEntity;
    public Vector2Int newPosition;
    public Vector2Int lastPosition;
    public EntityData bumpedEntity;
    public EntityData bumpedBy;

    public PathStep nextStep;
    
    public PathStep(EntityData _pathingEntity, Vector2Int _newPosition, EntityData _bumpedEntity = null)
    {
        pathingEntity = _pathingEntity;
        newPosition = _newPosition;
        bumpedBy = null;
        bumpedEntity = _bumpedEntity;
        lastPosition = new Vector2Int(-1, -1);
    }

    public bool IsFirstStep()
    {
        return lastPosition == new Vector2Int(-1, -1);
    }

    public bool IsLastStep()
    {
        return nextStep == null || (bumpedBy == null && nextStep.bumpedBy != null);
    }

    public Vector2Int GetNextPosition()
    {
        if (nextStep == null)
        {
            Debug.LogError("Trying to get next position from last step in path");
            return new Vector2Int(-1, -1);
        }

        return nextStep.newPosition;
    }
}
