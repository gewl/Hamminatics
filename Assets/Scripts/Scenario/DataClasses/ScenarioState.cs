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

    public ScenarioState(List<EntityData> _enemies, List<ItemData> _items)
    {
        player = GameStateManager.CurrentCampaign.player;
        enemies = _enemies;

        items = _items;
        turnStack = new Stack<Turn>();
        inventory = GameStateManager.CurrentCampaign.inventory.Copy();
    }

    public ScenarioState(EntityData _player, List<EntityData> _enemies, List<ItemData> _items, Stack<Turn> _turns, Inventory _inventory)
    {
        player = _player;
        enemies = _enemies;
        turnStack = _turns;
        items = _items;
        inventory = _inventory;
    }

    public void UpdateTurnStack(Stack<Turn> newTurnStack)
    {
        turnStack = newTurnStack;
    }

}
