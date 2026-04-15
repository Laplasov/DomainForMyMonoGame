using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Application.Combat.Snapshots;
using UnceasingFear.Domain.Combat.Aggregates;
using UnceasingFear.Domain.Combat.Entities;
using UnceasingFear.Domain.Combat.Services;
using UnceasingFear.Domain.Combat.ValueObjects;
using UnceasingFear.Domain.Shared.Events;
using UnceasingFear.Domain.Shared.ValueObjects;
using static UnceasingFear.Domain.Combat.Events.CombatEvents;
using static UnceasingFear.Domain.Combat.ValueObjects.BattleState;

namespace UnceasingFear.Application.Combat
{
    public class BattleApplicationService
    {
        private readonly Battle _battle;
        private readonly ITurnOrderService _turnOrder;
        private readonly ITargetResolver _targetResolver;
        private readonly IEventDispatcher _dispatcher;

        private Unit? _currentActor;
        public bool IsWaitingForPlayerInput =>
            _currentActor != null && _currentActor.IsAlly;

        public BattleApplicationService(
            IReadOnlyList<UnitProfile> allyProfiles,
            IReadOnlyList<UnitProfile> enemyProfiles,
            ITurnOrderService turnOrder,
            ITargetResolver targetResolver,
            IEventDispatcher dispatcher)
        {
            _turnOrder = turnOrder;
            _targetResolver = targetResolver;
            _dispatcher = dispatcher;
            _battle = new Battle();

            int allySlot = 0;
            foreach (var profile in allyProfiles)
            {
                var unit = new Unit(UnitId.New(), true, profile.AssignToSlot(allySlot));
                _battle.AddUnit(unit);
                allySlot++;
            }

            int enemySlot = 0;
            foreach (var profile in enemyProfiles)
            {
                var unit = new Unit(UnitId.New(), false, profile.AssignToSlot(enemySlot));
                _battle.AddUnit(unit);
                enemySlot++;
            }

            _dispatcher.Subscribe<SelectAbilityEventCommand>(OnAbilitySelected);
            PublishPendingEvents();
        }

        public void Update(float deltaTime)
        {
            if (_currentActor != null)
            {
                if (_currentActor.IsAlly)
                    return; // waiting for player input via SelectAbilityEventCommand
                else
                {
                    ProcessEnemyTurn(_currentActor);
                    return;
                }
            }

            _turnOrder.AdvanceGauges(_battle.Units, deltaTime);

            var ready = _turnOrder.GetReadyUnitsInOrder(_battle.Units);
            if (!ready.Any()) return;

            _currentActor = ready.First();

            var state = _battle.ComputeState(_currentActor);
            _battle.TransitionTo(state);
            PublishPendingEvents();

            if (_battle.State is Victory || _battle.State is Lost)
            {
                _dispatcher.Unsubscribe<SelectAbilityEventCommand>();
                _currentActor = null;
            }
        }

        private void OnAbilitySelected(SelectAbilityEventCommand e)
        {
            if (_currentActor == null || !_currentActor.IsAlly)
                return;

            var ability = _currentActor.Profile.Abilities[e.AbilitySlot];
            var targets = _targetResolver.ResolveTargets(
                _currentActor, ability, e.TargetSlot, _battle.Units);

            _battle.ApplyAbility(_currentActor, e.AbilitySlot, targets);
            PublishPendingEvents();
            _currentActor = null;
        }

        private void ProcessEnemyTurn(Unit enemy)
        {
            // Simple AI: use ability 0 against a random alive ally
            var targets = _battle.Units
                .Where(u => u.IsAlly && u.IsAlive)
                .Take(1);

            _battle.ApplyAbility(enemy, 0, targets);
            PublishPendingEvents();
            _currentActor = null;
        }

        private void PublishPendingEvents()
        {
            foreach (var e in _battle.PullDomainEvents())
                _dispatcher.Dispatch(e);

            foreach (var unit in _battle.Units)
                foreach (var e in unit.PullDomainEvents())
                    _dispatcher.Dispatch(e);
        }

        // Read model for presentation layer — no domain types exposed
        public BattleSnapshot GetSnapshot()
        {
            return new BattleSnapshot(
                _battle.Units.Select(u => new UnitSnapshot(
                    u.Id.Value,
                    u.Name,
                    u.IsAlly,
                    u.Profile.SlotIndex,
                    u.Profile.Stats.Health.Current,
                    u.Profile.Stats.Health.Max,
                    u.Profile.Stats.SpellPoints.Current,
                    u.Profile.Stats.SpellPoints.Max,
                    u.IsAlive,
                    u.TurnProgress.Value
                )).ToList(),
                _battle.State.GetType().Name,
                _currentActor?.Id.Value
            );
        }
    }
}