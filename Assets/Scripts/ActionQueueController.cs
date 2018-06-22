using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionQueueController : MonoBehaviour {

    Stack<Action> actionStack;
    public bool IsActionStackEmpty { get { return actionStack.Count == 0; } }

    [SerializeField]
    Button endTurnButton;

    private void Awake()
    {
        actionStack = new Stack<Action>();
        endTurnButton.interactable = false;
    }

    public void AddNewAction(CardData card, EntityData entity, Direction direction, int distance)
    {
        Action newAction = new Action(card, entity, direction, distance);

        actionStack.Push(newAction);
        endTurnButton.interactable = true;
    }

    public Action GetNextAction()
    {
        Action nextAction = actionStack.Pop();

        endTurnButton.interactable = !IsActionStackEmpty;

        return nextAction;
    }

}
