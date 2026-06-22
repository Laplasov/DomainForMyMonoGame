using System.Diagnostics;
using UnceasingFear.Application.Combat.Snapshots;
using UnceasingFear.Application.Commands;
using UnceasingFear.Application.World;
using UnceasingFear.Domain.Combat.Aggregates;
using UnceasingFear.Domain.Combat.Entities;
using UnceasingFear.Domain.Combat.Enums;
using UnceasingFear.Domain.Combat.Events;
using UnceasingFear.Domain.Combat.Services;
using UnceasingFear.Domain.Combat.ValueObjects;
using UnceasingFear.Domain.Shared.Events;
using UnceasingFear.Domain.Shared.ValueObjects;
using static UnceasingFear.Domain.Combat.ValueObjects.BattleState;
using static UnceasingFear.Domain.Shared.Events.SharedEvents;

namespace UnceasingFear.Application.Combat
{
    public record struct SelectAbilityEventCommand(int TargetSlot, int AbilitySlot); 
    public record struct UpdateCommand(float deltaTime);
    public record struct PassTurnCommand();
    public record struct RunFromBattleCommand();
    public class BattleApplicationService
    {
        private readonly Battle _battle;
        private readonly ITurnOrderService _turnOrder;
        private readonly ITargetResolver _targetResolver;
        public IEventDispatcher EventDispatcher { get; }
        public ICommandDispatcher CommandDispatcher { get; }

        private Unit? _currentActor;
        public bool IsWaitingForPlayerInput =>
            _currentActor != null && _currentActor.IsAlly;
        public BattleApplicationService(
            IReadOnlyList<UnitProfile> allyProfiles, 
            IReadOnlyList<UnitProfile> enemyProfiles, 
            IEventDispatcher dispatcher, 
            ICommandDispatcher commandDispatcher, 
            TurnOrderService turnOrderService,
            TargetResolver targetResolver,
            Battle battle
            )
        {
            _turnOrder = turnOrderService;
            _targetResolver = targetResolver;
            _battle = battle;

            EventDispatcher = dispatcher;
            CommandDispatcher = commandDispatcher;

            foreach (var profile in allyProfiles)
            {
                var unit = new Unit(UnitId.New(), true, profile);
                _battle.AddUnit(unit);
            }

            foreach (var profile in enemyProfiles)
            {
                var unit = new Unit(UnitId.New(), false, profile);
                _battle.AddUnit(unit);
            }

            CommandDispatcher.Register<SelectAbilityEventCommand>(OnAbilitySelected);
            CommandDispatcher.Register<UpdateCommand>(Update);
            CommandDispatcher.Register<PassTurnCommand>(OnPassTurn);
            CommandDispatcher.Register<RunFromBattleCommand>(OnRunFromBattle);
            PublishPendingEvents();
        }
        public void Update(UpdateCommand cmd)
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

            _turnOrder.AdvanceGauges(_battle.Units, cmd.deltaTime);

            var ready = _turnOrder.GetReadyUnitsInOrder(_battle.Units);
            if (!ready.Any()) return;

            _currentActor = ready.First();

            var state = _battle.ComputeState(_currentActor);
            _battle.TransitionTo(state);
            PublishPendingEvents();

            CheckAndConcludeBattle();
        }

        private void UnsubscribeFromCommands()
        {
            CommandDispatcher.Unsubscribe<SelectAbilityEventCommand>();
            CommandDispatcher.Unsubscribe<UpdateCommand>();
            CommandDispatcher.Unsubscribe<PassTurnCommand>();
            CommandDispatcher.Unsubscribe<RunFromBattleCommand>();
        }

        private void OnAbilitySelected(SelectAbilityEventCommand cmd)
        {
            if (_currentActor == null || !_currentActor.IsAlly)
                return;

            var ability = _currentActor.Profile.Abilities[cmd.AbilitySlot];
            var targets = _targetResolver.ResolveTargets(
                _currentActor, ability, cmd.TargetSlot, _battle.Units);

            _battle.ApplyAbility(_currentActor, cmd.AbilitySlot, targets);
            PublishPendingEvents();

            if (CheckAndConcludeBattle()) return;

            _currentActor = null;
        }
        private void OnPassTurn(PassTurnCommand cmd)
        {
            if (_currentActor == null || !_currentActor.IsAlly) 
                return;

            _currentActor.ConsumeTurn();
            PublishPendingEvents();

            if (CheckAndConcludeBattle()) return;

            _currentActor = null;
        }

        private void OnRunFromBattle(RunFromBattleCommand cmd)
        {
            var exitParty = GetProfiles(true);
            var enemyParty = GetProfiles(false);
            var collectedLoot = CollectLoot();

            _battle.ConcludeBattle(exitParty, enemyParty, collectedLoot);
            PublishPendingEvents();

            UnsubscribeFromCommands();

            _currentActor = null;
        }
        private IReadOnlyList<UnitProfile> GetProfiles(bool isAlly)
        {
            return _battle.Units
                .Where(u => u.IsAlly == isAlly)
                .Select(u => u.Profile)
                .ToList()
                .AsReadOnly();
        }
        private IReadOnlyList<Item> CollectLoot()
        {
            return _battle.Units
                .Where(u => !u.IsAlly && !u.IsAlive)
                .SelectMany(u => u.Profile.Stash)
                .ToList()
                .AsReadOnly();
        }

        private void ProcessEnemyTurn(Unit enemy)
        {
            // Simple AI: use ability 0 against a random alive ally
            var targets = _battle.Units
                .Where(u => u.IsAlly && u.IsAlive)
                .Take(1);

            _battle.ApplyAbility(enemy, 0, targets);
            PublishPendingEvents();

            if (CheckAndConcludeBattle()) return;

            _currentActor = null;
        }

        private bool CheckAndConcludeBattle()
        {
            // Directly check the domain aggregate's win/loss conditions
            if (_battle.IsVictory || _battle.IsDefeat)
            {
                var state = _battle.IsVictory ? (BattleState)new Victory() : new Lost();
                _battle.TransitionTo(state);

                var exitParty = GetProfiles(true);
                var enemyParty = GetProfiles(false);
                var collectedLoot = CollectLoot();

                _battle.ConcludeBattle(exitParty, enemyParty, collectedLoot);
                PublishPendingEvents();

                UnsubscribeFromCommands();

                _currentActor = null;
                return true; // Battle ended
            }
            return false; // Battle continues
        }

        private void PublishPendingEvents()
        {
            foreach (var e in _battle.PullDomainEvents())
                EventDispatcher.Dispatch(e);

            foreach (var unit in _battle.Units)
                foreach (var e in unit.PullDomainEvents())
                    EventDispatcher.Dispatch(e);
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
                    u.TurnProgress.Value,
                    u.Profile.Stats.Physic,
                    u.Profile.Stats.Defense,
                    u.Profile.Stats.Magic,
                    u.Profile.Stats.Speed,
                    u.Profile.Abilities.Select(a => new AbilitySnapshot(
                        a.Id,
                        a.Name,
                        a.Description,
                        (int)(a.Costs.FirstOrDefault(c => c.Stat == CostType.SP).Value)
                    )).ToList().AsReadOnly(),
                    u.Profile.Stash,
                    u.Profile.EquippedItems
                )).ToList(),
                _battle.State.GetType().Name,
                _currentActor?.Id.Value,
                IsWaitingForPlayerInput
            );
        }

    }
}