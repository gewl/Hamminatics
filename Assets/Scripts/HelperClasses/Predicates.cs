using System;
using System.Linq.Expressions;

namespace Predicates {

    public static class EntityPredicates
    {
        public static Predicate<EntityData> IsMelee = e => e.attackCard.range == 1;

        public static Predicate<EntityData> IsRanged = e => e.attackCard.range > 1;

        public static Predicate<EntityData> IsAlive = e => e.CurrentHealth >= 1;

        public static Predicate<EntityData> IsDead = e => e.CurrentHealth < 1;
    }

}
