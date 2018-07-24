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

public enum ItemCategory
{
    Treasure,
    Trap
}

public enum ItemCollectionType
{
    OnStep,
    OnFinishMove
}

public struct Constants
{
    public const string PLAYER_ID = "Player";
    public const int MAX_EQUIPPED_CARDS = 4;
}