using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampaignState {

    public Inventory inventory;
    public Map currentMap;
    public EntityData player;

    private CampaignState ( ) { }

    public CampaignState(Inventory _inventory, Map _currentMap, EntityData _player)
    {
        inventory = _inventory;
        currentMap = _currentMap;
        player = _player;
    }
}
