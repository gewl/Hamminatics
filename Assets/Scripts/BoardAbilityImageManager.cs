using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardAbilityImageManager : MonoBehaviour {

    [SerializeField]
    ActionStackController actionStackController;
    [SerializeField]
    ImageManager imageManager;
    [SerializeField]
    BoardController boardController;

    Dictionary<int, GameObject> actionHashToQueuedImageDict;

    private void Awake()
    {
        actionHashToQueuedImageDict = new Dictionary<int, GameObject>();
    }

    private void OnEnable()
    {
        actionStackController.OnActionStackUpdate += DrawActions;            
    }

    private void OnDisable()
    {
        actionStackController.OnActionStackUpdate -= DrawActions;
    }

    void DrawActions(List<Action> actions)
    {
        Dictionary<int, GameObject> newActionImageDict = new Dictionary<int, GameObject>();

        for (int i = 0; i < actions.Count; i++)
        {
            Action action = actions[i];
            int actionHash = action.GetHashCode();

            if (actionHashToQueuedImageDict.ContainsKey(actionHash))
            {
                newActionImageDict[actionHash] = actionHashToQueuedImageDict[actionHash];
                actionHashToQueuedImageDict.Remove(actionHash);
            }
            else
            {
                GameObject newActionImage = GenerateNewActionImage(action);
                newActionImageDict[actionHash] = newActionImage;
            }
        }

        foreach (int key in actionHashToQueuedImageDict.Keys)
        {
            GameObject.Destroy(actionHashToQueuedImageDict[key]);
        }

        actionHashToQueuedImageDict = newActionImageDict;
    }

    GameObject GenerateNewActionImage(Action action)
    {
        GameObject instantiatedActionImage = GetImageForAction(action.card.Category, action.direction);

        instantiatedActionImage.transform.SetParent(this.transform);
        Vector2 cellEdgePosition = boardController.GetCellEdgePosition(action.entity.Position, action.direction);
        instantiatedActionImage.transform.position = cellEdgePosition;

        return instantiatedActionImage;
    }

    GameObject GetImageForAction(CardCategory cardCategory, Direction direction)
    {
        switch (cardCategory)
        {
            case CardCategory.Movement:
                return GameObject.Instantiate(imageManager.GetDirectionImage(direction));
            case CardCategory.Attack:
                return GameObject.Instantiate(imageManager.GetDirectionImage(direction));
            default:
                return null;
        }
    }


}
