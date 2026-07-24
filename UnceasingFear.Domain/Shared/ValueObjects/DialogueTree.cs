using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.Shared.Enums;

namespace UnceasingFear.Domain.Shared.ValueObjects
{
    public enum ConditionType { HasItem, TakeItem, StatCheck, TakeUnit }
    public enum ChoiceAction { Continue, End, UnitShop, ItemShop, AttackCurrent, AttackFromSource, OpenCauldron }

    public readonly record struct DialogueRequest(IReadOnlyList<UnitProfile> Profiles);
    public readonly record struct DialogueResult(DialogueTree DialogueTree, IReadOnlyList<UnitProfile> UpdatedProfiles);
    public readonly record struct ConditionResult(IReadOnlyList<UnitProfile> UpdatedProfiles, string Target);

    public readonly record struct DialogueCondition(ConditionType Type, string Condition);
    public readonly record struct DialogueNode(string Id, string Speaker, string Text, IReadOnlyList<DialogueChoice> Choices);
    public readonly record struct DialogueTarget(string Target)
    {
        public string Left => Target.Contains(':') ? Target.Split(':')[0] : Target;
        public string Right => Target.Contains(':') ? Target.Split(':')[1] : Left;
    }
    public readonly record struct DialogueChoice(
        string Text, 
        ChoiceAction Action, 
        DialogueTarget Target, 
        IReadOnlyList<Item> ReciveItems,
        IReadOnlyList<UnitProfile> ReciveUnits,
        IReadOnlyList<DialogueCondition> Condition
        )
    {
        public ConditionResult EvaluateAndApplyEffects(DialogueRequest request)
        {
            var profiles = request.Profiles;
            var playerProfile = request.Profiles.FirstOrDefault(p => p.Name == "Player");

            if (string.IsNullOrEmpty(playerProfile.Name) || profiles == null)
                return new ConditionResult(request.Profiles, Target.Right);

            var currentProfile = playerProfile;

            foreach (DialogueCondition condition in Condition)
            {
                (bool passed, UnitProfile updatedProfile) = condition.Type switch
                {
                    ConditionType.HasItem => HasItem(condition, currentProfile),
                    ConditionType.TakeItem => TakeItem(condition, currentProfile),
                    ConditionType.StatCheck => StatCheck(condition, currentProfile),
                    _ => (true, currentProfile)
                };

                currentProfile = updatedProfile;

                if (!passed)
                    return new ConditionResult(request.Profiles, Target.Right);
            }

            currentProfile = currentProfile.AddToStash(ReciveItems);

            var newProfiles = profiles.Select(p => p.Name == "Player" ? currentProfile : p).ToList();
            if (ReciveUnits.Count > 0)
            {
                (bool hasSpace, var profilesWithUnits) = TryAddUnitsToSlots(ReciveUnits, newProfiles);

                if (!hasSpace)
                    return new ConditionResult(request.Profiles, Target.Right);

                newProfiles = profilesWithUnits;
            }

            return new ConditionResult(newProfiles, Target.Left);
        }
        private (bool, List<UnitProfile>) TryAddUnitsToSlots(IReadOnlyList<UnitProfile> UnitsToAdd, IReadOnlyList<UnitProfile> profiles)
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


        private (bool, UnitProfile) TakeItem(DialogueCondition Condition, UnitProfile profile)
        {
            var text = Condition.Condition.Split(':');
            int quantity = 1;
            if (text.Length > 1)
            {
                Int32.TryParse(text[1], out quantity);
            }
            string itemName = text[0];

            // 1. Check if we actually have enough of the item
            var existingItem = profile.Stash.FirstOrDefault(i => i.Name == itemName);
            if (existingItem.Name == null || existingItem.Quantity < quantity)
            {
                return (false, profile); // Condition failed
            }

            // 2. Create a temporary Item struct to pass to RemoveFromStash
            var itemToRemove = new Item(Guid.Empty, existingItem.Type, itemName, quantity, 0, "", existingItem.IsStackable);

            // 3. Remove the item and return the updated profile
            var updatedProfile = profile.RemoveFromStash(new[] { itemToRemove });
            return (true, updatedProfile);
        }
        private (bool, UnitProfile) HasItem(DialogueCondition Condition, UnitProfile profile)
        {
            var parts = Condition.Condition.Split(':');
            string itemName = parts[0];
            int quantity = parts.Length > 1 ? int.Parse(parts[1]) : 1;

            bool hasItem = profile.Stash.Any(i => i.Name == itemName && i.Quantity >= quantity);
            return (hasItem, profile);
        }

        private (bool, UnitProfile) StatCheck(DialogueCondition condition, UnitProfile profile)
        {
            var parts = condition.Condition.Split(':');
            string statName = parts[0];
            int requiredValue = parts.Length > 1 ? int.Parse(parts[1]) : 0;

            int statValue = GetStatValue(profile, statName);
            bool passed = statValue >= requiredValue;

            return (passed, profile);
        }
        private int GetStatValue(UnitProfile profile, string statName) => statName switch
        {
            "CurrentHp" => profile.Stats.Health.Current,
            "CurrentSp" => profile.Stats.SpellPoints.Current,
            "Physic" => profile.Stats.Physic,
            "Defense" => profile.Stats.Defense,
            "Magic" => profile.Stats.Magic,
            "Speed" => profile.Stats.Speed,
            "MaxHp" => profile.Stats.MaxHp,
            "MaxSp" => profile.Stats.MaxSp,
            _ => 0
        };
    }
    public readonly record struct DialogueTree(string Id, DialogueNode CurrentNode, IReadOnlyList<DialogueNode> Nodes)
    {
        public static DialogueTree Empty => new(string.Empty, new DialogueNode(string.Empty, string.Empty, string.Empty, Array.Empty<DialogueChoice>()), Array.Empty<DialogueNode>());
        public DialogueTree SetNode(string id)
        {
            var node = Nodes.FirstOrDefault(n => n.Id == id);
            if (node.Id is null)
                throw new InvalidOperationException($"Dialogue node '{id}' not found in tree '{Id}'.");
            return this with { CurrentNode = node };
        }
        public DialogueResult UpdateCurrentNode(DialogueChoice choice, DialogueRequest request)
        {
            var result = choice.EvaluateAndApplyEffects(request);
            return new DialogueResult(this with { CurrentNode = Nodes.FirstOrDefault(n => n.Id == result.Target) }, result.UpdatedProfiles);
        }
    }
}
