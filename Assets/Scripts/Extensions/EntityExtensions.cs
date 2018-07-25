using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EntityExtensions {

    public static void DealDamage(this EntityData entity, int amount, GameState gameState)
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

}
