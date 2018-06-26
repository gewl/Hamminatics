using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyActionCalculator : MonoBehaviour {

    ActionStackController actionQueueController;

    private void Awake()
    {
        actionQueueController = GetComponent<ActionStackController>();
    }

    public void CalculateAndQueueActions(GameState gameState)
    {
        List<EntityData> enemies = gameState.enemies;

        foreach (EntityData enemy in enemies)
        {
            CardData enemyMovementCard = enemy.MovementCard;

            actionQueueController.AddNewAction(enemyMovementCard, enemy, Direction.Left, enemyMovementCard.Range);
        }
    }
}
