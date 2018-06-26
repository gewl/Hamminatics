using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionQueueDash : MonoBehaviour {

    [SerializeField]
    ActionStackController actionStack;

    QueuedActionController[] queuedActionControllers;
    Image[] actionBackgroundImages;

    RectTransform rect;
    LayoutGroup layoutGroup;
    bool[] actionsAreBudged;

    [SerializeField]
    Color playerActionColor;
    [SerializeField]
    Color enemyActionColor;

    Transform draggingAction;

    private void Awake()
    {
        queuedActionControllers = GetComponentsInChildren<QueuedActionController>();
        actionBackgroundImages = new Image[queuedActionControllers.Length];

        for (int i = 0; i < queuedActionControllers.Length; i++)
        {
            actionBackgroundImages[i] = queuedActionControllers[i].GetComponent<Image>();
        }

        actionsAreBudged = new bool[queuedActionControllers.Length];

        rect = GetComponent<RectTransform>();
        layoutGroup = GetComponent<LayoutGroup>();
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

            if (actions[i].entity.ID == Constants.PLAYER_ID)
            {
                queuedActionControllers[i].isPlayerAction = true;
                actionBackgroundImages[i].color = playerActionColor;
            }
            else
            {
                queuedActionControllers[i].isPlayerAction = false;
                actionBackgroundImages[i].color = enemyActionColor;
            }
        }

        for (int j = actions.Count; j < queuedActionControllers.Length; j++)
        {
            queuedActionControllers[j].gameObject.SetActive(false);
        }
    }

    #region Queued action event handlers
    public void OnQueuedActionBeginDrag(Transform queuedAction)
    {
        ResetBudgeTracker();
        draggingAction = queuedAction;
        layoutGroup.enabled = false;
    }

    public void OnQueuedActionDrag()
    {
        float draggingActionX = draggingAction.position.x;

        for (int i = 0; i < actionBackgroundImages.Length; i++)
        {
            Image actionBackgroundImage = actionBackgroundImages[i];
            if (actionBackgroundImage.transform == draggingAction)
            {
                continue; 
            }
            if (!actionBackgroundImage.IsActive())
            {
                break;
            }

            if (!actionsAreBudged[i] && actionBackgroundImage.transform.position.x < draggingActionX)
            {
                queuedActionControllers[i].ToggleBudgedStatus(true);
                actionsAreBudged[i] = true;
            }
            else if (actionsAreBudged[i] && actionBackgroundImage.transform.position.x > draggingActionX)
            {
                queuedActionControllers[i].ToggleBudgedStatus(false);
                actionsAreBudged[i] = false;
            }
        }
    }

    public void OnQueuedActionDrop()
    {
        for (int i = 0; i < queuedActionControllers.Length; i++)
        {
            queuedActionControllers[i].OnOtherActionDragEnded();
        }

        // If any of the non-player actions are budged, shuffle player action between last budged
        // & first un-budged actions. Checking first two because one of them could be player action,
        // and actually finding the player action's index is way more expensive.
        if (actionsAreBudged[0] || actionsAreBudged[1])
        {
            int lastBudgedActionIndex = 0, peekedActionIndex = 1;
            while (peekedActionIndex < actionsAreBudged.Length && actionsAreBudged[peekedActionIndex])
            {
                lastBudgedActionIndex = peekedActionIndex;
                peekedActionIndex++;
            }

            actionStack.ChangePlayerActionPosition(lastBudgedActionIndex);
        }
        draggingAction = null;

        layoutGroup.enabled = true;
    }

    void ResetBudgeTracker()
    {
        for (int i = 0; i < actionsAreBudged.Length; i++)
        {
            actionsAreBudged[i] = false;
        }
    }
    #endregion
}
