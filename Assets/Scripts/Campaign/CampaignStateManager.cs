﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampaignStateManager : MonoBehaviour {
    const string PLAYER_ID = "Player";

    public static CampaignState currentCampaign;

    [SerializeField]
    MapController mapController;

    private void Awake()
    {
        EntityData player = DataManager.GetEntityData(PLAYER_ID);
        int mapLayerCount = 6;

        currentCampaign = new CampaignState(new Inventory(), new Map(0, mapLayerCount), player);

        InitializeMap();
    }

    void InitializeMap()
    {
        mapController.DrawMap(currentCampaign.currentMap);
    }
}
