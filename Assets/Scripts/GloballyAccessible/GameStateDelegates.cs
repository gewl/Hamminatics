using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameStateDelegates {

    public delegate void NoParametersDelegate();
    public static NoParametersDelegate OnCardDeselected;

    public delegate void PositionDelegate(Vector2Int position);
    public static PositionDelegate OnPositionAttacked;

    public delegate void UpcomingStatesChangeDelegate(GameState currentState, List<ProjectedGameState> upcomingStates);
    public static UpcomingStatesChangeDelegate OnCurrentGameStateChange;
    public static UpcomingStatesChangeDelegate ReturnToDefaultBoard;

    public delegate void ResolvingTurnDelegate(Turn turn);
    public static ResolvingTurnDelegate OnResolvingTurn;

    public delegate void GameStateChangeDelegate(GameState updatedGameState);
    public static GameStateChangeDelegate OnRoundEnded;

    public delegate void CardDataDelegate(CardData card);
    public static CardDataDelegate OnCardSelected;

    public delegate void SpecificEntityDelegate(EntityData entity, GameState currentGameState, List<ProjectedGameState> upcomingGameStates);
    public static SpecificEntityDelegate OnEntitySelected;

    public delegate void SpecificItemDelegate(ItemData item);
    public static SpecificItemDelegate OnItemSelected;
}
