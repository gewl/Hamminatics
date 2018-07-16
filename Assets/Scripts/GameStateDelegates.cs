using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameStateDelegates {

    public delegate void ResolvingTurnDelegate(Turn turn);
    public static ResolvingTurnDelegate OnResolvingTurn;

    public delegate void GameStateChangeDelegate(GameState updatedGameState);
    public static GameStateChangeDelegate OnCurrentGameStateChange;
    public static GameStateChangeDelegate OnRoundEnded;

}
