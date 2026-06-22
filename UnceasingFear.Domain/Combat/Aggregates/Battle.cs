
using UnceasingFear.Domain.Combat.Entities;
using UnceasingFear.Domain.Combat.Enums;
using UnceasingFear.Domain.Combat.Events;
using UnceasingFear.Domain.Combat.ValueObjects;
using UnceasingFear.Domain.Shared;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Domain.Shared.ValueObjects.Abilities;
using static UnceasingFear.Domain.Combat.Events.CombatEvents;
using static UnceasingFear.Domain.Shared.Events.SharedEvents;
using static UnceasingFear.Domain.Shared.ValueObjects.Abilities.AbilityResult;

namespace UnceasingFear.Domain.Combat.Aggregates
{
    public class Battle : Entity
    {
        private readonly List<Unit> _units = new();
        public IReadOnlyList<Unit> Units => _units.AsReadOnly();


        private BattleState _battleState;
        public BattleState State => _battleState;

        public bool IsVictory => _units.Any(u => u.IsAlive && u.IsAlly) && !_units.Any(u => u.IsAlive && !u.IsAlly);
        public bool IsDefeat => !_units.Any(u => u.IsAlive && u.IsAlly);
        public Battle() => _battleState = new BattleState.Initializing();

        public BattleState ComputeState(Unit nextActor)
        {
            if (IsVictory) return new BattleState.Victory();
            if (IsDefeat) return new BattleState.Lost();
            return nextActor.IsAlly ? new BattleState.PlayerTurn() : new BattleState.EnemyTurn();
        }

        public void TransitionTo(BattleState newState)
        {
            var old = _battleState;
            _battleState = newState;
            AddDomainEvent(new BattleStateChangedEvent(old, newState));
        }

        public void ConcludeBattle(IReadOnlyList<UnitProfile> AllyProfiles, IReadOnlyList<UnitProfile> EnemyProfiles, IReadOnlyList<Item> CollectedLoot)
        {
            AddDomainEvent(new OutOfBattleEvent(AllyProfiles, EnemyProfiles, CollectedLoot));
        }

        public void AddUnit(Unit unit)
        {
            if (_units.Any(u => u.Id == unit.Id))
                throw new InvalidOperationException("Unit already in battle");
            _units.Add(unit);
            AddDomainEvent(new UnitJoinedBattleEvent(unit.Id, unit.Name, unit.IsAlly));
        }

        public Unit? GetUnit(UnitId id)
            => _units.FirstOrDefault(u => u.Id == id);

        public AbilityResult ApplyAbility(Unit actor, int abilityIndex, IEnumerable<Unit> resolvedTargets)
        {
            if (!_units.Contains(actor))
            {
                var failure = new AbilityResult.Failure("Actor not in this battle");
                AddDomainEvent(new CombatEvents.AbilityFailedEvent(actor.Name, failure.Reason));
                return failure;
            }

            if (!actor.CanAct)
            {
                var failure = new AbilityResult.Failure("Actor cannot act yet");
                AddDomainEvent(new CombatEvents.AbilityFailedEvent(actor.Name, failure.Reason));
                return failure;
            }

            if (!resolvedTargets.Any())
            {
                var failure = new AbilityResult.Failure("No valid targets");
                AddDomainEvent(new CombatEvents.AbilityFailedEvent(actor.Name, failure.Reason));
                return failure;
            }

            var result = actor.UseAbility(abilityIndex);

            if (result is AbilityResult.Success success)
            {
                foreach (var target in resolvedTargets.Where(t => t.IsAlive))
                    target.TakeDamage(success.Effect.Power);

                actor.ConsumeTurn();
                AddDomainEvent(new CombatEvents.AbilitySucceededEvent(
                actor.Name, actor.Profile.Abilities[abilityIndex].Name, success.Effect.Power));
            }
            else if (result is AbilityResult.Failure failure)
            {
                AddDomainEvent(new CombatEvents.AbilityFailedEvent(actor.Name, failure.Reason));
            }
            return result;
        }
    }
}

