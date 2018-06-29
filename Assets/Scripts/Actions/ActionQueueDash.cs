using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionQueueDash : MonoBehaviour {

    [SerializeField]
    ActionStackController actionStack;

    QueuedActionController[] queuedActionControllers;
    Image[] actionBackgroundImages;

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
        int actionIndex = queuedAction.GetSiblingIndex();
        ResetBudgeTracker(actionIndex);
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

        int newPlayerActionIndex = 0;
        for (int i = 0; i < queuedActionControllers.Length; i++)
        {
            Transform actionTransform = queuedActionControllers[i].transform;

            if (actionTransform == draggingAction)
            {
                continue;
            }

            if (!actionTransform.gameObject.activeSelf || actionTransform.position.x > draggingAction.position.x)
            {
                newPlayerActionIndex = i;
                break;
            }
        }

        actionStack.ChangePlayerActionPosition(newPlayerActionIndex);

        draggingAction = null;

        layoutGroup.enabled = true;
    }

    void ResetBudgeTracker(int actionIndex)
    {
        for (int i = 0; i < actionIndex; i++)
        {
            actionsAreBudged[i] = true;
            queuedActionControllers[i].RecalculateBudgedPosition(true);
        }
        for (int i = actionIndex; i < actionsAreBudged.Length; i++)
        {
            actionsAreBudged[i] = false;
            queuedActionControllers[i].RecalculateBudgedPosition(false);
        }
    }
    #endregion
}
