using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampaignState {

    public Inventory inventory;
    public Map currentMap;
    public EntityData player;
    public MapNode CurrentPlayerNode { get; private set; }

    public List<MapNode> pastPlayerNodes;

    private CampaignState ( ) { }

    public CampaignState(Inventory _inventory, Map _currentMap, EntityData _player)
    {
        inventory = _inventory;
        player = _player;

        UpdateMap(_currentMap);
    }

    public void UpdatePlayerNode(MapNode newNode)
    {
        pastPlayerNodes.Add(CurrentPlayerNode);
        CurrentPlayerNode = newNode;
    }

    public void UpdateMap(Map newMap)
    {
        currentMap = newMap;
        CurrentPlayerNode = currentMap.GetStarterNode();
        pastPlayerNodes = new List<MapNode>();
    }
}
