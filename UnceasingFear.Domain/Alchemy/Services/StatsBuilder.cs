using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Domain.Shared.ValueObjects.Stats;

namespace UnceasingFear.Domain.Alchemy.Services
{
    public static class StatsBuilder
    {
        public readonly record struct StatFormulaConfig(
            int HealthPerTier, int SPPerTier, int PhysicPerTier,
            int DefensePerTier, int MagicPerTier, int SpeedPerTier);
        public static UnitStats CreateNewFromEssences(IReadOnlyList<StatDelta> inheritedStatDeltas, int tier, StatFormulaConfig config)
        {
            var baseStats = UnitStats.Create(
                maxHealth: tier * config.HealthPerTier,
                maxSP: tier * config.SPPerTier,
                physic: tier * config.PhysicPerTier,
                defense: tier * config.DefensePerTier,
                magic: tier * config.MagicPerTier,
                speed: tier * config.SpeedPerTier);

            return inheritedStatDeltas.Aggregate(baseStats, (stats, delta) => stats + delta);
        }
    }
}
