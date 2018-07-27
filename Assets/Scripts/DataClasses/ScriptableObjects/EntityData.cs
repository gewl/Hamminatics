using UnityEngine;

[CreateAssetMenu(menuName = "Entity")]
public class EntityData : ScriptableObject {
    public string ID;
    public string description = "Placeholder description.";

    [SerializeField]
    protected int _maxHealth = 1;
    public int MaxHealth { get { return _maxHealth; } }
    private int _currentHealth = int.MinValue;
    public int CurrentHealth {
        get
        {
            if (_currentHealth == int.MinValue)
            {
                _currentHealth = _maxHealth;
            }
            return _currentHealth;
        }
    }

    public Sprite EntitySprite;
    public int Speed = 1;
    public Color IdentifyingColor;
    private Vector2Int _position;
    [HideInInspector]
    public Vector2Int Position { get { return _position; } }

    public ItemData dropItem;

    public MovementCardData movementCard;
    public AttackCardData attackCard;

    public void SetHealth(int newHealth)
    {
        _currentHealth = newHealth;
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
        EntityData copy = ScriptableObject.Instantiate(this) as EntityData;

        copy.ID = ID;
        copy._maxHealth = _maxHealth;
        copy._currentHealth = _currentHealth;
        copy.EntitySprite = EntitySprite;
        copy.Speed = Speed;
        copy.IdentifyingColor = IdentifyingColor;
        copy._position = Position;
        copy.movementCard = movementCard;
        copy.attackCard = attackCard;
        copy.dropItem = dropItem;

        return copy;
    }

    public void SetPosition(Vector2Int newPosition, ScenarioState state)
    {
        _position = newPosition;

        ItemData itemData = state.GetItemInPosition(newPosition);

        if (itemData == null || itemData.CollectionType != ItemCollectionType.OnStep)
        {
            return;
        }

        state.CollectItem(itemData, this);
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
