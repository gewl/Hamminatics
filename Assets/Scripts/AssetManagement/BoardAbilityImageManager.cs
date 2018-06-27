using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardAbilityImageManager : MonoBehaviour {

    [SerializeField]
    ActionStackController actionStackController;
    [SerializeField]
    BoardController boardController;

    private void Awake()
    {
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

        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        for (int i = 0; i < actions.Count; i++)
        {
            Action action = actions[i];
            GameObject newActionImage = GenerateNewActionImage(action);
        }
    }

    GameObject GenerateNewActionImage(Action action)
    {
        GameObject instantiatedActionImage = ImageManager.GetAbilityPointer(action.card.Category, action.direction);

        instantiatedActionImage.transform.SetParent(this.transform);
        Vector2 cellEdgePosition = boardController.GetCellEdgePosition(action.entity.Position, action.direction);
        instantiatedActionImage.transform.position = cellEdgePosition;

        return instantiatedActionImage;
    }

}
