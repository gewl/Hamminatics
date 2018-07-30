﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviour {

    Image mapBackgroundImage;
    float height, width, mapNodeXVariance;

    [SerializeField]
    GameObject mapNodePrefab;

    private void Awake()
    {
        mapBackgroundImage = GetComponent<Image>();
        height = mapBackgroundImage.rectTransform.rect.height;
        width = mapBackgroundImage.rectTransform.rect.width;
    }

    public void DrawMap(Map map)
    {
        int numberOfLayers = map.nodeLayers.Count;
        float layerPixelHeight = height / (numberOfLayers - 1);

        MapNode entrance = map.nodeLayers[0][0];
        GameObject entranceNode = Instantiate(mapNodePrefab, transform);
        entrance.AssociateNodeWithController(entranceNode.GetComponent<MapNodeController>());
        RectTransform entranceRect = entranceNode.GetComponent<RectTransform>();
        Vector2 entrancePositioning = new Vector2(0.5f, 0f);
        entranceRect.anchorMin = entrancePositioning;
        entranceRect.anchorMax = entrancePositioning;
        entranceRect.pivot = entrancePositioning;

        mapNodeXVariance = (width - (entranceRect.rect.width * 1.5f)) / 2f;

        MapNode exit = map.nodeLayers[numberOfLayers - 1][0];
        GameObject exitNode = Instantiate(mapNodePrefab, transform);
        exit.AssociateNodeWithController(exitNode.GetComponent<MapNodeController>());
        RectTransform exitRect = exitNode.GetComponent<RectTransform>();
        Vector2 exitPositioning = new Vector2(0.5f, 1);
        exitRect.anchorMin = exitPositioning;
        exitRect.anchorMax = exitPositioning;
        exitRect.pivot = exitPositioning;

        float halfHeight = entranceRect.GetComponent<Image>().rectTransform.rect.height / 2f;
        float baseLayerYCoord = entranceRect.localPosition.y + halfHeight;
        float topLayerYCoord = exitRect.localPosition.y - halfHeight;
        float totalYHeightOfNodes = Mathf.Abs(topLayerYCoord - baseLayerYCoord);
        float yGapBetweenLayers = Mathf.Abs(totalYHeightOfNodes / (numberOfLayers - 1));

        // For the time being, just going to stack nodes in layer rightward from leftmost valid point.
        // TODO: Prettify
        float firstLayerXPosition = entranceRect.localPosition.x - mapNodeXVariance; 
        for (int i = 1; i < numberOfLayers - 1; i++)
        {
            List<MapNode> layer = map.nodeLayers[i];
            int nodesInLayer = layer.Count;

            for (int j = 0; j < nodesInLayer; j++)
            {
                MapNode node = layer[j];
                GameObject drawnNode = Instantiate(mapNodePrefab, transform);
                node.AssociateNodeWithController(drawnNode.GetComponent<MapNodeController>());
                RectTransform newNodeRect = drawnNode.GetComponent<RectTransform>();
                newNodeRect.localPosition = new Vector2(mapNodeXVariance * (j - 1), yGapBetweenLayers * i + baseLayerYCoord);
            }
        }
    }
}
