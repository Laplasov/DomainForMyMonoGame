using System;
using System.Collections.Generic;
using System.Text;

namespace UnceasingFear.Domain.World.Enums
{
    public enum UnitBehavior { Stationary, Patrol, Territorial, Chase, PlayerControlled, Neutral }

    public struct MovementPatternHelper
    {
        public static bool IsAggresive(UnitBehavior pattern)
        {
            return pattern switch
            {
                UnitBehavior.Stationary => true,
                UnitBehavior.Patrol => true,
                UnitBehavior.Territorial => true,
                UnitBehavior.Chase => true,
                UnitBehavior.PlayerControlled => false,
                UnitBehavior.Neutral => false,
                _ => false,
            };
        }

    };

}
