using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.Shared.Enums;

namespace UnceasingFear.Domain.Shared.ValueObjects
{
    public enum ChoiceAction { Continue, End, UnitShop, ItemShop, AttackCurrent, AttackFromSource }
    public enum ConditionType { HasItem, TakeItem, StatCheck }
    public readonly record struct DialogueNode(string Id, string Speaker, string Text, IReadOnlyList<DialogueChoice> Choices);
    public readonly record struct DialogueCondition(ConditionType Type, string Condition);
    public readonly record struct ConditionResult(UnitProfile Profile, string Target);
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
        IReadOnlyList<DialogueCondition> Condition
        )
    {
        public ConditionResult CheckConditionsAndReciveItems(UnitProfile profile)
        {
            var currentProfile = profile;
            foreach (DialogueCondition condition in Condition)
            {
                (bool passed, UnitProfile updatedProfile) = condition.Type switch
                {
                    ConditionType.HasItem => HasItem(condition, currentProfile),
                    ConditionType.TakeItem => TakeItem(condition, currentProfile),
                    ConditionType.StatCheck => StatCheck(condition, currentProfile),
                    _ => (true, currentProfile)
                };

                if (!passed)
                    return new ConditionResult(profile, Target.Right);

                currentProfile = updatedProfile;
            }
            currentProfile = currentProfile.AddToStash(ReciveItems);

            return new ConditionResult(currentProfile, Target.Left);
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
        public static DialogueTree Empty => new(string.Empty, default, Array.Empty<DialogueNode>());

        public (DialogueTree, UnitProfile) UpdateCurrentNode(DialogueChoice choice, UnitProfile profile)
        {
            var result = choice.CheckConditionsAndReciveItems(profile);
            return (this with { CurrentNode = Nodes.FirstOrDefault(n => n.Id == result.Target) }, result.Profile);
        }

        public DialogueTree SetNode(string id) =>
            this with { CurrentNode = Nodes.FirstOrDefault(n => n.Id == id) };
    }
}
