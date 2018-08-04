using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour {
    const string PLAYER_ID = "Player";

    public static CampaignState CurrentCampaign { get; private set; }

    [SerializeField]
    MapController mapController;
    [SerializeField]
    ScenarioStateManager scenarioManager;
    [SerializeField]
    EventPane eventPane;
    [SerializeField]
    GameObject dimmer;

    static GameStateManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        EntityData player = DataManager.GetEntityData(PLAYER_ID);
        int mapLayerCount = 6;

        CurrentCampaign = new CampaignState(new Inventory(), new Map(0, mapLayerCount), player);
        GameStateDelegates.OnCampaignStateUpdated(CurrentCampaign);

        InitializeMap();
    }

    void InitializeMap()
    {
        Map currentMap = CurrentCampaign.currentMap;
        mapController.DrawMap(currentMap);
        mapController.UpdateMapState(CurrentCampaign);
    }

    public void UpdatePlayerPosition(MapNode newPlayerNode)
    {
        CurrentCampaign.UpdatePlayerNode(newPlayerNode);
        mapController.UpdateMapState(CurrentCampaign);

        switch (newPlayerNode.nodeType)
        {
            case MapNodeType.Start:
                Debug.Log("moved to " + newPlayerNode.nodeType);
                break;
            case MapNodeType.Scenario:
                SwitchToScenario();
                break;
            case MapNodeType.Event:
                TriggerEvent();
                break;
            case MapNodeType.Store:
                Debug.Log("moved to " + newPlayerNode.nodeType);
                break;
            case MapNodeType.End:
                Debug.Log("moved to " + newPlayerNode.nodeType);
                break;
            default:
                break;
        }
    }

    #region event processing
    public void ProcessEventOutcome(string effect, string stringData, int intData)
    {
        switch (effect)
        {
            case EventType.CHANGE_VALUE:
                HandleChangeValue(stringData, intData);
                break;
            default:
                Debug.LogError("No handler found for effect: " + effect);
                break;
        }

        GameStateDelegates.OnCampaignStateUpdated(CurrentCampaign);
    }

    void HandleChangeValue(string stringData, int intData)
    {
        switch (stringData)
        {
            case ChangeValueTarget.GOLD:
                CurrentCampaign.inventory.gold += intData;
                break;
            case ChangeValueTarget.HEALTH:
                CurrentCampaign.player.ChangeHealthValue(intData);
                break;
            default:
                Debug.LogError("Unable to HandleChangeValue because stringData not found: " + stringData);
                break;
        }

    }
    #endregion

    #region focus/pane switching
    void SwitchToScenario()
    {
        mapController.gameObject.SetActive(false);
        scenarioManager.GenerateAndDrawScenario(CurrentCampaign.depth);
    }

    public void SwitchToCampaign(ScenarioState lastScenarioState)
    {
        scenarioManager.gameObject.SetActive(false);
        mapController.gameObject.SetActive(true);

        CurrentCampaign.player = lastScenarioState.player;
        CurrentCampaign.inventory = lastScenarioState.inventory;
        GameStateDelegates.OnCampaignStateUpdated(CurrentCampaign);
    }

    void TriggerEvent()
    {
        eventPane.DisplayEvent(CurrentCampaign);
        SetDim(true);
    }

    public void CloseEventPane()
    {
        eventPane.gameObject.SetActive(false);
        SetDim(false);
    }

    public static void SetDim(bool isDim)
    {
        instance.dimmer.gameObject.SetActive(isDim);
    }
    #endregion
}
