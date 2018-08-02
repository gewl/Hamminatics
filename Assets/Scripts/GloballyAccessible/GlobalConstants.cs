﻿#region Cards/actions
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
#endregion

#region board/paths
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
#endregion

#region Ground items
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

public enum TrapCategory
{
    InstantDamage,
    Warp
}
#endregion

public enum ModifierCategory
{
    Slow,
    Speed,
    Weaken,
    Strength,
    DamageOverTime,
    HealOverTime
}

public struct Constants
{
    public const string PLAYER_ID = "Player";
    public const int MAX_EQUIPPED_CARDS = 4;
}

public enum MapNodeType
{
    Start,
    Scenario,
    Event,
    Store,
    End
}

public enum Layer
{
    Campaign,
    Scenario,
    Event
}