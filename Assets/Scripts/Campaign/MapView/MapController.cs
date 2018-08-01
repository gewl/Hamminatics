using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviour {

    Image mapBackgroundImage;
    float height, width, mapNodeXVariance;

    [SerializeField]
    GameObject mapNodePrefab;
    [SerializeField]
    GameObject mapPathPrefab;

    [SerializeField]
    Color playerNodeColor;
    [SerializeField]
    Color potentialDestinationColor;
    [SerializeField]
    Color potentialPathColor;

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
        float halfHeight = entranceRect.rect.height / 2f;
        entranceRect.anchorMin = entrancePositioning;
        entranceRect.anchorMax = entrancePositioning;
        entranceRect.anchoredPosition = new Vector2(0f, halfHeight);

        mapNodeXVariance = (width - (entranceRect.rect.width * 1.5f)) / 2f;

        MapNode exit = map.nodeLayers[numberOfLayers - 1][0];
        GameObject exitNode = Instantiate(mapNodePrefab, transform);
        exit.AssociateNodeWithController(exitNode.GetComponent<MapNodeController>());
        RectTransform exitRect = exitNode.GetComponent<RectTransform>();
        Vector2 exitPositioning = new Vector2(0.5f, 1);
        exitRect.anchorMin = exitPositioning;
        exitRect.anchorMax = exitPositioning;
        exitRect.anchoredPosition = new Vector2(0f, -halfHeight);

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

                foreach (MapNode parent in node.parents)
                {
                    GameObject pathToNode = Instantiate(mapPathPrefab, transform);
                    LineRenderer pathRenderer = pathToNode.GetComponent<LineRenderer>();
                    pathRenderer.SetPosition(0, new Vector3(newNodeRect.position.x, newNodeRect.position.y, 0f));
                    Vector3 parentPosition = parent.NodeController.GetPosition();
                    pathRenderer.SetPosition(1, new Vector3(parentPosition.x, parentPosition.y, 0f));

                    parent.NodeController.AddPath(node, pathRenderer);
                }
            }
        }

        foreach (MapNode parent in exit.parents)
        {
            GameObject pathToNode = Instantiate(mapPathPrefab, transform);
            LineRenderer pathRenderer = pathToNode.GetComponent<LineRenderer>();
            Vector2 exitNodePosition = exit.NodeController.GetPosition();
            pathRenderer.SetPosition(0, new Vector3(exitNodePosition.x, exitNodePosition.y, 0f));
            Vector3 parentPosition = parent.NodeController.GetPosition();
            pathRenderer.SetPosition(1, new Vector3(parentPosition.x, parentPosition.y, 0f));

            parent.NodeController.AddPath(exit, pathRenderer);
        }
    }

    public void UpdateMapState(Map map)
    {
        map.nodeLayers.ForEach(layer => layer.ForEach(node => node.NodeController.DeactivateHighlight()));

        MapNode currentPlayerNode = map.currentPlayerNode;
        currentPlayerNode.NodeController.UpdateHighlight(playerNodeColor);
        foreach (MapNode child in currentPlayerNode.children)
        {
            child.NodeController.UpdateHighlight(potentialDestinationColor);
            LineRenderer pathToChild = currentPlayerNode.NodeController.GetPath(child);
            pathToChild.startColor = potentialPathColor;
            pathToChild.endColor = potentialPathColor;
        }

    }
}
