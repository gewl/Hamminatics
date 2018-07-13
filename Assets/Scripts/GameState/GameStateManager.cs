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
    public Vector2Int ProjectedPlayerPosition { get { return Player.Position; } }

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

        List<Tile> reachableTiles = BoardHelperFunctions.GetDirectlyReachableTiles(ProjectedPlayerPosition);

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

            if (reachableTiles.Exists(tile => tile.Position == testPosition))
            {
                AttemptToHighlightCell(testPosition);
            }

            testX++;
        }

        while (testY <= bottomMaximumY)
        {
            Vector2Int testPosition = new Vector2Int(Player.Position.x, testY);

            if (reachableTiles.Exists(tile => tile.Position == testPosition))
            {
                AttemptToHighlightCell(testPosition);
            }

            testY++;
        }
    }

    void AttemptToHighlightCell(Vector2Int position)
    {
        if (GameStateHelperFunctions.IsTileValid(position) && Player.Position != position)
        {
            boardController.HighlightSelectedCell(position);
            potentialCardTargets.Add(position);
        }
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
            //turnStackController.AddPlayerTurn(equippedCardsManager.GetSelectedCard(), Player, GameStateHelperFunctions.GetDirectionFromEntity(CurrentGameState.player, cellPosition), GameStateHelperFunctions.GetCellDistanceFromPlayer(cellPosition, CurrentGameState));
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
        CurrentGameState.tilesAttackedLastRound.Clear();
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
