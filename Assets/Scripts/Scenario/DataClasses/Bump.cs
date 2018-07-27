using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bump {

    public EntityData bumpingEntity;
    public EntityData bumpedEntity;

    private Bump() { }

    public Bump(EntityData _bumpingEntity, EntityData _bumpedEntity)
    {
        bumpedEntity = _bumpedEntity;
        bumpingEntity = _bumpingEntity;
    }
}
