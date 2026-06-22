using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.Shared.ValueObjects;

namespace UnceasingFear.Application.Combat.Snapshots
{
    public record struct AbilitySnapshot(
    string Id,
    string Name,
    string Description,
    int SpCost
);

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
        float TurnProgress,
        int Physic,
        int Defense,
        int Magic,
        int Speed,
        IReadOnlyList<AbilitySnapshot> Abilities,
        IReadOnlyList<Item> Stash,
        IReadOnlyList<Item> EquippedItems
    );

    public record struct BattleSnapshot(
        IReadOnlyList<UnitSnapshot> Units,
        string BattleState,
        Guid? CurrentActorId,
        bool IsWaitingForPlayerInput
    );

}
