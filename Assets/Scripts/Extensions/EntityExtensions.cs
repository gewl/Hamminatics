using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EntityExtensions {

    /// <summary>
    /// Only for use in scenario.
    /// </summary>
    public static void DealDamage(this EntityData entity, int amount, ScenarioState gameState)
    {
        int newHealth = entity.CurrentHealth - amount;
        entity.SetHealth(newHealth);

        if (entity.CurrentHealth <= 0)
        {
            gameState.enemies.RemoveAll(e => e == entity);

            TreasureData itemToSpawn = entity.dropItem as TreasureData;
            Vector2Int positionToSpawn = entity.Position;

            if (itemToSpawn != null && !gameState.DoesPositionContainItem(positionToSpawn))
            {
                TreasureData itemInstance = Object.Instantiate(itemToSpawn);
                itemInstance.Position = positionToSpawn;
                gameState.items.Add(itemInstance);
            }
        }
    }

    /// <summary>
    /// Only for use in campaign view (outside of scenario).
    /// </summary>
    /// <param name="entity">Entity to have health changed.</param>
    /// <param name="value">Value to be added to entity health.</param>
    public static void ChangeHealthValue(this EntityData entity, int value)
    {
        int newHealth = entity.CurrentHealth + value;
        entity.SetHealth(newHealth);
    }
}
