using System;
using System.Collections.Generic;
using System.Text;

namespace UnceasingFear.Domain.Combat.ValueObjects.Stats
{
    public record UnitStats
    {
        public Health Health { get; init; }
        public SpellPoints SpellPoints { get; init; }
        public int Physic { get; init; }
        public int Defense { get; init; }
        public int Magic { get; init; }
        public int Speed { get; init; }

        public bool IsAlive => Health.IsAlive;
        public static UnitStats Create(int maxHealth, int maxSP, int physic, int defense, int magic, int speed)
        {
            if (maxHealth <= 0)
                throw new ArgumentException("Health must be positive");
            if (maxSP <= 0)
                throw new ArgumentException("SP must be positive");
            return new UnitStats(
                new Health(maxHealth, maxHealth),
                new SpellPoints(maxSP, maxSP),
                physic, defense, magic, speed
            );
        }

        private UnitStats(Health health, SpellPoints spellPoints,
                      int physic, int defense, int magic, int speed)
        {
            Health = health; SpellPoints = spellPoints;
            Physic = physic; Defense = defense; Magic = magic; Speed = speed;
        }

        public UnitStats WithDamage(int amount) => this with { Health = Health.WithDamage(amount) };
        public UnitStats WithHealing(int amount) => this with { Health = Health.WithHealing(amount) };
        public UnitStats WithSpendSP(int amount) => this with { SpellPoints = SpellPoints.WithSpend(amount) };
        public UnitStats WithRestoreSP(int amount) => this with { SpellPoints = SpellPoints.WithRestore(amount) };
    }
}
