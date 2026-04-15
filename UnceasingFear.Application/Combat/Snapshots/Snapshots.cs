using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnceasingFear.Application.Combat.Snapshots
{
    public record struct UnitSnapshot(
        Guid Id,
        string Name,
        bool IsAlly,
        int SlotIndex,
        int CurrentHp,
        int MaxHp,
        int CurrentSp,
        int MaxSp,
        bool IsAlive,
        float TurnProgress
    );

    public record struct BattleSnapshot(
        IReadOnlyList<UnitSnapshot> Units,
        string BattleState,
        Guid? CurrentActorId
    );
}
