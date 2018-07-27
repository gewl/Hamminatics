using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapNode {

    public int layer;
    public MapNodeType nodeType;
    public HashSet<MapNode> parents;
    public HashSet<MapNode> children;

    private MapNode() { }

    public MapNode(MapNodeType _nodeType, int _layer)
    {
        nodeType = _nodeType;
        layer = _layer;
        children = new HashSet<MapNode>();
        parents = new HashSet<MapNode>();
    }

    public void AddChild(MapNode child)
    {
        children.Add(child);
        child.parents.Add(this);
    }

    public void AddParent(MapNode parent)
    {
        parents.Add(parent);
        parent.children.Add(this);
    }
}
