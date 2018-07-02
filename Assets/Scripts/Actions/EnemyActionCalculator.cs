using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyActionCalculator : MonoBehaviour {

    [SerializeField]
    BoardController boardController;

    ActionStackController actionStackController;

    private void Awake()
    {
        actionStackController = GetComponent<ActionStackController>();
    }

    // TODO: This needs to work with 2-subaction actions, non-linear movement, etc.
    public void CalculateAndQueueActions(GameState gameState)
    {
        List<EntityData> enemies = gameState.enemies;

        foreach (EntityData enemy in enemies)
        {
            MovementCardData enemyMove = enemy.MovementCard;

            Vector2Int enemyPosition = enemy.Position;
            int enemyRange = enemyMove.Range;

            List<Tile> potentialTargetTiles = boardController.GetPotentialMoves(enemyPosition, enemyRange);
            potentialTargetTiles.Sort((t1, t2) => t1.DistanceFromPlayer.CompareTo(t2.DistanceFromPlayer));

            Tile targetTile = potentialTargetTiles[0];

            Direction direction = GameStateHelperFunctions.GetDirectionFromEntity(enemy, targetTile.Position);

            actionStackController.AddNewAction(enemyMove, enemy, direction, enemyRange);
        }
    }
}
