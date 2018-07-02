using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnQueueDash : MonoBehaviour {

    [SerializeField]
    TurnStackController turnStack;

    QueuedTurnController[] queuedTurnController;
    Image[] turnBackgroundImages;

    LayoutGroup layoutGroup;
    bool[] turnsAreBudged;

    [SerializeField]
    Color playerTurnColor;
    [SerializeField]
    Color enemyTurnColor;

    Transform draggingTurn;

    private void Awake()
    {
        queuedTurnController = GetComponentsInChildren<QueuedTurnController>();
        turnBackgroundImages = new Image[queuedTurnController.Length];

        for (int i = 0; i < queuedTurnController.Length; i++)
        {
            turnBackgroundImages[i] = queuedTurnController[i].GetComponent<Image>();
        }

        turnsAreBudged = new bool[queuedTurnController.Length];

        layoutGroup = GetComponent<LayoutGroup>();
    }

    private void OnEnable()
    {
        turnStack.OnTurnStackUpdate += UpdateTurnQueueDash;
    }

    private void OnDisable()
    {
        turnStack.OnTurnStackUpdate -= UpdateTurnQueueDash;
    }

    void UpdateTurnQueueDash(List<Turn> turns)
    {
        for (int i = 0; i < turns.Count; i++)
        {
            queuedTurnController[i].gameObject.SetActive(true);
            queuedTurnController[i].UpdateDepictedTurn(turns[i]);

            if (turns[i].Entity.ID == Constants.PLAYER_ID)
            {
                queuedTurnController[i].isPlayerTurn = true;
                turnBackgroundImages[i].color = playerTurnColor;
            }
            else
            {
                queuedTurnController[i].isPlayerTurn = false;
                turnBackgroundImages[i].color = enemyTurnColor;
            }
        }

        for (int j = turns.Count; j < queuedTurnController.Length; j++)
        {
            queuedTurnController[j].gameObject.SetActive(false);
        }
    }

    #region Queued action event handlers
    public void OnQueuedTurnBeginDrag(Transform queuedTurn)
    {
        int turnIndex = queuedTurn.GetSiblingIndex();
        ResetBudgeTracker(turnIndex);
        draggingTurn = queuedTurn;
        layoutGroup.enabled = false;
    }

    public void OnQueuedTurnDrag()
    {
        float draggingTurnX = draggingTurn.position.x;

        for (int i = 0; i < turnBackgroundImages.Length; i++)
        {
            Image turnBackgroundImage = turnBackgroundImages[i];
            if (turnBackgroundImage.transform == draggingTurn)
            {
                continue; 
            }
            if (!turnBackgroundImage.IsActive())
            {
                break;
            }

            if (!turnsAreBudged[i] && turnBackgroundImage.transform.position.x < draggingTurnX)
            {
                queuedTurnController[i].ToggleBudgedStatus(true);
                turnsAreBudged[i] = true;
            }
            else if (turnsAreBudged[i] && turnBackgroundImage.transform.position.x > draggingTurnX)
            {
                queuedTurnController[i].ToggleBudgedStatus(false);
                turnsAreBudged[i] = false;
            }
        }
    }

    public void OnQueuedTurnDrop()
    {
        for (int i = 0; i < queuedTurnController.Length; i++)
        {
            queuedTurnController[i].OnOtherTurnDragEnded();
        }

        int newPlayerTurnIndex = 0;
        for (int i = 0; i < queuedTurnController.Length; i++)
        {
            Transform turnTransform = queuedTurnController[i].transform;

            if (turnTransform == draggingTurn)
            {
                continue;
            }

            if (!turnTransform.gameObject.activeSelf || turnTransform.position.x > draggingTurn.position.x)
            {
                newPlayerTurnIndex = i;
                break;
            }
        }

        turnStack.ChangePlayerTurnPosition(newPlayerTurnIndex);

        draggingTurn = null;

        layoutGroup.enabled = true;
    }

    void ResetBudgeTracker(int turnIndex)
    {
        for (int i = 0; i < turnIndex; i++)
        {
            turnsAreBudged[i] = true;
            queuedTurnController[i].RecalculateBudgedPosition(true);
        }
        for (int i = turnIndex; i < turnsAreBudged.Length; i++)
        {
            turnsAreBudged[i] = false;
            queuedTurnController[i].RecalculateBudgedPosition(false);
        }
    }
    #endregion
}
