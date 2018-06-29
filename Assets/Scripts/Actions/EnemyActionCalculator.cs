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
            CardData enemyAttackCard = enemy.attackCard;

            actionQueueController.AddNewAction(enemyAttackCard, enemy, Direction.Left, enemyAttackCard.Range);
        }
    }
}
