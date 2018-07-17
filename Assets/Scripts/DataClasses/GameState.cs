using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameState {
    public EntityData player;
    public List<EntityData> enemies;

    public Stack<Turn> turnStack;

    public List<CompletedMove> movesCompletedLastRound;
    public List<CompletedAction> actionsCompletedLastRound;

    public Dictionary<EntityData, List<PathStep>> entityPathsMap;

    public GameState(EntityData _player, List<EntityData> _enemies)
    {
        player = _player;
        enemies = _enemies;

        turnStack = new Stack<Turn>();
        movesCompletedLastRound = new List<CompletedMove>();
        actionsCompletedLastRound = new List<CompletedAction>();

        entityPathsMap = new Dictionary<EntityData, List<PathStep>>();
        this.GetAllEntities().ForEach(e => entityPathsMap[e] = new List<PathStep>());
    }

    public GameState(EntityData _player, List<EntityData> _enemies, Stack<Turn> _turns)
    {
        player = _player;
        enemies = _enemies;
        turnStack = _turns;

        movesCompletedLastRound = new List<CompletedMove>();
        actionsCompletedLastRound = new List<CompletedAction>();

        entityPathsMap = new Dictionary<EntityData, List<PathStep>>();
        this.GetAllEntities().ForEach(e => entityPathsMap[e] = new List<PathStep>());
    }

    public void UpdateTurnStack(Stack<Turn> newTurnStack)
    {
        turnStack = newTurnStack;
    }

}
