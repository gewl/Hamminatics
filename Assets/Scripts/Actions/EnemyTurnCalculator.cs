﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyTurnCalculator : MonoBehaviour {

    TurnStackController turnStackController;

    List<Tile> projectedEnemyMovementTiles;
    List<Tile> projectedEnemyAttackTiles;
    List<Tile> currentlyOccupiedTiles;

    private void Awake()
    {
        turnStackController = GetComponent<TurnStackController>();
        projectedEnemyAttackTiles = new List<Tile>();
        projectedEnemyMovementTiles = new List<Tile>();

        currentlyOccupiedTiles = new List<Tile>();
    }

    // TODO: Very rudimentary AI rn, assumes all enemies rushing at player, need some
    // switch logic for different AI types.
    public void CalculateAndQueueEnemyTurns(GameState gameState)
    {
        List<EntityData> enemies = gameState.enemies;
        projectedEnemyMovementTiles.Clear();
        projectedEnemyAttackTiles.Clear();

        currentlyOccupiedTiles = enemies.Select(enemy => BoardHelperFunctions.GetTileAtPosition(enemy.Position)).ToList<Tile>();

        foreach (EntityData enemy in enemies)
        {
            Vector2Int enemyPosition = enemy.Position;

            MovementCardData enemyMovementCard = enemy.MovementCard;
            int enemyMoveRange = enemyMovementCard.Range;

            List<Tile> potentialMoveTargetTiles = BoardHelperFunctions.GetPotentialTargetsRecursively(enemyPosition, enemyMoveRange);

            List<Tile> sortedPotentialMoveTargets = SortTilesByMoveEligibility(potentialMoveTargetTiles);

            Tile targetMovementTile = sortedPotentialMoveTargets[0];
            BoardController.TurnTileColor(targetMovementTile, Color.blue);
            projectedEnemyMovementTiles.Add(targetMovementTile);

            List<Direction> movesToTargetMovementTile = BoardHelperFunctions.GetPathToTile(BoardHelperFunctions.GetTileAtPosition(enemyPosition), targetMovementTile);

            AttackCardData enemyAttackCard = enemy.attackCard;
            int enemyAttackRange = enemyAttackCard.Range;

            // IF enemy is projected to move into player's tile:
            // THEN attack as if enemy is NOT moving (to account for projected player displacement)
            // ELSE attack from projected movement tile
            Tile projectedMovementTile = targetMovementTile.DistanceFromPlayer == 0 ? BoardHelperFunctions.GetTileAtPosition(enemyPosition) : targetMovementTile;

            List<Tile> potentialAttackTargetTiles = BoardHelperFunctions.GetPotentialTargets(projectedMovementTile.Position, enemyAttackRange);
            List<Tile> sortedPotentialAttackTargets = SortTilesByMoveEligibility(potentialAttackTargetTiles);
            Tile targetAttackTile = sortedPotentialAttackTargets[0];

            projectedEnemyAttackTiles.Add(targetAttackTile);

            int rangeOfProjectedAttack = BoardHelperFunctions.GetLinearDistanceBetweenTiles(projectedMovementTile, targetAttackTile);
            Direction attackDirection = GameStateHelperFunctions.GetDirectionFromPosition(projectedMovementTile.Position, targetAttackTile.Position);

            Action secondAction = new Action(enemyAttackCard, enemy, attackDirection, rangeOfProjectedAttack);

            Turn enemyTurn = new Turn(enemy, movesToTargetMovementTile, secondAction);

            turnStackController.AddNewTurn(enemyTurn);
        }
    }

    List<Tile> SortTilesByMoveEligibility(List<Tile> unsortedList)
    {
        return unsortedList
            .OrderBy(tile => CalculateTileMovementValue(tile))
            .ThenBy(tile => Random.Range(0f, 1f))
            .ToList<Tile>();
    }

    // OrderBy is ascending, so higher result = lower calculated value
    int CalculateTileMovementValue(Tile tile)
    {
        int result = 0;

        result += tile.DistanceFromPlayer;
        result += tile.DistanceFromPlayer == 0 ? 10 : 0;
        result += projectedEnemyAttackTiles.Contains(tile) ? 3 : 0;
        result += projectedEnemyMovementTiles.Contains(tile) ? 5 : 0;
        result += currentlyOccupiedTiles.Contains(tile) ? 10 : 0;

        return result;
    }

    List<Tile> SortTilesByAttackEligibility(List<Tile> unsortedList)
    {
        return unsortedList
            .OrderBy(tile => CalculateTileAttackValue(tile))
            .ThenBy(tile => Random.Range(0f, 1f))
            .ToList<Tile>();
    }

    int CalculateTileAttackValue(Tile tile)
    {
        int result = 0;

        result += tile.DistanceFromPlayer;
        result += projectedEnemyAttackTiles.Contains(tile) ? 3 : 0;
        result += projectedEnemyMovementTiles.Contains(tile) ? 5 : 0;
        result += currentlyOccupiedTiles.Contains(tile) ? 10 : 0;

        return result;
    }
}
