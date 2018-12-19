using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Map {
    public int depth;

    public List<List<MapNode>> nodeLayers;
    Random rand;

    // Node generation values.
    // If randomly generated number (1-100) is less than these values, that node is a Scenario,
    // otherwise it's an Event. "Second" and "third" refers to *on that layer*. First is always Scenario.
    int secondNodeScenarioChance = 60;
    int thirdNodeScenarioChance = 40;

    private Map() { }
    public Map(int _depth)
    {
        depth = _depth;
        rand = new Random();

        if (BossInfo.bossDepths.Contains(depth))
        {
            nodeLayers = GenerateBossLayers(depth);
        }
        else
        {
            int layerCount = rand.Next(6, 9);
            nodeLayers = GenerateNormalLayers(layerCount);
        }
    }

    List<List<MapNode>> GenerateNormalLayers(int numberOfLayers)
    {
        List<List<MapNode>> layers = new List<List<MapNode>>();

        MapNode starterNode = new MapNode(MapNodeType.Start, 0);

        // Every map starts with a layer of one (starter) node.
        layers.Add(new List<MapNode>() { starterNode });

        for (int i = 1; i < numberOfLayers - 1; i++)
        {
            int numberOfNodesInLayer = rand.Next(2, 4);
            List<MapNode> nodesThisLayer = new List<MapNode>();

            for (int j = 0; j < numberOfNodesInLayer; j++)
            {
                MapNode nextNode = GenerateNode(i, j);
                nodesThisLayer.Add(nextNode);
            }

            nodesThisLayer.Shuffle();
            layers.Add(nodesThisLayer);
        }

        for (int layer = 0; layer < layers.Count; layer++)
        {
            List<MapNode> thisLayer = layers[layer];
            thisLayer.ForEach(node =>
            {
                if (layer < layers.Count - 1)
                {
                    List<MapNode> nextLayer = layers[layer + 1];
                    node.AddChild(nextLayer.GetRandomElement());
                }
                if (layer != 0 && node.parents.Count == 0)
                {
                    List<MapNode> lastLayer = layers[layer - 1];
                    node.AddParent(lastLayer.GetRandomElement());
                }
            });
        }

        MapNode endNode = new MapNode(MapNodeType.End, numberOfLayers - 1);
        List<MapNode> previousLayer = layers[numberOfLayers - 2];
        previousLayer.ForEach(terminalPathNode => endNode.AddParent(terminalPathNode));
        layers.Add(new List<MapNode>() { endNode });

        // Place store randomly in one of two final (non-exit) layers.
        if (numberOfLayers > 2)
        {
            int layersCount = layers.Count;
            int shopLayer = rand.Next(layersCount - 3, layersCount - 2);
            layers[shopLayer].GetRandomElement<MapNode>().nodeType = MapNodeType.Store;
        }

        return layers;
    }

    List<List<MapNode>> GenerateBossLayers(int depth)
    {
        List<List<MapNode>> layers = new List<List<MapNode>>();

        MapNode starterNode = new MapNode(MapNodeType.Start, 0);
        // Every map starts with a layer of one (starter) node.
        layers.Add(new List<MapNode>() { starterNode });

        MapNode bossNode = new MapNode(MapNodeType.Boss, 1);
        layers.Add(new List<MapNode>() { bossNode });

        starterNode.AddChild(bossNode);

        return layers;
    }

    MapNode GenerateNode(int layer, int index)
    {
        if (index == 0)
        {
            return new MapNode(MapNodeType.Scenario, layer);
        }
        else if (index == 1)
        {
            int typeDeterminant = rand.Next(1, 101);
            MapNodeType thisNodeType = MapNodeType.Event;
            if (typeDeterminant < secondNodeScenarioChance)
            {
                thisNodeType = MapNodeType.Scenario;
            }

            return new MapNode(thisNodeType, layer);
        }
        else
        {
            int typeDeterminant = rand.Next(1, 101);
            MapNodeType thisNodeType = MapNodeType.Event;
            if (typeDeterminant < thirdNodeScenarioChance)
            {
                thisNodeType = MapNodeType.Scenario;
            }

            return new MapNode(thisNodeType, layer);
        }
    }

    public MapNode GetStarterNode()
    {
        return nodeLayers[0][0];
    }

    // Return node layer's distance 'through' map as a percentage, for e.g. scaling difficulty
    // as player progresses in map.
    public float GetNodeDistance(MapNode node)
    {
        return (float)nodeLayers.FindIndex(layer => layer.Contains(node)) / (float)nodeLayers.Count;
    }
}
