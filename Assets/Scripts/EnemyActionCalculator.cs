using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyActionCalculator : MonoBehaviour {

    ActionQueueController actionQueueController;

    private void Awake()
    {
        actionQueueController = GetComponent<ActionQueueController>();
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
