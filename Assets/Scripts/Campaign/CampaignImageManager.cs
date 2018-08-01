using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class CampaignImageManager : SerializedMonoBehaviour {

    [SerializeField]
    Dictionary<MapNodeType, Sprite> mapNodeImages;

    static CampaignImageManager instance;

    private void Awake()
    {
        instance = this;
    }

    public static Sprite GetMapNodeImage(MapNodeType nodeType)
    {
        return instance.mapNodeImages[nodeType];
    }
}
