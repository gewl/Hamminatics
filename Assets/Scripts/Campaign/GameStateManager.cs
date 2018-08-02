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
                Debug.Log("moved to " + newPlayerNode.nodeType);
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

    void SwitchToScenario()
    {
        mapController.gameObject.SetActive(false);
        scenarioManager.gameObject.SetActive(true);
    }

    public void SwitchToCampaign(ScenarioState lastScenarioState)
    {
        scenarioManager.gameObject.SetActive(false);
        mapController.gameObject.SetActive(true);

        CurrentCampaign.player = lastScenarioState.player;
        CurrentCampaign.inventory = lastScenarioState.inventory;
        GameStateDelegates.OnCampaignStateUpdated(CurrentCampaign);
    }
}
