﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapNode {

    public int layer;
    public MapNodeType nodeType;
    public HashSet<MapNode> parents;
    public HashSet<MapNode> children;

    MapNodeController nodeController;

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

    public void AssociateNodeWithController(MapNodeController controller)
    {
        nodeController = controller;
        nodeController.UpdateDepictedNode(this);
    }

    public Vector2 GetPosition()
    {
        Vector3 position = nodeController.transform.position;
        switch (nodeType)
        {
            case MapNodeType.Start:
                position.y += 0.5f;
                return position;
            case MapNodeType.End:
                position.y -= 0.5f;
                return position;
            default:
                return position;
        }
    }
}
