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
        public record EnterBattleEvent(IReadOnlyList<UnitProfile> AllyProfiles, IReadOnlyList<UnitProfile> EnemyProfiles) : IDomainEvent;
        public record OutOfBattleEvent(IReadOnlyList<UnitProfile> AllyProfiles, IReadOnlyList<UnitProfile> EnemyProfiles) : IDomainEvent;
    }
}
