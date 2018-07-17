using UnityEngine;

[CreateAssetMenu(menuName = "Data/Entity")]
public class EntityData : ScriptableObject {
    public string ID;
    public int Health = 1;
    public Sprite EntitySprite;
    public int Speed = 1;
    [HideInInspector]
    public Vector2Int Position;

    public MovementCardData MovementCard;
    public AttackCardData attackCard;

    public override int GetHashCode()
    {
        int result = 37;

        result *= 397;
        if (ID != null)
        {
            result += ID.GetHashCode();
        }

        result *= 397;
        if (Position != null)
        {
            result += Position.GetHashCode();
        }

        return result;
    }

    public override bool Equals(object other)
    {
        if (other == null || GetType() != other.GetType())
        {
            return false;
        }

        EntityData entity = (EntityData)other;

        return entity.GetHashCode() == GetHashCode();
    }
}
