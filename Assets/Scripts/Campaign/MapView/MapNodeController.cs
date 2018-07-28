using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapNodeController : MonoBehaviour {
    public MapNode depictedNode;
    Text text;

    public void UpdateDepictedNode(MapNode _depictedNode)
    {
        depictedNode = _depictedNode;

        text = GetComponentInChildren<Text>();
        text.text = depictedNode.nodeType.ToString();
    }
}
