using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapNodeController : MonoBehaviour {
    public MapNode depictedNode;

    SpriteRenderer _nodeRenderer;
    SpriteRenderer NodeRenderer
    {
        get
        {
            if (_nodeRenderer == null)
            {
                _nodeRenderer = GetComponent<SpriteRenderer>();
            }
            return _nodeRenderer;
        }
    }

    Image _highlight;
    Image Highlight
    {
        get
        {
            if (_highlight == null)
            {
                _highlight = transform.GetChild(0).GetComponent<Image>();
            }

            return _highlight;
        }
    }

    Dictionary<MapNode, LineRenderer> pathsToChildren;

    private void Awake()
    {
        pathsToChildren = new Dictionary<MapNode, LineRenderer>();
        DeactivateHighlight();
    }

    #region data manip
    public Vector2 GetPosition()
    {
        Vector3 position = transform.position;
        return position;
    }

    public void AddPath(MapNode child, LineRenderer path)
    {
        pathsToChildren[child] = path;
    }

    public LineRenderer GetPath(MapNode child)
    {
        return pathsToChildren[child];
    }
    #endregion

    #region visuals
    public void UpdateDepictedNode(MapNode _depictedNode)
    {
        depictedNode = _depictedNode;

        NodeRenderer.sprite = CampaignImageManager.GetMapNodeImage(depictedNode.nodeType);
    }

    public void DeactivateHighlight()
    {
        Highlight.gameObject.SetActive(false);
    }

    public void UpdateHighlight(Color newColor)
    {
        Highlight.gameObject.SetActive(true);
        Highlight.color = newColor;
    }
    #endregion
}
