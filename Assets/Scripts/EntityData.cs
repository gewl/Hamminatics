using UnityEngine;

[CreateAssetMenu(menuName="Data/Entity")]
public class EntityData : ScriptableObject {
    public string Name;
    public int Health = 1;
    public Sprite EntitySprite;
    [HideInInspector]
    public Vector2Int Position;
}
