using System;
using System.Collections.Generic;
using System.Text;
using UnceasingFear.Domain.Combat.Entities;
using UnceasingFear.Domain.Combat.Enums;
using UnceasingFear.Domain.Combat.ValueObjects.Abilities;

namespace UnceasingFear.Domain.Combat.Services
{
    public class TargetResolver : ITargetResolver
    {
        public IEnumerable<Unit> ResolveTargets(Unit caster, Ability ability, int selectedSlot, IEnumerable<Unit> allUnits)
        {
            var potential = allUnits.Where(u => u.IsAlive);

            // Filter by side if ability targets enemies/allies
            if (ability.Target == Target.Enemy)
                potential = potential.Where(u => u.IsAlly != caster.IsAlly);
            else if (ability.Target == Target.Ally)
                potential = potential.Where(u => u.IsAlly == caster.IsAlly);

            // Apply range rules (you write these — you know your slot layout)
            return ability.Range switch
            {
                TargetRange.Melee => potential.Where(u => u.SlotIndex == selectedSlot && u.SlotIndex < 3), // Front row only
                TargetRange.Range => potential.Where(u => u.SlotIndex == selectedSlot),
                TargetRange.All => potential,
                _ => Enumerable.Empty<Unit>()
            };
        }
    }
}
