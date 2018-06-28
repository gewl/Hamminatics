using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameState {
    public EntityData player;
    public List<EntityData> enemies;

    public Stack<Action> actionStack;

    public GameState(EntityData _player, List<EntityData> _enemies)
    {
        player = _player;
        enemies = _enemies;

        actionStack = new Stack<Action>();
    }

    public GameState(EntityData _player, List<EntityData> _enemies, Stack<Action> _actions)
    {
        player = _player;
        enemies = _enemies;
        actionStack = _actions;
    }

    public void UpdateActionStack(Stack<Action> newActionStack)
    {
        actionStack = newActionStack;
    }

}
