using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EntityExtensions {

    public static void DealDamage(this EntityData entity, int amount, GameState gameState)
    {
        int newHealth = entity.Health - amount;
        entity.SetHealth(newHealth);

        if (entity.Health <= 0)
        {
            gameState.enemies.RemoveAll(e => e == entity);

            ItemData itemToSpawn = entity.dropItem;
            Vector2Int positionToSpawn = entity.Position;

            if (itemToSpawn != null && !gameState.DoesPositionContainItem(positionToSpawn))
            {
                ItemData itemInstance = Object.Instantiate(itemToSpawn);
                gameState.items.Add(itemInstance);
            }
        }

    }

}
