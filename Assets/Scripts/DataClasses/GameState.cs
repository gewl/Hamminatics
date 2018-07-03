using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameState {
    public EntityData player;
    public List<EntityData> enemies;

    public Stack<Turn> turnStack;

    public List<Tile> tilesAttackedLastRound;

    public GameState(EntityData _player, List<EntityData> _enemies)
    {
        player = _player;
        enemies = _enemies;

        turnStack = new Stack<Turn>();
        tilesAttackedLastRound = new List<Tile>();
    }

    public GameState(EntityData _player, List<EntityData> _enemies, Stack<Turn> _turns)
    {
        player = _player;
        enemies = _enemies;
        turnStack = _turns;

        tilesAttackedLastRound = new List<Tile>();
    }

    public void UpdateTurnStack(Stack<Turn> newTurnStack)
    {
        turnStack = newTurnStack;
    }

}
