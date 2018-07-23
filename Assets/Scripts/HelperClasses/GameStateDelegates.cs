using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameStateDelegates {

    public delegate void UpcomingStatesChangeDelegate(GameState currentState, List<ProjectedGameState> upcomingStates);
    public static UpcomingStatesChangeDelegate OnCurrentGameStateChange;
    public static UpcomingStatesChangeDelegate ReturnToDefaultBoard;

    public delegate void ResolvingTurnDelegate(Turn turn);
    public static ResolvingTurnDelegate OnResolvingTurn;

    public delegate void GameStateChangeDelegate(GameState updatedGameState);
    public static GameStateChangeDelegate OnRoundEnded;

    public delegate void SpecificEntityDelegate(EntityData entity, GameState currentGameState, List<ProjectedGameState> upcomingGameStates);
    public static SpecificEntityDelegate OnEntitySelected;
}
