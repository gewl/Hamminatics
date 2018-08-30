#region Cards/actions
public enum CardCategory
{
    Movement,
    Attack,
    Self
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
    HealOverTime,
    Push,
    Pull,
    Blowback,
    FollowUp
}

public struct Constants
{
    public const string PLAYER_ID = "Player";
    public const int MAX_EQUIPPED_CARDS = 4;
    public const int MAX_MODIFIERS = 8;
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

public enum StoreItem
{
    Card,
    Health,
    Upgrade
}

#region event handling
public class EventType
{
    public const string CHANGE_VALUE = "changeValue";
    public const string OFFER_CARD = "offerCard";
    public const string TRIGGER_SCENARIO = "triggerScenario";
    public const string APPLY_MODIFIER = "applyModifier";
    public const string OFFER_UPGRADE = "offerUpgrade";
}

public class ChangeValueTarget
{
    public const string HEALTH = "health";
    public const string GOLD = "gold";
}
#endregion

public class ScenarioRewardText
{
    public const string HEALTH_TITLE = "Certainty";
    public const string HEALTH_BODY = "You have tidily dispatched of your enemies. You feel a surge of pride and self-surety. Why had you doubted yourself?\n\nYou have recovered 1 health.";

    public const string UPGRADE_TITLE = "Refinement";
    public const string UPGRADE_BODY = "You continually think you have reached the pinnacle of your strength. Then you surprise yourself. It happens quickly, almost imperceptibly, but you've learned to keep an eye out for it. In the middle of battle, between one moment and the next, it happens. Like a subtle parting. You understand. A twist of the tongue, maybe, or a slantwise approach to an idea you'd thought a hundred times before. A path opens before you. You take the step.\n\nOne of your abilities has improved.";

    public const string NEW_CARD_TITLE = "Growth";
    public const string NEW_CARD_BODY = "You've learned so much from your studies. You forget that you can learn things from others. It happens in a moment of conflict. Your idea glances off another's, creating a sort of spark. You can use this. This can be yours.";

    public const string GOLD_TITLE = "Abundance";
    public const string GOLD_BODY = "You've earned it.\n\nYou have gained some gold";
}