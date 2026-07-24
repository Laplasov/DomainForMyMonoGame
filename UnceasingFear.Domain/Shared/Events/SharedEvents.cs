using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.Combat.ValueObjects;
using UnceasingFear.Domain.Shared.ValueObjects;

namespace UnceasingFear.Domain.Shared.Events
{
    public class SharedEvents
    {
        public record struct EnterBattleEvent(IReadOnlyList<UnitProfile> AllyProfiles, IReadOnlyList<UnitProfile> EnemyProfiles) : IDomainEvent;
        public record struct OutOfBattleEvent(IReadOnlyList<UnitProfile> AllyProfiles, IReadOnlyList<UnitProfile> EnemyProfiles, IReadOnlyList<Item> CollectedLoot) : IDomainEvent;
        public record struct ExitGame() : IDomainEvent;
        public record struct PauseGame(bool ShouldPause) : IDomainEvent;
        public record struct DialogueStartedEvent(string Speaker, string Text, IReadOnlyList<DialogueChoice> Choices) : IDomainEvent;
        public record struct DialogueAdvancedEvent(string Speaker, string Text, IReadOnlyList<DialogueChoice> Choices) : IDomainEvent;
        public record struct DialogueEndEvent() : IDomainEvent;
        public record struct CauldronOpenedEvent() : IDomainEvent;
    }
}
