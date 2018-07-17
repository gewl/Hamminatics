using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathStep {

    public EntityData pathingEntity;
    public Vector2Int position;
    public EntityData bumpedEntity;
    public EntityData bumpedBy;
    
    public PathStep(EntityData _pathingEntity, Vector2Int _position, EntityData _bumpedEntity = null)
    {
        pathingEntity = _pathingEntity;
        position = _position;
        bumpedBy = null;
        bumpedEntity = _bumpedEntity;
    }
}
