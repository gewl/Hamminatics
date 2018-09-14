using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScenarioState {
    public EntityData player;
    public Inventory inventory;
    public List<EntityData> enemies;
    public List<ItemData> items;

    public Stack<Turn> turnStack;
    public ScenarioState lastGameState;
    public CardData scenarioReward;

    public List<Vector2Int> stagnatedPositions;
    public List<Vector2Int> threatenedStagnationPositions;
    public Direction stagnationDirection;
    public StagnationStates stagnationState;
    public bool isBossScenario = false;

    public ScenarioState(List<EntityData> _enemies, List<ItemData> _items, CardData _scenarioReward, Direction _stagnationDirection, bool _isBossScenario)
    {
        player = GameStateManager.CurrentCampaign.player;
        enemies = _enemies;

        items = _items;
        turnStack = new Stack<Turn>();
        inventory = GameStateManager.CurrentCampaign.inventory.Copy();
        scenarioReward = _scenarioReward;

        stagnationDirection = _stagnationDirection;
        stagnatedPositions = new List<Vector2Int>();
        threatenedStagnationPositions = new List<Vector2Int>();
        stagnationState = StagnationStates.Threatening;

        isBossScenario = _isBossScenario;
    }

    public ScenarioState(EntityData _player, List<EntityData> _enemies, List<ItemData> _items, CardData _scenarioReward, Stack<Turn> _turns, Inventory _inventory, List<Vector2Int> _stagnatedPosition, List<Vector2Int> _threatenedPositions, StagnationStates _stagnationState, Direction _stagnationDirection, bool _isBossScenario)
    {
        player = _player;
        enemies = _enemies;
        turnStack = _turns;
        items = _items;
        inventory = _inventory;
        scenarioReward = _scenarioReward;

        stagnatedPositions = _stagnatedPosition;
        threatenedStagnationPositions = _threatenedPositions;
        stagnationState = _stagnationState;
        stagnationDirection = _stagnationDirection;

        isBossScenario = _isBossScenario;
    }

    public void UpdateTurnStack(Stack<Turn> newTurnStack)
    {
        turnStack = newTurnStack;
    }

}
