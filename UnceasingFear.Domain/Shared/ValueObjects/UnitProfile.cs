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
        public IReadOnlyList<Item> Stash { get; init; }
        public IReadOnlyList<Item> EquippedItems { get; init; }
        UnitProfile(string name, int slot, UnitStats stats, IReadOnlyList<Ability> abilities, IReadOnlyList<Item> lootDrops, IReadOnlyList<Item> equippedItems)
        {
            Name = name; SlotIndex = slot;  Stats = stats; Abilities = abilities; Stash = lootDrops; EquippedItems = equippedItems;
        }
        public static UnitProfile Create(string name, int slot, UnitStats stats, IEnumerable<Ability> abilities, IEnumerable<Item> lootDrops, IEnumerable<Item> equippedItems) 
            => new(name, slot, stats, abilities.ToList().AsReadOnly(), lootDrops.ToList().AsReadOnly(), equippedItems.ToList().AsReadOnly());

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
            if (slotIndex <= 0 || slotIndex > 9) 
                throw new ArgumentException("Invalid slot index");
            return this with { SlotIndex = slotIndex };
        }

        public UnitProfile AddLoot(IEnumerable<Item> newLoots)
        {
            var currentLoots = Stash.ToList();

            foreach (var loot in newLoots)
            {
                // Find existing loot of the same Type and Name
                var existingIndex = currentLoots.FindIndex(l => l.Type == loot.Type && l.Name == loot.Name);

                if (existingIndex >= 0)
                {
                    // Stack it!
                    var existing = currentLoots[existingIndex];
                    currentLoots[existingIndex] = existing with { Quantity = existing.Quantity + loot.Quantity };
                }
                else
                {
                    // New item
                    currentLoots.Add(loot);
                }
            }
            return this with { Stash = currentLoots.AsReadOnly() };
        }
    }
}