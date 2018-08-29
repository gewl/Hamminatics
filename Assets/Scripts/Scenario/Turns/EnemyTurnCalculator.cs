using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyTurnCalculator : MonoBehaviour {

    TurnStackController turnStackController;

    List<EntityTurnTargets> upcomingEntityTargets;
    List<Tile> currentlyOccupiedTiles;

    private void Awake()
    {
        turnStackController = GetComponent<TurnStackController>();
        upcomingEntityTargets = new List<EntityTurnTargets>();

        currentlyOccupiedTiles = new List<Tile>();
    }

    // TODO: Very rudimentary AI rn, assumes all enemies rushing at player, need some
    // switch logic for different AI types.
    public void CalculateAndQueueEnemyTurns(ScenarioState gameState)
    {
        GameBoard currentBoard = BoardController.CurrentBoard;
        List<EntityData> enemies = gameState.enemies;
        upcomingEntityTargets.Clear();

        currentlyOccupiedTiles = enemies.Select(enemy => currentBoard.GetTileAtPosition(enemy.Position)).ToList<Tile>();

        foreach (EntityData enemy in enemies)
        {
            Tile enemyTile = currentBoard.GetTileAtPosition(enemy.Position);

            MovementCardData enemyMovementCard = enemy.movementCard;
            int enemyMoveRange = enemyMovementCard.range + enemy.GetMovementModifierValue();

            AttackCardData enemyAttackCard = enemy.attackCard;
            int enemyAttackRange = enemyAttackCard.range;

            List<EntityTurnTargets> possibleEntityTurns = enemyTile.GetAllPossibleEntityTurns(enemyMoveRange, enemyAttackRange);
            List<EntityTurnTargets> sortedPotentialTurns = SortTurnTargetsByValue(enemyTile, possibleEntityTurns);
            //List<Tile> potentialMoveTargetTiles = enemyTile.GetAllTilesWithinRange(enemyMoveRange);

            //List<Tile> sortedPotentialMoveTargets = SortTilesByMoveEligibility(enemyTile, potentialMoveTargetTiles, enemyAttackRange);

            int turnIndex = 0;
            EntityTurnTargets selectedTurnTargets = sortedPotentialTurns[turnIndex];
            List<Direction> movesToTargetMovementTile = BoardHelperFunctions.FindPathBetweenTiles(enemyTile, selectedTurnTargets.targetMovementTile);
            List<Tile> tilesToTargetMovementTile = BoardHelperFunctions.GetTilesOnPath(enemyTile, movesToTargetMovementTile);

            // Enemies will not move through traps if they have any other moves available.
            while (turnIndex < sortedPotentialTurns.Count &&
                tilesToTargetMovementTile.Any(tile => gameState.DoesPositionContainItemWhere(tile.Position, item => item.itemCategory == ItemCategory.Trap)))
            {
                turnIndex++;
                selectedTurnTargets = sortedPotentialTurns[turnIndex];
                movesToTargetMovementTile = BoardHelperFunctions.FindPathBetweenTiles(enemyTile, selectedTurnTargets.targetMovementTile);
                tilesToTargetMovementTile = BoardHelperFunctions.GetTilesOnPath(enemyTile, movesToTargetMovementTile);
            }

            if (turnIndex == sortedPotentialTurns.Count)
            {
                selectedTurnTargets = sortedPotentialTurns[0];
            }

            upcomingEntityTargets.Add(selectedTurnTargets);
            //projectedEnemyMovementTiles.Add(targetMovementTile);

            //// IF enemy is projected to move into player's tile:
            //// THEN attack as if enemy is NOT moving (to account for projected player displacement)
            //// ELSE attack from projected movement tile
            //Tile projectedMovementTile = targetMovementTile.DistanceFromPlayer == 0 ? enemyTile : targetMovementTile;

            //List<Tile> potentialAttackTargetTiles = projectedMovementTile.GetTilesWithinLinearRange(enemyAttackRange);
            //List<Tile> sortedPotentialAttackTargets = SortTilesByAttackEligibility(potentialAttackTargetTiles);
            //Tile targetAttackTile = sortedPotentialAttackTargets[0];

            //projectedEnemyAttackTiles.Add(targetAttackTile);

            int rangeOfProjectedAttack = BoardHelperFunctions.GetLinearDistanceBetweenTiles(selectedTurnTargets.targetMovementTile, selectedTurnTargets.targetAttackTile);
            Direction attackDirection = BoardHelperFunctions.GetDirectionFromPosition(selectedTurnTargets.targetMovementTile.Position, selectedTurnTargets.targetAttackTile.Position);

            Action enemyAction = new Action(enemyAttackCard, enemy, attackDirection, rangeOfProjectedAttack);

            Turn enemyTurn = new Turn(enemy, movesToTargetMovementTile, enemyAction);

            turnStackController.AddNewTurn(enemyTurn);
        }
    }

    List<EntityTurnTargets> SortTurnTargetsByValue(Tile startingTile, List<EntityTurnTargets> unsortedList)
    {
        return unsortedList
            .OrderBy(turn => CalculateTurnValue(startingTile, turn))
            .ThenBy(tile => Random.Range(0f, 1f))
            .ToList();
    }

    // Used for OrderBy, so higher result = lower priority = lower value turn
    int CalculateTurnValue(Tile startingTile, EntityTurnTargets turn)
    {
        int result = 0;

        result += Mathf.Abs(startingTile.Position.x - turn.targetMovementTile.Position.x);
        result += Mathf.Abs(startingTile.Position.y - turn.targetMovementTile.Position.y);
        result += (turn.targetAttackTile.DistanceFromPlayer * 5);
        result += upcomingEntityTargets.Any(target => target.targetAttackTile == turn.targetAttackTile) ? 15 : 0;

        return result;
    }


    //List<Tile> SortTilesByMoveEligibility(Tile startingTile, List<Tile> unsortedList, int attackRange)
    //{
    //    return unsortedList
    //        .OrderBy(tile => CalculateTileMovementValue(startingTile, tile, attackRange))
    //        .ThenBy(tile => Random.Range(0f, 1f))
    //        .ToList<Tile>();
    //}

    // OrderBy is ascending, so higher result = lower value
    //int CalculateTileMovementValue(Tile startingTile, Tile destinationTile, int attackRange)
    //{
    //    int result = 0;

    //    result += Mathf.Abs(destinationTile.DistanceFromPlayer - attackRange) * 10;

    //    // Super rough addition to increase value of tiles closer to origin tile
    //    // without having to perform redundant path-mapping just to find exact distance.

    //    result += Mathf.Abs(startingTile.Position.x - destinationTile.Position.x);
    //    result += Mathf.Abs(startingTile.Position.y - destinationTile.Position.y);

    //    result += destinationTile.DistanceFromPlayer == 0 ? 100 : 0;
    //    result += projectedEnemyAttackTiles.Contains(destinationTile) ? 30 : 0;
    //    result += projectedEnemyMovementTiles.Contains(destinationTile) ? 50 : 0;
    //    result += currentlyOccupiedTiles.Contains(destinationTile) ? 100 : 0;

    //    return result;
    //}

    //List<Tile> SortTilesByAttackEligibility(List<Tile> unsortedList)
    //{
    //    return unsortedList
    //        .OrderBy(tile => CalculateTileAttackValue(tile))
    //        .ThenBy(tile => Random.Range(0f, 1f))
    //        .ToList<Tile>();
    //}

    //    int CalculateTileAttackValue(Tile tile)
    //    {
    //        int result = 0;

    //        result += tile.DistanceFromPlayer;
    //        result += projectedEnemyAttackTiles.Contains(tile) ? 3 : 0;
    //        result += projectedEnemyMovementTiles.Contains(tile) ? 5 : 0;
    //        result += currentlyOccupiedTiles.Contains(tile) ? 10 : 0;

    //        return result;
    //    }
}
