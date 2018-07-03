using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardAbilityImageManager : MonoBehaviour {

    [SerializeField]
    TurnStackController actionStackController;
    [SerializeField]
    BoardController boardController;

    private void Awake()
    {
    }

    private void OnEnable()
    {
        actionStackController.OnTurnStackUpdate += DrawTurns;            
    }

    private void OnDisable()
    {
        actionStackController.OnTurnStackUpdate -= DrawTurns;
    }

    void DrawTurns(List<Turn> turns)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < turns.Count; i++)
        {
            Turn turn = turns[i];
            GenerateNewActionImage(turn.FirstAction, turn.FirstAction.entity.Position, turn.FirstAction.direction);
        }
    }

    void GenerateNewActionImage(Action action, Vector2Int position, Direction direction)
    {
        GameObject instantiatedActionImage = ImageManager.GetAbilityPointer(action.card.Category, action.direction);

        instantiatedActionImage.transform.SetParent(transform);
        Vector2 cellEdgePosition = boardController.GetCellEdgePosition(position, direction);
        instantiatedActionImage.transform.position = cellEdgePosition;
    }

}
