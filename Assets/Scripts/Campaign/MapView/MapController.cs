using System.Collections;
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

        GameObject entranceNode = Instantiate(mapNodePrefab, transform);
        entranceNode.GetComponent<MapNodeController>().UpdateDepictedNode(map.nodeLayers[0][0]);
        RectTransform entranceRect = entranceNode.GetComponent<RectTransform>();
        Vector2 entrancePositioning = new Vector2(0.5f, 0f);
        entranceRect.anchorMin = entrancePositioning;
        entranceRect.anchorMax = entrancePositioning;
        entranceRect.pivot = entrancePositioning;

        mapNodeXVariance = (width - (entranceRect.rect.width * 1.5f)) / 2f;

        GameObject exitNode = Instantiate(mapNodePrefab, transform);
        exitNode.GetComponent<MapNodeController>().UpdateDepictedNode(map.nodeLayers[map.nodeLayers.Count - 1][0]);
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

        for (int i = 1; i < numberOfLayers - 1; i++)
        {
            List<MapNode> layer = map.nodeLayers[i];
            int nodesInLayer = layer.Count;
        }
    }
}
