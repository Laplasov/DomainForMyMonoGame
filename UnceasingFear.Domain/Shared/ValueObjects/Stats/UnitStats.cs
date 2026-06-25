using System;
using System.Collections.Generic;
using System.Text;
using UnceasingFear.Domain.Shared.Enums;

namespace UnceasingFear.Domain.Shared.ValueObjects.Stats
{
    public record UnitStats
    {
        public Health Health { get; init; }
        public SpellPoints SpellPoints { get; init; }
        public int MaxHp { get; init; }
        public int MaxSp { get; init; }
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
            return new UnitStats(maxHealth,maxSP, physic, defense, magic, speed);
        }
        private UnitStats(int maxHealth, int maxSP, int physic, int defense, int magic, int speed)
        {
            Health = new Health(maxHealth); SpellPoints = new SpellPoints(maxSP);
            Physic = physic; Defense = defense; Magic = magic; Speed = speed;
            MaxHp = maxHealth; MaxSp = maxSP;
        }

        public UnitStats WithDamage(int amount, ConsumedEssence ConsumedEssences) => this with { Health = Health.WithDamage(amount, MaxHp + ConsumedEssences.GetBonus(StatType.MaxHP)) };
        public UnitStats WithHealing(int amount, ConsumedEssence ConsumedEssences) => this with { Health = Health.WithHealing(amount, MaxHp + ConsumedEssences.GetBonus(StatType.MaxHP)) };
        public UnitStats WithSpendSP(int amount, ConsumedEssence ConsumedEssences) => this with { SpellPoints = SpellPoints.WithSpend(amount, MaxSp + ConsumedEssences.GetBonus(StatType.MaxSP)) };
        public UnitStats WithRestoreSP(int amount, ConsumedEssence ConsumedEssences) => this with { SpellPoints = SpellPoints.WithRestore(amount, MaxSp + ConsumedEssences.GetBonus(StatType.MaxSP)) };

        public static UnitStats operator +(UnitStats stats, StatDelta delta)
        {
            int effectiveMaxHp = stats.MaxHp + delta.MaxHp;
            int effectiveMaxSp = stats.MaxSp + delta.MaxSp;

            // Standard RPG rule: Gaining Max HP heals you by that amount. 
            // Losing Max HP (negative delta) reduces your current HP, but we clamp it so it doesn't go below 0.
            // We also clamp upwards to the new effectiveMax so Current can never exceed Max.
            int newCurrentHp = Math.Clamp(stats.Health.Current + delta.MaxHp, 0, effectiveMaxHp);
            int newCurrentSp = Math.Clamp(stats.SpellPoints.Current + delta.MaxSp, 0, effectiveMaxSp);

            return new UnitStats(
                maxHealth: effectiveMaxHp,   // Properly sets BaseMaxHp to the Effective Max in the returned view
                maxSP: effectiveMaxSp,       // Properly sets BaseMaxSp to the Effective Max in the returned view
                physic: stats.Physic + delta.Physic,
                defense: stats.Defense + delta.Defense,
                magic: stats.Magic + delta.Magic,
                speed: stats.Speed + delta.Speed
            ) with
            {
                // Override the constructor's Health/SP initialization to preserve the calculated Current values
                Health = new Health(newCurrentHp),
                SpellPoints = new SpellPoints(newCurrentSp)
            };
        }
    }
}
