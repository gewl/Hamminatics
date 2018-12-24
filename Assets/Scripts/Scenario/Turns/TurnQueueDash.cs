using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class TurnQueueDash : MonoBehaviour {

    [SerializeField]
    TurnStackController turnStack;

    CanvasScaler canvasScaler;

    List<Turn> currentlyDepictedTurns;

    QueuedTurnController[] queuedTurnControllers;
    Image[] turnImages;
    Outline[] turnOutlines;

    LayoutGroup layoutGroup;
    bool[] turnsAreBudged;

    [SerializeField]
    Color playerTurnColor;
    [SerializeField]
    Color enemyTurnColor;

    Transform draggingTurn;

    #region lifecycle
    private void Awake()
    {
        canvasScaler = GetComponentInParent<CanvasScaler>();

        queuedTurnControllers = GetComponentsInChildren<QueuedTurnController>();
        turnImages = new Image[queuedTurnControllers.Length];
        turnOutlines = new Outline[queuedTurnControllers.Length];

        for (int i = 0; i < queuedTurnControllers.Length; i++)
        {
            turnImages[i] = queuedTurnControllers[i].GetComponent<Image>();
            turnOutlines[i] = queuedTurnControllers[i].GetComponent<Outline>();
        }

        turnsAreBudged = new bool[queuedTurnControllers.Length];

        layoutGroup = GetComponent<LayoutGroup>();
    }

    private void OnEnable()
    {
        turnStack.OnTurnStackUpdate += UpdateTurnQueueDash;
        GameStateDelegates.OnResolvingState += UpdateCurrentlyResolvingState;
        GameStateDelegates.OnEntitySelected += UpdateSelectedEntityTurn;
        GameStateDelegates.ReturnToDefaultBoard += OnDeselectEntity;
    }

    private void OnDisable()
    {
        turnStack.OnTurnStackUpdate -= UpdateTurnQueueDash;
        GameStateDelegates.OnResolvingState -= UpdateCurrentlyResolvingState;
        GameStateDelegates.OnEntitySelected -= UpdateSelectedEntityTurn;
        GameStateDelegates.ReturnToDefaultBoard -= OnDeselectEntity;
    }
    #endregion

    #region handle state changes
    void UpdateSelectedEntityTurn(EntityData entity, ScenarioState currentGameState, List<ProjectedGameState> upcomingStates)
    {
        int turnIndex = currentlyDepictedTurns.FindIndex(t => t.Entity == entity);
        turnOutlines[turnIndex].effectColor = Color.yellow;
    }

    void UpdateCurrentlyResolvingState(ProjectedGameState projectedState)
    {
        ResetTurnOutlines();
        int turnIndex = currentlyDepictedTurns.FindIndex(t => t.Entity == projectedState.activeEntity);

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
            queuedTurnControllers[i].gameObject.SetActive(true);
            queuedTurnControllers[i].UpdateDepictedTurn(turns[i]);

            turnImages[i].color = turns[i].Entity.IdentifyingColor;
            if (turns[i].Entity.ID == Constants.PLAYER_ID)
            {
                queuedTurnControllers[i].isPlayerTurn = true;
            }
            else
            {
                queuedTurnControllers[i].isPlayerTurn = false;
            }
        }

        for (int j = turns.Count; j < queuedTurnControllers.Length; j++)
        {
            queuedTurnControllers[j].gameObject.SetActive(false);
        }
    }
    #endregion

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
                queuedTurnControllers[i].ToggleBudgedStatus(true);
                turnsAreBudged[i] = true;
            }
            else if (turnsAreBudged[i] && turnBackgroundImage.transform.position.x > draggingTurnX)
            {
                queuedTurnControllers[i].ToggleBudgedStatus(false);
                turnsAreBudged[i] = false;
            }
        }
    }

    public void OnQueuedTurnDrop()
    {
        for (int i = 0; i < queuedTurnControllers.Length; i++)
        {
            queuedTurnControllers[i].OnOtherTurnDragEnded();
        }

        int newPlayerTurnIndex = 0;
        for (int i = 0; i < queuedTurnControllers.Length; i++)
        {
            Transform queuedTurnTransform = queuedTurnControllers[i].transform;

            // If moving turn to later in the queue, don't count the turn for the purpose of determining how many turns it's succeeding.
            // (Was giving higher-than-expected value when player's turn was moving later.)
            if (queuedTurnTransform == draggingTurn)
            {
                newPlayerTurnIndex--;
                continue;
            }

            if (!queuedTurnTransform.gameObject.activeSelf || queuedTurnTransform.position.x > draggingTurn.position.x)
            {
                newPlayerTurnIndex += i;
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
            queuedTurnControllers[i].RecalculateBudgedPosition(true);
        }
        for (int i = turnIndex; i < turnsAreBudged.Length; i++)
        {
            turnsAreBudged[i] = false;
            queuedTurnControllers[i].RecalculateBudgedPosition(false);
        }
    }
    #endregion

    #region helper funcs
    // source: https://forum.unity.com/threads/canvas-scaler-and-moving-2d-items-with-touch.296310/
    public Vector3 UnscalePointerData(Vector3 pointerPosition)
    {
        Vector2 referenceResolution = canvasScaler.referenceResolution;
        Vector2 currentResolution = new Vector2(Screen.width, Screen.height);

        float widthRatio = currentResolution.x / referenceResolution.x;
        float heightRatio = currentResolution.y / referenceResolution.y;
        float ratio = Mathf.Lerp(widthRatio, heightRatio, canvasScaler.matchWidthOrHeight);

        return pointerPosition / ratio;
    }
    #endregion
}
