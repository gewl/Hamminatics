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
    Right,
    Down,
    Left
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

public enum PathType
{
    Straight,
    RightTurn,
    LeftTurn,
    Terminating,
    Beginning,
    Bumped,
    FailedBumpStraight,
    FailedBumpRight,
    FailedBumpLeft
}

public struct Constants
{
    public const string PLAYER_ID = "Player";
}