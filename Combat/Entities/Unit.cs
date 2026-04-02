using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using UnceasingFear.Domain.Combat.Enums;
using UnceasingFear.Domain.Combat.ValueObjects;
using UnceasingFear.Domain.Combat.ValueObjects.Abilities;
using UnceasingFear.Domain.Combat.ValueObjects.Stats;
using UnceasingFear.Domain.Shared;
using static UnceasingFear.Domain.Combat.Events.CombatEvents;

namespace UnceasingFear.Domain.Combat.Entities
{
    public class Unit : Entity
    {
        public UnitId Id { get; }
        public string Name { get; }
        public bool IsAlly { get; }
        public int SlotIndex { get; private set; }

        public UnitStats Stats { get; private set; }
        public IReadOnlyList<Ability> Abilities { get; private set; }
        public TurnProgress TurnProgress { get; private set; }

        public bool IsAlive => Stats.IsAlive;
        public bool CanAct => TurnProgress.IsReady && IsAlive;

        public Unit(UnitId id, string name, bool isAlly, UnitStats stats, IEnumerable<Ability> abilities, TurnProgress? turnProgress = null)
        {
            Id = id;
            Name = name; 
            IsAlly = isAlly; 
            Stats = stats;

            TurnProgress = turnProgress ?? new TurnProgress();
            Abilities = abilities?.TakeLast(4).ToList() ?? new();
        }
        public void AdvanceGauge(float baseAmount, float speedModifier = 1f)
        {
            TurnProgress = TurnProgress.Advance(baseAmount, speedModifier);
            if (TurnProgress.IsReady)
                AddDomainEvent(new UnitTurnReadyEvent(Id, Name, IsAlly));
        }

        public void ConsumeTurn()
            => TurnProgress = TurnProgress.Consume();
        public void AssignToSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= 6) 
                throw new ArgumentException("Invalid slot index");
            SlotIndex = slotIndex;
        }
        public AbilityResult UseAbility(int index)
        {
            if (!CanAct)
                return new AbilityResult.Failure("Unit cannot act yet");

            if (index < 0 || index >= Abilities.Count)
                return new AbilityResult.Failure("Invalid ability slot");

            var ability = Abilities[index];

            foreach (var cost in ability.Costs)
            {
                if (!CanPay(cost))
                    return new AbilityResult.Failure($"Cannot pay {cost.Stat}");
            }

            foreach (var cost in ability.Costs)
                PayCost(cost);

            var effect = ability.CalculateEffect(Stats);

            AddDomainEvent(new AbilityUsedEvent(Id, ability.Id, ability.Name));

            return new AbilityResult.Success(effect);
        }
        private bool CanPay(Cost cost) => cost.Stat switch
        {
            CostType.HP => Stats.Health.Current >= cost.Value,
            CostType.SP => Stats.SpellPoints.Current >= cost.Value,
            _ => true
        };

        private void PayCost(Cost cost)
        {
            switch (cost.Stat)
            {
                case CostType.HP: Stats = Stats.WithDamage((int)cost.Value); break;
                case CostType.SP: Stats = Stats.WithSpendSP((int)cost.Value); break;
            }
        }
        
        public void TakeDamage(int amount)
        {
            Stats = Stats.WithDamage(amount);

            AddDomainEvent(new UnitDamagedEvent(Id, Name, amount, Stats.Health));

            if (!IsAlive) 
                AddDomainEvent(new UnitDiedEvent(Id, Name, IsAlly));
        }
        
    }
}
