using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CampaignInfoPane : MonoBehaviour {

    MapNode selectedMapNode;
    [SerializeField]
    GameStateManager campaignManager;
    [SerializeField]
    Text infoTitle;
    [SerializeField]
    Text infoBody;
    [SerializeField]
    Button moveButton;

    public void HandleMapNodeClick(MapNode clickedNode, MapNode currentPlayerNode)
    {
        if (selectedMapNode == clickedNode)
        {
            DeactivatePane();
        }
        else
        {
            DisplayNodeInfo(clickedNode, currentPlayerNode);
        }
    }

    public void DeactivatePane()
    {
        selectedMapNode = null;
        gameObject.SetActive(false);
    }

    void DisplayNodeInfo(MapNode selectedNode, MapNode currentPlayerNode)
    {
        selectedMapNode = selectedNode;
        moveButton.interactable = currentPlayerNode.children.Any(child => child == selectedNode);
        gameObject.SetActive(true);
        infoTitle.text = selectedNode.nodeType.ToString();
        infoBody.text = GetMapNodeText(selectedNode.nodeType);
    }

    string GetMapNodeText(MapNodeType nodeType)
    {
        switch (nodeType)
        {
            case MapNodeType.Start:
                return "The entrance to the zone.";
            case MapNodeType.Scenario:
                return "A scenario.";
            case MapNodeType.Event:
                return "An event.";
            case MapNodeType.Store:
                return "A store.";
            case MapNodeType.End:
                return "The exit from the zone.";
            default:
                Debug.LogError("Incompatible nodetype.");
                return "placeholder";
        }
    }

    public void MoveToNode()
    {
        campaignManager.UpdatePlayerPosition(selectedMapNode);
    }
}
