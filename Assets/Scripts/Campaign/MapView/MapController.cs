using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class MapController : MonoBehaviour {

    Image mapBackgroundImage;
    float height, width, mapNodeXVariance;

    [SerializeField]
    GameObject mapNodePrefab;
    [SerializeField]
    GameObject mapPathPrefab;

    [SerializeField]
    MapNodeController entranceNodeController;
    [SerializeField]
    MapNodeController exitNodeController;

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
        entrance.AssociateNodeWithController(entranceNodeController);
        RectTransform entranceRect = entranceNodeController.GetComponent<RectTransform>();
        entranceNodeController.GetComponent<Button>().onClick.AddListener(GenerateMapNodeClickListener(entrance));

        float halfHeight = entranceRect.rect.height / 2f;
        mapNodeXVariance = (width - (entranceRect.rect.width * 1.5f)) / 2f;

        MapNode exit = map.nodeLayers[numberOfLayers - 1][0];
        exit.AssociateNodeWithController(exitNodeController);
        RectTransform exitRect = exitNodeController.GetComponent<RectTransform>();
        exitNodeController.GetComponent<Button>().onClick.AddListener(GenerateMapNodeClickListener(exit));

        float baseLayerYCoord = entranceRect.localPosition.y;
        float topLayerYCoord = exitRect.localPosition.y;
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
                drawnNode.GetComponent<Button>().onClick.AddListener(GenerateMapNodeClickListener(node));

                foreach (MapNode parent in node.parents)
                {
                    DrawPath(node.NodeController, parent.NodeController);
                }
            }
        }

        foreach (MapNode parent in exit.parents)
        {
            DrawPath(exit.NodeController, parent.NodeController);
        }
    }

    void DrawPath(MapNodeController targetNodeController, MapNodeController parentNodeController)
    {
        GameObject pathToNode = Instantiate(mapPathPrefab, transform);
        LineRenderer pathRenderer = pathToNode.GetComponent<LineRenderer>();

        Vector3 parentPosition = parentNodeController.GetPosition();
        pathRenderer.SetPosition(0, new Vector3(parentPosition.x, parentPosition.y, 0f));

        Vector3 nodePosition = targetNodeController.GetPosition();
        pathRenderer.SetPosition(1, new Vector3(nodePosition.x, nodePosition.y, 0f));

        parentNodeController.AddPath(targetNodeController.depictedNode, pathRenderer);
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

    void OnMapNodeClick(MapNode node)
    {
        Debug.Log("node clicked: " + node.nodeType);
        Debug.Log(node.NodeController.GetComponent<RectTransform>().rect.center);
        Debug.Log(node.NodeController.GetComponent<RectTransform>().anchoredPosition);
        Debug.Log(node.NodeController.GetComponent<RectTransform>().position);
        Debug.Log(node.NodeController.transform.position);
    }
    
    UnityAction GenerateMapNodeClickListener(MapNode node)
    {
        return () => OnMapNodeClick(node);
    }

}
