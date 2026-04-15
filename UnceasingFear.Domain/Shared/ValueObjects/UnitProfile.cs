using UnceasingFear.Domain.Combat.Enums;
using UnceasingFear.Domain.Shared.ValueObjects.Abilities;
using UnceasingFear.Domain.Shared.ValueObjects.Stats;

namespace UnceasingFear.Domain.Shared.ValueObjects
{
    public readonly record struct UnitProfile
    {
        public string Name { get; }
        public int SlotIndex { get; init; }
        public UnitStats Stats { get; init; }
        public IReadOnlyList<Ability> Abilities { get; }
        UnitProfile(string name, int slot, UnitStats stats, IReadOnlyList<Ability> abilities)
        {
            Name = name; SlotIndex = slot;  Stats = stats; Abilities = abilities;
        }
        public static UnitProfile Create(string name, int slot, UnitStats stats, IEnumerable<Ability> abilities) 
            => new(name, slot, stats, abilities.ToList().AsReadOnly());

        public bool CanPay(Cost cost) => cost.Stat switch
        {
            CostType.HP => Stats.Health.Current >= cost.Value,
            CostType.SP => Stats.SpellPoints.Current >= cost.Value,
            _ => true
        };
        public UnitProfile PayCost(Cost cost) => cost.Stat switch
        {
            CostType.HP => this with { Stats = Stats.WithDamage((int)cost.Value) },
            CostType.SP => this with { Stats = Stats.WithSpendSP((int)cost.Value) },
            _ => this 
        };
        public UnitProfile TakeDamage(int amount) => this with { Stats = Stats.WithDamage(amount) };
        public UnitProfile AssignToSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= 6)
                throw new ArgumentException("Invalid slot index");
            return this with { SlotIndex = slotIndex };
        }

    }
}