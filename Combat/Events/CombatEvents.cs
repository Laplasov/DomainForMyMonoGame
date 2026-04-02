using UnceasingFear.Domain.Combat.ValueObjects;
using UnceasingFear.Domain.Combat.ValueObjects.Stats;
using UnceasingFear.Domain.Shared.Events;

namespace UnceasingFear.Domain.Combat.Events
{
    public class CombatEvents
    {
        public record UnitDiedEvent(UnitId UnitId, string Name, bool WasAlly) : IDomainEvent;
        public record UnitDamagedEvent(UnitId UnitId, string Name, int Amount, Health Remaining) : IDomainEvent;
        public record UnitTurnReadyEvent(UnitId UnitId, string Name, bool IsAlly) : IDomainEvent;
        public record AbilityUsedEvent(UnitId UnitId, string AbilityId, string AbilityName) : IDomainEvent;
        public record BattleStateChangedEvent(BattleState From, BattleState To) : IDomainEvent;
        public record UnitJoinedBattleEvent(UnitId UnitId, string Name, bool IsAlly) : IDomainEvent;
    }
}
