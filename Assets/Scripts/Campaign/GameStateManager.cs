using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour {
    const string PLAYER_ID = "Player";

    public static CampaignState currentCampaign;

    [SerializeField]
    MapController mapController;
    [SerializeField]
    ScenarioStateManager scenarioManager;

    private void Start()
    {
        EntityData player = DataManager.GetEntityData(PLAYER_ID);
        int mapLayerCount = 6;

        currentCampaign = new CampaignState(new Inventory(), new Map(0, mapLayerCount), player);

        InitializeMap();
    }

    void InitializeMap()
    {
        Map currentMap = currentCampaign.currentMap;
        mapController.DrawMap(currentMap);
        mapController.UpdateMapState(currentCampaign);
    }

    public void UpdatePlayerPosition(MapNode newPlayerNode)
    {
        currentCampaign.UpdatePlayerNode(newPlayerNode);
        mapController.UpdateMapState(currentCampaign);

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
}
