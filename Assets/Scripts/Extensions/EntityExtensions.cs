using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using static Predicates.EntityPredicates;

public static class EntityExtensions {

    /// <summary>
    /// Only for use in scenario.
    /// </summary>
    public static void DealDamage(this EntityData entity, int amount, ScenarioState gameState)
    {
        int newHealth = entity.CurrentHealth - amount;
        entity.SetHealth(newHealth);

        if (entity.CheckThat(IsDead))
        {
            gameState.enemies.RemoveAll(e => e == entity);

            TreasureData itemToSpawn = entity.dropItem as TreasureData;
            Vector2Int positionToSpawn = entity.Position;

            if (itemToSpawn != null && !gameState.DoesPositionContainItem(positionToSpawn))
            {
                TreasureData itemInstance = UnityEngine.Object.Instantiate(itemToSpawn);
                itemInstance.Position = positionToSpawn;
                gameState.items.Add(itemInstance);
            }
        }
    }

    /// <summary>
    /// Only for use in campaign view OR healing in scenario.
    /// </summary>
    /// <param name="entity">Entity to have health changed.</param>
    /// <param name="value">Value to be added to entity health.</param>
    public static void ChangeHealthValue(this EntityData entity, int value)
    {
        int newHealth = entity.CurrentHealth + value;
        entity.SetHealth(newHealth);
    }

    public static int GetMovementModifierValue(this EntityData entity)
    {
        int result = 0;

        entity.activeModifiers.ForEach(m =>
        {
            if (m.modifierCategory == ModifierCategory.Speed)
            {
                result += m.value;
            }
            else if (m.modifierCategory == ModifierCategory.Slow)
            {
                result -= m.value;
            }
        });

        return result;
    }

    public static int GetAttackModifierValue(this EntityData entity)
    {
        int result = 0;

        entity.activeModifiers.ForEach(m =>
        {
            if (m.modifierCategory == ModifierCategory.Strength)
            {
                result += m.value;
            }
            else if (m.modifierCategory == ModifierCategory.Weaken)
            {
                result -= m.value;
            }
        });

        return result;
    }

    public static List<ModifierData> GetModifiersOfCategory(this EntityData entity, params ModifierCategory[] categories)
    {
        return entity.activeModifiers.Where(m => categories.Any(c => c == m.modifierCategory)).ToList();
    }

    public static bool CheckThat(this EntityData entity, Predicate<EntityData> predicate)
    {
        return predicate(entity);
    }
}
