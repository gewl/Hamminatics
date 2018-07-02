﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyTurnCalculator : MonoBehaviour {

    [SerializeField]
    BoardController boardController;

    TurnStackController turnStackController;

    List<Tile> projectedEnemyMovementTiles;
    List<Tile> projectedEnemyAttackTiles;

    private void Awake()
    {
        turnStackController = GetComponent<TurnStackController>();
        projectedEnemyAttackTiles = new List<Tile>();
        projectedEnemyMovementTiles = new List<Tile>();
    }

    // TODO: This needs to work with 2-subaction actions, etc.
    public void CalculateAndQueueEnemyTurns(GameState gameState)
    {
        List<EntityData> enemies = gameState.enemies;
        projectedEnemyMovementTiles.Clear();
        projectedEnemyAttackTiles.Clear();

        foreach (EntityData enemy in enemies)
        {
            Vector2Int enemyPosition = enemy.Position;

            MovementCardData enemyMovementCard = enemy.MovementCard;
            int enemyMoveRange = enemyMovementCard.Range;

            List<Tile> potentialMoveTargetTiles = boardController.GetPotentialMoves(enemyPosition, enemyMoveRange);

            List<Tile> sortedPotentialMoveTargets = SortTilesByEligibility(potentialMoveTargetTiles);

            Tile targetMovementTile = sortedPotentialMoveTargets[0];
            projectedEnemyMovementTiles.Add(targetMovementTile);

            Direction moveDirection = GameStateHelperFunctions.GetDirectionFromEntity(enemy, targetMovementTile.Position);

            Action firstAction = new Action(enemyMovementCard, enemy, moveDirection, enemyMoveRange);

            AttackCardData enemyAttackCard = enemy.attackCard;
            int enemyAttackRange = enemyAttackCard.Range;

            // IF enemy is projected to move into player's tile:
            // THEN attack as if enemy is NOT moving (to account for projected player displacement)
            // ELSE attack from projected movement tile
            Tile projectedMovementTile = targetMovementTile.DistanceFromPlayer == 0 ? boardController.GetTileAtPosition(enemyPosition) : targetMovementTile;

            List<Tile> potentialAttackTargetTiles = boardController.GetPotentialMoves(projectedMovementTile.Position, enemyAttackRange);
            List<Tile> sortedPotentialAttackTargets = SortTilesByEligibility(potentialAttackTargetTiles);
            Tile targetAttackTile = sortedPotentialAttackTargets[0];

            projectedEnemyAttackTiles.Add(targetAttackTile);

            Direction attackDirection = GameStateHelperFunctions.GetDirectionFromPosition(projectedMovementTile.Position, targetAttackTile.Position);

            Action secondAction = new Action(enemyAttackCard, enemy, attackDirection, enemyAttackRange);

            Turn enemyTurn = new Turn(enemy, firstAction, secondAction);

            turnStackController.AddNewTurn(enemyTurn);
        }
    }

    List<Tile> SortTilesByEligibility(List<Tile> unsortedList)
    {
        return unsortedList
            .OrderBy(tile => !projectedEnemyAttackTiles.Contains(tile) ? 0 : 1)
            .ThenBy(tile => !projectedEnemyMovementTiles.Contains(tile) ? 0 : 1)
            .ThenBy(tile => tile.DistanceFromPlayer)
            .ToList<Tile>();
    }
}