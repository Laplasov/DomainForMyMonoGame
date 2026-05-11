using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.Shared.ValueObjects;

namespace UnceasingFear.Application.Commands
{
    public class SharedCommands
    {
        public record struct EnterBattleCommand(IReadOnlyList<UnitProfile> AllyProfiles, IReadOnlyList<UnitProfile> EnemyProfiles);
    }
}
