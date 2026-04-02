using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnceasingFear.Domain.Combat.Enums;
using UnceasingFear.Domain.Combat.ValueObjects.Stats;
using static UnceasingFear.Domain.Combat.ValueObjects.Abilities.AbilityResult;

namespace UnceasingFear.Domain.Combat.ValueObjects.Abilities
{
    public readonly record struct Ability
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public TargetRange Range { get; }
        public Target Target { get; }
        public IReadOnlyList<Scale> Scales { get; }
        public IReadOnlyList<Cost> Costs { get; }
        public IReadOnlyList<Status> StatusEffects { get; }
        private Ability(string id, string name, string description,
                   TargetRange range, Target target,
                   List<Scale> scales,
                   List<Cost> costs,
                   List<Status> statusEffects)
        {
            Id = id; 
            Name = name; 
            Description = description;
            Range = range; 
            Target = target;
            Scales = scales; 
            Costs = costs; 
            StatusEffects = statusEffects;
        }

        public static Ability Create(string id, string name, string description,
            TargetRange range, Target target,
            IEnumerable<Scale> scales,
            IEnumerable<Cost> costs,
            IEnumerable<Status> statusEffects)
        {
            if (string.IsNullOrWhiteSpace(id)) 
                throw new ArgumentException("Id required");
            if (string.IsNullOrWhiteSpace(name)) 
                throw new ArgumentException("Name required");

            return new Ability(
                id, 
                name, 
                description, 
                range, 
                target,
                scales?.ToList() ?? new(),
                costs?.ToList() ?? new(),
                statusEffects?.ToList() ?? new());
        }
        public AbilityEffect CalculateEffect(UnitStats stats)
        {
            int power = 0;
            foreach (var scale in Scales)
            {
                int statValue = scale.Stat switch
                {
                    StatType.Physic => stats.Physic,
                    StatType.Magic => stats.Magic,
                    StatType.Defense => stats.Defense,
                    StatType.Speed => stats.Speed,

                    _ => 0
                };
                power += (int)(statValue * scale.Percentage);
            }
            return new AbilityEffect(power, Scales.Select(s => s.Stat).ToList());
        }

    }
}
