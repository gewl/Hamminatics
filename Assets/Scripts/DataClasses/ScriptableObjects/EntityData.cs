using UnityEngine;

[CreateAssetMenu(menuName = "Entity")]
public class EntityData : ScriptableObject {
    public string ID;

    [SerializeField]
    protected int _health = 1;
    public int Health { get { return _health; } }

    public Sprite EntitySprite;
    public int Speed = 1;
    public Color IdentifyingColor;
    [HideInInspector]
    public Vector2Int Position;

    public ItemData dropItem;

    public MovementCardData movementCard;
    public AttackCardData attackCard;

    public void SetHealth(int newHealth)
    {
        _health = newHealth;
    }

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

        return entity.ID == ID;
    }

    public EntityData Copy()
    {
        EntityData copy = ScriptableObject.CreateInstance(typeof(EntityData)) as EntityData;

        copy.ID = ID;
        copy._health = Health;
        copy.EntitySprite = EntitySprite;
        copy.Speed = Speed;
        copy.IdentifyingColor = IdentifyingColor;
        copy.Position = Position;
        copy.movementCard = movementCard;
        copy.attackCard = attackCard;

        return copy;
    }

    public static bool operator ==(EntityData entity1, EntityData entity2)
    {
        if (ReferenceEquals(entity1, null))
        {
             return ReferenceEquals(entity2, null);
        }        
        return entity1.Equals(entity2);
    }

    public static bool operator !=(EntityData entity1, EntityData entity2)
    {
        return !(entity1 == entity2);
    }
}
