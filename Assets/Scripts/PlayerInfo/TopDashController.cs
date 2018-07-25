using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDashController : MonoBehaviour {

    [SerializeField]
    InfoPanelHandler textInfoHandler;

    private void OnEnable()
    {
        GameStateDelegates.OnCardSelected += DisplayCardInfo;
        GameStateDelegates.OnEntitySelected += DisplayEntityInfo;
        GameStateDelegates.OnItemSelected += DisplayItemInfo;
        GameStateDelegates.OnCardDeselected += HideInfoPanel;
        GameStateDelegates.ReturnToDefaultBoard += OnReturnToDefaultBoard;
    }

    private void OnDisable()
    {
        GameStateDelegates.OnCardSelected -= DisplayCardInfo;
        GameStateDelegates.OnEntitySelected -= DisplayEntityInfo;
        GameStateDelegates.OnItemSelected -= DisplayItemInfo;
        GameStateDelegates.OnCardDeselected -= HideInfoPanel;
        GameStateDelegates.ReturnToDefaultBoard -= OnReturnToDefaultBoard;
    }

    void DisplayCardInfo(CardData card)
    {
        textInfoHandler.gameObject.SetActive(true);
        textInfoHandler.UpdateText(card);
    }

    void DisplayEntityInfo(EntityData entity, GameState currentGameState, List<ProjectedGameState> upcomingStates)
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

    void OnReturnToDefaultBoard(GameState currentGameState, List<ProjectedGameState> upcomingGameStates)
    {
        HideInfoPanel();
    }

}
