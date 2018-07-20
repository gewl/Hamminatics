using UnityEngine;

[CreateAssetMenu(menuName = "Data/Entity")]
public class EntityData : ScriptableObject {
    public string ID;
    public int Health = 1;
    public Sprite EntitySprite;
    public int Speed = 1;
    public Color IdentifyingColor;
    [HideInInspector]
    public Vector2Int Position;

    public MovementCardData movementCard;
    public AttackCardData attackCard;

    public override int GetHashCode()
    {
        int result = 37;

        result = result * 23 + ID.GetHashCode();
        return result;
    }

    public override bool Equals(object other)
    {
        if (other == null || GetType() != other.GetType())
        {
            return false;
        }

        EntityData entity = (EntityData)other;

        return entity.GetHashCode() == GetHashCode() && entity.Position == Position;
    }

    public EntityData Copy()
    {
        EntityData copy = ScriptableObject.CreateInstance(typeof(EntityData)) as EntityData;

        copy.ID = ID;
        copy.Health = Health;
        copy.EntitySprite = EntitySprite;
        copy.Speed = Speed;
        copy.IdentifyingColor = IdentifyingColor;
        copy.Position = Position;
        copy.movementCard = movementCard;
        copy.attackCard = attackCard;

        return copy;
    }
}
