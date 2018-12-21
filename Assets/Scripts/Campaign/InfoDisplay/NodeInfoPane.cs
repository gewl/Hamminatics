using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class NodeInfoPane : MonoBehaviour {

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
                return "The staircase you came down. You won't be going back that way.";
            case MapNodeType.Scenario:
                return "Ghouls, goblins, gold. Take and receive damage. Use mechanics for your pleasure.";
            case MapNodeType.Event:
                return "Could be good, could be bad. But one thing's for certain. It'll be good or bad.";
            case MapNodeType.Store:
                return "The fundament of commerce...the humble small business.";
            case MapNodeType.End:
                return "A staircase leading downward. The only apparent exit.";
            case MapNodeType.Boss:
                return "One of the guardians of this place.";
            default:
                Debug.LogError("Incompatible nodetype.");
                return "placeholder";
        }
    }

    public void MoveToNode()
    {
        campaignManager.UpdatePlayerPosition(selectedMapNode);
        selectedMapNode = null;
        DeactivatePane();
    }
}
