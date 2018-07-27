using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDashController : MonoBehaviour {

    [SerializeField]
    InfoPanelHandler textInfoHandler;

    private void OnEnable()
    {
        ScenarioStateDelegates.OnCardSelected += DisplayCardInfo;
        ScenarioStateDelegates.OnEntitySelected += DisplayEntityInfo;
        ScenarioStateDelegates.OnItemSelected += DisplayItemInfo;
        ScenarioStateDelegates.OnCardDeselected += HideInfoPanel;
        ScenarioStateDelegates.ReturnToDefaultBoard += OnReturnToDefaultBoard;
    }

    private void OnDisable()
    {
        ScenarioStateDelegates.OnCardSelected -= DisplayCardInfo;
        ScenarioStateDelegates.OnEntitySelected -= DisplayEntityInfo;
        ScenarioStateDelegates.OnItemSelected -= DisplayItemInfo;
        ScenarioStateDelegates.OnCardDeselected -= HideInfoPanel;
        ScenarioStateDelegates.ReturnToDefaultBoard -= OnReturnToDefaultBoard;
    }

    void DisplayCardInfo(CardData card)
    {
        textInfoHandler.gameObject.SetActive(true);
        textInfoHandler.UpdateText(card);
    }

    void DisplayEntityInfo(EntityData entity, ScenarioState currentGameState, List<ProjectedGameState> upcomingStates)
    {
        textInfoHandler.gameObject.SetActive(true);
        textInfoHandler.UpdateText(entity);
    }

    void DisplayItemInfo(ItemData item)
    {
        textInfoHandler.gameObject.SetActive(true);
        textInfoHandler.UpdateText(item);
    }

    void HideInfoPanel()
    {
        textInfoHandler.gameObject.SetActive(false);
    }

    void OnReturnToDefaultBoard(ScenarioState currentGameState, List<ProjectedGameState> upcomingGameStates)
    {
        HideInfoPanel();
    }

}
