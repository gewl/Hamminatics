using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameStateDelegates {

    public delegate void NoParametersDelegate();
    public static NoParametersDelegate OnCardDeselected;

    #region scenario delegates
    public delegate void PositionDelegate(Vector2Int position);
    public static PositionDelegate OnPositionAttacked;

    public delegate void UpcomingStatesChangeDelegate(ScenarioState currentState, List<ProjectedGameState> upcomingStates);
    public static UpcomingStatesChangeDelegate OnCurrentScenarioStateChange;
    public static UpcomingStatesChangeDelegate ReturnToDefaultBoard;

    public delegate void ProjectedGameStateDelegate(ProjectedGameState projectedState);
    public static ProjectedGameStateDelegate OnResolvingState;

    public delegate void ScenarioStateDelegate(ScenarioState updatedGameState);
    public static ScenarioStateDelegate OnNewScenario;
    public static ScenarioStateDelegate OnRoundEnded;

    public delegate void CardDataDelegate(CardData card);
    public static CardDataDelegate OnCardSelected;

    public delegate void SpecificEntityDelegate(EntityData entity, ScenarioState currentGameState, List<ProjectedGameState> upcomingGameStates);
    public static SpecificEntityDelegate OnEntitySelected;

    public delegate void SpecificItemDelegate(ItemData item);
    public static SpecificItemDelegate OnItemSelected;
    #endregion

    #region Campaign delegates
    public delegate void CampaignStateDelegate(CampaignState campaignState);
    public static CampaignStateDelegate OnCampaignStateUpdated;
    #endregion
}
