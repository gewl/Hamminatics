using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameStateManager : MonoBehaviour {

    public delegate void GameStateChangeDelegate(GameState updatedGameState);
    public GameStateChangeDelegate OnCurrentGameStateChange;
    public GameStateChangeDelegate OnRoundEnded;

    TurnStackController _turnStackController;
    TurnStackController turnStackController
    {
        get
        {
            if (_turnStackController == null)
            {
                _turnStackController = GetComponent<TurnStackController>();
            }

            return _turnStackController;
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
    EnemyTurnCalculator _enemyTurnCalculator;
    EnemyTurnCalculator enemyTurnCalculator
    {
        get
        {
            if (_enemyTurnCalculator == null)
            {
                _enemyTurnCalculator = GetComponent<EnemyTurnCalculator>();
            }

            return _enemyTurnCalculator;
        }
    }

    public GameState CurrentGameState { get; private set; }
    public GameState ProjectedGameState { get; private set; }
    public EntityData Player { get { return CurrentGameState.player; } }

    List<Vector2Int> potentialCardTargets;

    bool isHandlingTurn = false;
    public bool IsHandlingActions { get { return isHandlingTurn; } }
    public Vector2Int ProjectedPlayerPosition { get { return ProjectedGameState.player.Position; } }

    private void Awake()
    {
        potentialCardTargets = new List<Vector2Int>();
    }

    public void InitializeGameState(GameBoard board)
    {
        CurrentGameState = GameStateGenerator.GenerateNewGameState(board.Entrance.Position, board.BoardWidth);
        OnCurrentGameStateChange += ResetBoard;
        // This has to be delayed so layout group can space accordingly.
        Invoke("SetBoardUp", 0.1f);
    }

    private void OnEnable()
    {
        turnStackController.OnTurnStackUpdate += ResetBoard; 
    }

    private void OnDisable()
    {
        turnStackController.OnTurnStackUpdate -= ResetBoard; 
    }

    void SetBoardUp()
    {
        GenerateNextTurnStack(CurrentGameState);
        OnCurrentGameStateChange(CurrentGameState);
    }

    public void HighlightPotentialCardTargets(CardData card)
    {
        ResetBoard(CurrentGameState);

        int cardRange = card.Range;

        // Movement availability is always 'from' player's current position.
        // Other actions are 'from' player's projected position.
        Vector2Int playerOrigin = card.Category == CardCategory.Movement ? Player.Position : ProjectedPlayerPosition;

        BoardHelperFunctions.GetPotentialBranchingTargets(playerOrigin, cardRange).ForEach(t => HighlightCell(t.Position));
    }

    void HighlightCell(Vector2Int position)
    {
        boardController.HighlightSelectedCell(position);
        potentialCardTargets.Add(position);
    }

    public void ResetBoard(List<Turn> turnList)
    {
        ResetBoard(CurrentGameState);
    }

    public void ResetBoard()
    {
        ResetBoard(CurrentGameState);
    }

    void ResetBoard(GameState currentGameState)
    {
        if (!isHandlingTurn)
        {
            ProjectedGameState = GameStateHelperFunctions.CalculateFollowingGameState(currentGameState);
        }
        boardController.DrawBoard(currentGameState, ProjectedGameState);
        potentialCardTargets.Clear();
    }

    public void RegisterCellClick(Vector2Int cellPosition)
    {
        if (isHandlingTurn)
        {
            return;      
        }
        if (potentialCardTargets.Contains(cellPosition))
        {
            CardData selectedCard = equippedCardsManager.GetSelectedCard();
            Vector2Int playerOrigin = selectedCard.Category == CardCategory.Movement ? Player.Position : ProjectedPlayerPosition;
            turnStackController.AddToPlayerTurn(selectedCard, Player, playerOrigin, cellPosition);
            equippedCardsManager.ClearSelectedCard();
            OnCurrentGameStateChange(CurrentGameState);
        }
        else
        {
        }
    }

    public void EndRound()
    {
        StartCoroutine(ProcessCurrentRoundActions());
    }

    IEnumerator ProcessCurrentRoundActions()
    {
        isHandlingTurn = true;
        CurrentGameState.actionsCompletedLastRound.Clear();
        while (!turnStackController.IsTurnStackEmpty)
        {
            Turn nextTurn = turnStackController.GetNextTurn();

            GameStateHelperFunctions.ProcessTurn(nextTurn, CurrentGameState);

            OnCurrentGameStateChange(CurrentGameState);
            yield return new WaitForSeconds(0.5f);
        }

        GenerateNextTurnStack(CurrentGameState);

        if (OnRoundEnded != null)
        {
            OnRoundEnded(CurrentGameState);
        }

        isHandlingTurn = false;

        if (OnCurrentGameStateChange != null)
        {
            OnCurrentGameStateChange(CurrentGameState);
        }
    }

    void GenerateNextTurnStack(GameState gameState)
    {
        enemyTurnCalculator.CalculateAndQueueEnemyTurns(CurrentGameState);
        turnStackController.AddEmptyPlayerTurn();
    }
}
