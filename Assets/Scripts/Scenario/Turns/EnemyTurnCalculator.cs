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
            List<EntityTurnTargets> sortedPotentialTurns = SortTurnTargetsByValue(enemyTile, possibleEntityTurns, gameState);
            int turnIndex = 0;
            EntityTurnTargets selectedTurnTargets = sortedPotentialTurns[turnIndex];
            List<Direction> movesToTargetMovementTile = BoardHelperFunctions.FindPathBetweenTiles(enemyTile, selectedTurnTargets.targetMovementTile);
            List<Tile> tilesToTargetMovementTile = BoardHelperFunctions.GetTilesOnPath(enemyTile, movesToTargetMovementTile);

            // Enemies will not move through traps if they have any other moves available.
            while (turnIndex < sortedPotentialTurns.Count &&
                tilesToTargetMovementTile.Any(tile => gameState.DoesPositionContainItemWhere(tile.Position, item => item.itemCategory == ItemCategory.Trap) ||
                gameState.IsTileOccupied(tile)))
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
            int rangeOfProjectedAttack = BoardHelperFunctions.GetLinearDistanceBetweenTiles(selectedTurnTargets.targetMovementTile, selectedTurnTargets.targetAttackTile);
            Direction attackDirection = BoardHelperFunctions.GetDirectionFromPosition(selectedTurnTargets.targetMovementTile.Position, selectedTurnTargets.targetAttackTile.Position);

            Action enemyAction = new Action(enemyAttackCard, enemy, attackDirection, rangeOfProjectedAttack);

            Turn enemyTurn = new Turn(enemy, movesToTargetMovementTile, enemyAction);

            turnStackController.AddNewTurn(enemyTurn);
        }
    }

    List<EntityTurnTargets> SortTurnTargetsByValue(Tile startingTile, List<EntityTurnTargets> unsortedList, ScenarioState expectedState)
    {
        return unsortedList
            .OrderBy(turn => CalculateTurnValue(startingTile, turn, expectedState))
            .ThenBy(tile => Random.Range(0f, 1f))
            .ToList();
    }

    // Used for OrderBy, so higher result = lower priority = lower value turn
    int CalculateTurnValue(Tile startingTile, EntityTurnTargets turn, ScenarioState expectedState)
    {
        int result = 0;

        result += Mathf.Abs(startingTile.Position.x - turn.targetMovementTile.Position.x) * 2;
        result += Mathf.Abs(startingTile.Position.y - turn.targetMovementTile.Position.y) * 2;
        result += (turn.targetAttackTile.DistanceFromPlayer * 5);
        result += upcomingEntityTargets.Any(target => target.targetAttackTile == turn.targetAttackTile) ? 15 : 0;
        result += upcomingEntityTargets.Any(target => expectedState.threatenedStagnationPositions.Contains(target.targetMovementTile.Position)) ? 20 : 0;
        result += upcomingEntityTargets.Any(target => target.targetMovementTile == turn.targetAttackTile) ? 30 : 0;
        result += upcomingEntityTargets.Any(target => expectedState.stagnatedPositions.Contains(target.targetMovementTile.Position)) ? 40 : 0;

        return result;
    }

}
