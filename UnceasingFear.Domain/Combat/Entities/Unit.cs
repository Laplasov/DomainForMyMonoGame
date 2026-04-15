using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using UnceasingFear.Domain.Combat.Enums;
using UnceasingFear.Domain.Combat.ValueObjects;
using UnceasingFear.Domain.Shared;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Domain.Shared.ValueObjects.Abilities;
using UnceasingFear.Domain.Shared.ValueObjects.Stats;
using static UnceasingFear.Domain.Combat.Events.CombatEvents;

namespace UnceasingFear.Domain.Combat.Entities
{
    public class Unit : Entity
    {
        public UnitId Id { get; }
        public string Name => Profile.Name;
        public bool IsAlly { get; }
        public UnitProfile Profile { get; private set; }
        public TurnProgress TurnProgress { get; private set; }

        public bool IsAlive => Profile.Stats.IsAlive;
        public bool CanAct => TurnProgress.IsReady && IsAlive;

        public Unit(UnitId id, bool isAlly, UnitProfile profile, TurnProgress? turnProgress = null)
        {
            Id = id;
            IsAlly = isAlly;
            Profile = profile;
            TurnProgress = turnProgress ?? new TurnProgress();
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
            => Profile = Profile.AssignToSlot(slotIndex);
        public void TakeDamage(int amount)
        {
            Profile = Profile.TakeDamage(amount);

            AddDomainEvent(new UnitDamagedEvent(Id, Name, amount, Profile.Stats.Health));

            if (!IsAlive) 
                AddDomainEvent(new UnitDiedEvent(Id, Name, IsAlly));
        }
        public AbilityResult UseAbility(int index)
        {
            if (!CanAct)
                return new AbilityResult.Failure("Unit cannot act yet");

            if (index < 0 || index >= Profile.Abilities.Count)
                return new AbilityResult.Failure("Invalid ability slot");

            var ability = Profile.Abilities[index];

            foreach (var cost in ability.Costs)
            {
                if (!Profile.CanPay(cost))
                    return new AbilityResult.Failure($"Cannot pay {cost.Stat}");
            }

            foreach (var cost in ability.Costs)
                Profile = Profile.PayCost(cost);

            var effect = ability.CalculateEffect(Profile.Stats);

            AddDomainEvent(new AbilityUsedEvent(Id, ability.Id, ability.Name));

            return new AbilityResult.Success(effect);
        }
    }
}
