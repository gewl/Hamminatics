using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionQueueDash : MonoBehaviour {

    [SerializeField]
    ActionStackController actionStack;

    QueuedActionController[] queuedActionControllers;

    private void Awake()
    {
        queuedActionControllers = GetComponentsInChildren<QueuedActionController>();
    }

    private void OnEnable()
    {
        actionStack.OnActionStackUpdate += UpdateActionQueueDash;
    }

    private void OnDisable()
    {
        actionStack.OnActionStackUpdate -= UpdateActionQueueDash;
    }

    void UpdateActionQueueDash(List<Action> actions)
    {
        for (int i = 0; i < actions.Count; i++)
        {
            queuedActionControllers[i].gameObject.SetActive(true);
            queuedActionControllers[i].UpdateDepictedAction(actions[i]);
        }

        for (int j = actions.Count; j < queuedActionControllers.Length; j++)
        {
            queuedActionControllers[j].gameObject.SetActive(false);
        }
    }
}
