public enum CellContents {
    None,
    Player,
    Enemy
}

public enum CardCategory
{
    Movement,
    Attack
}

public enum AbilityTarget
{
    Self,
    Targeted,
    Line,
    Anywhere
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public enum AbilityDirection
{
    Right,
    Left,
    Up,
    Down,
    Horizontal,
    Vertical,
    AllDirections
}

public struct Constants
{
    public const string PLAYER_ID = "Player";
}