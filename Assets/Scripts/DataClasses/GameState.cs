using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameState {
    public EntityData player;
    public Inventory inventory;
    public List<EntityData> enemies;
    public List<ItemData> items;

    public Stack<Turn> turnStack;
    public GameState lastGameState;

    public GameState(EntityData _player, List<EntityData> _enemies)
    {
        player = _player;
        enemies = _enemies;

        items = new List<ItemData>();
        turnStack = new Stack<Turn>();
        inventory = new Inventory();
    }

    public GameState(EntityData _player, List<EntityData> _enemies, List<ItemData> _items, Inventory _inventory)
    {
        player = _player;
        enemies = _enemies;
        items = _items;
        inventory = _inventory;

        turnStack = new Stack<Turn>();
    }

    public GameState(EntityData _player, List<EntityData> _enemies, List<ItemData> _items, Stack<Turn> _turns, Inventory _inventory)
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
