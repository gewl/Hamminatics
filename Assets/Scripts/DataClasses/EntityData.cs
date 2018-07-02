using UnityEngine;

[CreateAssetMenu(menuName="Data/Entity")]
public class EntityData : ScriptableObject {
    public string ID;
    public int Health = 1;
    public Sprite EntitySprite;
    public int Speed = 1;
    [HideInInspector]
    public Vector2Int Position;

    public MovementCardData MovementCard;
    public AttackCardData attackCard;
}
