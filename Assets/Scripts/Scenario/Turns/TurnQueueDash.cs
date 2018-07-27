using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class TurnQueueDash : MonoBehaviour {

    [SerializeField]
    TurnStackController turnStack;

    List<Turn> currentlyDepictedTurns;

    QueuedTurnController[] queuedTurnController;
    Image[] turnImages;
    Outline[] turnOutlines;

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
        turnImages = new Image[queuedTurnController.Length];
        turnOutlines = new Outline[queuedTurnController.Length];

        for (int i = 0; i < queuedTurnController.Length; i++)
        {
            turnImages[i] = queuedTurnController[i].GetComponent<Image>();
            turnOutlines[i] = queuedTurnController[i].GetComponent<Outline>();
        }

        turnsAreBudged = new bool[queuedTurnController.Length];

        layoutGroup = GetComponent<LayoutGroup>();
    }

    private void OnEnable()
    {
        turnStack.OnTurnStackUpdate += UpdateTurnQueueDash;
        ScenarioStateDelegates.OnResolvingTurn += UpdateCurrentlyResolvingTurn;
        ScenarioStateDelegates.OnEntitySelected += UpdateSelectedEntityTurn;
        ScenarioStateDelegates.ReturnToDefaultBoard += OnDeselectEntity;
    }

    private void OnDisable()
    {
        turnStack.OnTurnStackUpdate -= UpdateTurnQueueDash;
        ScenarioStateDelegates.OnResolvingTurn -= UpdateCurrentlyResolvingTurn;
        ScenarioStateDelegates.OnEntitySelected -= UpdateSelectedEntityTurn;
        ScenarioStateDelegates.ReturnToDefaultBoard -= OnDeselectEntity;
    }

    void UpdateSelectedEntityTurn(EntityData entity, ScenarioState currentGameState, List<ProjectedGameState> upcomingStates)
    {
        int turnIndex = currentlyDepictedTurns.FindIndex(t => t.Entity == entity);
        turnOutlines[turnIndex].effectColor = Color.yellow;
    }

    void UpdateCurrentlyResolvingTurn(Turn turn)
    {
        ResetTurnOutlines();
        int turnIndex = currentlyDepictedTurns.FindIndex(t => t == turn);

        turnOutlines[turnIndex].effectColor = Color.yellow;
    }

    void OnDeselectEntity(ScenarioState currentState, List<ProjectedGameState> upcomingStates)
    {
        ResetTurnOutlines();
    }

    void ResetTurnOutlines()
    {
        for (int i = 0; i < turnOutlines.Length; i++)
        {
            turnOutlines[i].effectColor = Color.black;
        }
    }

    void UpdateTurnQueueDash(List<Turn> turns)
    {
        ResetTurnOutlines();
        currentlyDepictedTurns = turns;

        for (int i = 0; i < turns.Count; i++)
        {
            queuedTurnController[i].gameObject.SetActive(true);
            queuedTurnController[i].UpdateDepictedTurn(turns[i]);

            turnImages[i].color = turns[i].Entity.IdentifyingColor;
            if (turns[i].Entity.ID == Constants.PLAYER_ID)
            {
                queuedTurnController[i].isPlayerTurn = true;
            }
            else
            {
                queuedTurnController[i].isPlayerTurn = false;
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

        for (int i = 0; i < turnImages.Length; i++)
        {
            Image turnBackgroundImage = turnImages[i];
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
