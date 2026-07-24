using UnceasingFear.Domain.Shared.Enums;
using UnceasingFear.Domain.Shared.ValueObjects.Abilities;
using UnceasingFear.Domain.Shared.ValueObjects.Stats;

namespace UnceasingFear.Domain.Shared.ValueObjects
{
    public readonly record struct UnitProfile
    {
        public string Name { get; }
        public int SlotIndex { get; init; }
        public UnitStats BaseStats { get; init; }
        public IReadOnlyList<Ability> Abilities { get; }
        public IReadOnlyList<Item> Stash { get; init; }
        public IReadOnlyList<Item> EquippedItems { get; init; }
        public ConsumedEssence ConsumedEssences { get; init; }
        public Identity Identity { get; init; }
        public UnitStats Stats  => BaseStats + ConsumedEssences.CalculateTotalBonus();
        UnitProfile(string name, int slot, UnitStats stats, IReadOnlyList<Ability> abilities, IReadOnlyList<Item> lootDrops, IReadOnlyList<Item> equippedItems, ConsumedEssence consumedEssences, Identity identity)
        {
            Name = name; SlotIndex = slot; BaseStats = stats; Abilities = abilities; Stash = lootDrops; EquippedItems = equippedItems; ConsumedEssences = consumedEssences; Identity = identity;
        }
        public static UnitProfile Create(string name, int slot, UnitStats stats, IEnumerable<Ability> abilities, IEnumerable<Item> lootDrops, IEnumerable<Item> equippedItems, ConsumedEssence consumedEssences, Identity identity) 
            => new(name, slot, stats, abilities.ToList().AsReadOnly(), lootDrops.ToList().AsReadOnly(), equippedItems.ToList().AsReadOnly(), consumedEssences, identity);

        public bool CanPay(Cost cost) => cost.Stat switch
        {
            CostType.HP => BaseStats.Health.Current >= cost.Value,
            CostType.SP => BaseStats.SpellPoints.Current >= cost.Value,
            _ => true
        };
        public UnitProfile PayCost(Cost cost) => cost.Stat switch
        {
            CostType.HP => this with { BaseStats = BaseStats.WithDamage((int)cost.Value, ConsumedEssences) },
            CostType.SP => this with { BaseStats = BaseStats.WithSpendSP((int)cost.Value, ConsumedEssences) },
            _ => this 
        };
        public UnitProfile TakeDamage(int amount) => this with { BaseStats = BaseStats.WithDamage(amount, ConsumedEssences) };
        public UnitProfile AssignToSlot(int slotIndex)
        {
            if (slotIndex <= 0 || slotIndex > 9) 
                throw new ArgumentException("Invalid slot index");
            return this with { SlotIndex = slotIndex };
        }

        public UnitProfile RemoveFromStash(IEnumerable<Item> items)
        {
            var currentLoots = Stash.ToList();

            foreach (var itemToRemove in items)
            {
                // Match by Type and Name to ensure we are removing the exact same item
                var existingIndex = currentLoots.FindIndex(i => i.Type == itemToRemove.Type && i.Name == itemToRemove.Name);

                if (existingIndex >= 0)
                {
                    var existing = currentLoots[existingIndex];
                    int newQuantity = existing.Quantity - itemToRemove.Quantity;

                    if (newQuantity <= 0)
                    {
                        // If quantity is 0 or less, remove the item completely from the list
                        currentLoots.RemoveAt(existingIndex);
                    }
                    else
                    {
                        // Otherwise, create a new Item with the reduced quantity
                        currentLoots[existingIndex] = new Item(
                            existing.Id,
                            existing.Type,
                            existing.Name,
                            newQuantity,
                            existing.Value,
                            existing.Description,
                            existing.IsStackable
                        );
                    }
                }
            }

            // Return the new immutable struct with the updated stash
            return this with { Stash = currentLoots.AsReadOnly() };
        }

        public UnitProfile AddToStash(IEnumerable<Item> items)
        {
            var currentLoots = Stash.ToList();

            foreach (var item in items)
            {
                var existingIndex = item.IsStackable ? currentLoots.FindIndex(i => i.Type == item.Type && i.Name == item.Name) : -1;
                if (existingIndex >= 0)
                {
                    var existing = currentLoots[existingIndex];

                    currentLoots[existingIndex] = new Item(
                        existing.Id,
                        existing.Type,
                        existing.Name,
                        existing.Quantity + item.Quantity,
                        existing.Value,
                        existing.Description,
                        existing.IsStackable
                    );
                }
                else
                {
                    currentLoots.Add(item);
                }
            }
            return this with { Stash = currentLoots.AsReadOnly() };
        }

        public static (bool, List<UnitProfile>) TryAddUnitsToSlots(IReadOnlyList<UnitProfile> UnitsToAdd, IReadOnlyList<UnitProfile> profiles)
        {
            var newProfiles = profiles.ToList();

            // Get all occupied slots
            var occupiedSlots = newProfiles.Select(p => p.SlotIndex).ToHashSet();

            // Find all available slots (1-9)
            var availableSlots = new List<int>();
            for (int i = 1; i <= 9; i++)
            {
                if (!occupiedSlots.Contains(i))
                    availableSlots.Add(i);
            }

            // Check if we have enough space for all units
            if (availableSlots.Count < UnitsToAdd.Count)
                return (false, newProfiles);

            // Add each unit to an available slot
            for (int i = 0; i < UnitsToAdd.Count; i++)
            {
                var unit = UnitsToAdd[i].AssignToSlot(availableSlots[i]);
                newProfiles.Add(unit);
            }

            return (true, newProfiles);
        }

    }
}