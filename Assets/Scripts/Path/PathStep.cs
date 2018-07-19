using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathStep {

    public EntityData pathingEntity;
    public Vector2Int newPosition;
    public EntityData bumpedEntity;
    public EntityData bumpedBy;

    PathStep lastStep;
    public PathStep nextStep;
    
    public PathStep(EntityData _pathingEntity, Vector2Int _newPosition, PathStep _lastStep, EntityData _bumpedEntity = null, EntityData _bumpedBy = null)
    {
        pathingEntity = _pathingEntity;
        newPosition = _newPosition;
        bumpedBy = _bumpedBy;
        bumpedEntity = _bumpedEntity;
        lastStep = _lastStep;
    }

    public bool IsFirstStep()
    {
        return lastStep == null;
    }

    public bool IsFailedBump()
    {
        return bumpedEntity != null && newPosition == lastStep.newPosition;
    }

    public bool IsLastStep()
    {
        return nextStep == null || (bumpedBy == null && nextStep.bumpedBy != null);
    }

    public bool IsLastStepBeforeFailedBump()
    {
        return nextStep != null &&
            nextStep.IsFailedBump();
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

    public Vector2Int GetLastPosition()
    {
        if (lastStep == null)
        {
            Debug.LogError("Trying to get last position from first step in path");
            return new Vector2Int(-1, -1);
        }

        return lastStep.newPosition;
    }
}
