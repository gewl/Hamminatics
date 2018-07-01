using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameStateManager : MonoBehaviour {

    public delegate void GameStateChangeDelegate(GameState updatedGameState);
    public GameStateChangeDelegate OnCurrentGameStateChange;
    public GameStateChangeDelegate OnTurnEnded;

    ActionStackController _actionStackController;
    ActionStackController actionStackController
    {
        get
        {
            if (_actionStackController == null)
            {
                _actionStackController = GetComponent<ActionStackController>();
            }

            return _actionStackController;
        }
    }
    BoardController _boardController;
    BoardController boardController
    {
        get
        {
            if (_boardController == null)
            {
                _boardController = GetComponentInChildren<BoardController>();
            }

            return _boardController;
        }
    }
    EquippedCardsManager _equippedCardsManager;
    EquippedCardsManager equippedCardsManager
    {
        get
        {
            if (_equippedCardsManager == null)
            {
                _equippedCardsManager = GetComponentInChildren<EquippedCardsManager>();
            }

            return _equippedCardsManager;
        }
    }
    EnemyActionCalculator _enemyActionCalculator;
    EnemyActionCalculator enemyActionCalculator
    {
        get
        {
            if (_enemyActionCalculator == null)
            {
                _enemyActionCalculator = GetComponent<EnemyActionCalculator>();
            }

            return _enemyActionCalculator;
        }
    }

    public GameState CurrentGameState { get; private set; }
    public GameState ProjectedGameState { get; private set; }
    List<Vector2Int> projectedAttackCoordinates;
    public EntityData Player { get { return CurrentGameState.player; } }

    List<Vector2Int> potentialCardTargets;

    bool isHandlingActions = false;
    public bool IsHandlingActions { get { return isHandlingActions; } }

    private void Awake()
    {
        potentialCardTargets = new List<Vector2Int>();

        projectedAttackCoordinates = new List<Vector2Int>();
    }

    public void InitializeGameState(GameBoard board)
    {
        CurrentGameState = GameStateGenerator.GenerateNewGameState(board.Entrance.Position);
        OnCurrentGameStateChange += ResetBoard;
        // This has to be delayed so layout group can space accordingly.
        Invoke("SetBoardUp", 0.1f);
    }

    private void OnEnable()
    {
        actionStackController.OnActionStackUpdate += ResetBoard; 
    }

    private void OnDisable()
    {
        actionStackController.OnActionStackUpdate -= ResetBoard; 
    }

    void SetBoardUp()
    {
        enemyActionCalculator.CalculateAndQueueActions(CurrentGameState);
        OnCurrentGameStateChange(CurrentGameState);
    }

    public void HighlightPotentialCardTargets(CardData card)
    {
        ResetBoard(CurrentGameState);

        int cardRange = card.Range;

        int leftMaximumX = Player.Position.x - cardRange;
        int rightMaximumX = Player.Position.x + cardRange;
        int topMaximumY = Player.Position.y - cardRange;
        int bottomMaximumY = Player.Position.y + cardRange;

        int testX = leftMaximumX;
        int testY = topMaximumY;

        while (testX <= rightMaximumX)
        {
            Vector2Int testPosition = new Vector2Int(testX, Player.Position.y);

            AttemptToHighlightCell(testPosition);

            testX++;
        }

        while (testY <= bottomMaximumY)
        {
            Vector2Int testPosition = new Vector2Int(Player.Position.x, testY);

            AttemptToHighlightCell(testPosition);

            testY++;
        }
    }

    void AttemptToHighlightCell(Vector2Int position)
    {
        if (GameStateHelperFunctions.IsCellValid(position) && Player.Position != position)
        {
            boardController.HighlightSelectedCell(position);
            potentialCardTargets.Add(position);
        }
    }

    public void ResetBoard(List<Action> actionList)
    {
        ResetBoard(CurrentGameState);
    }

    public void ResetBoard()
    {
        ResetBoard(CurrentGameState);
    }

    void ResetBoard(GameState currentGameState)
    {
        if (!isHandlingActions)
        {
            ProjectedGameState = GameStateHelperFunctions.CalculateFollowingGameState(currentGameState);
        }
        boardController.DrawBoard(currentGameState, ProjectedGameState, projectedAttackCoordinates);
        potentialCardTargets.Clear();
    }

    public void RegisterCellClick(Vector2Int cellPosition)
    {
        if (isHandlingActions)
        {
            return;      
        }
        if (potentialCardTargets.Contains(cellPosition))
        {
            actionStackController.AddPlayerAction(equippedCardsManager.GetSelectedCard(), Player, GameStateHelperFunctions.GetDirectionFromPlayer(cellPosition, CurrentGameState), GameStateHelperFunctions.GetCellDistanceFromPlayer(cellPosition, CurrentGameState));
            equippedCardsManager.ClearSelectedCard();
            OnCurrentGameStateChange(CurrentGameState);
        }
        else
        {
        }
    }

    public void EndTurn()
    {
        StartCoroutine(ProcessCurrentTurnActions());
    }

    IEnumerator ProcessCurrentTurnActions()
    {
        isHandlingActions = true;
        projectedAttackCoordinates.Clear();
        while (!actionStackController.IsActionStackEmpty)
        {
            Action nextAction = actionStackController.GetNextAction();

            GameStateHelperFunctions.ProcessAction(nextAction, CurrentGameState);

            OnCurrentGameStateChange(CurrentGameState);
            yield return new WaitForSeconds(0.5f);
        }

        enemyActionCalculator.CalculateAndQueueActions(CurrentGameState);

        isHandlingActions = false;
        if (OnCurrentGameStateChange != null)
        {
            OnCurrentGameStateChange(CurrentGameState);
        }

        if (OnTurnEnded != null)
        {
            OnTurnEnded(CurrentGameState);
        }
    }
}
