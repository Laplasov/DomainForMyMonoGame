using System;
using System.Collections.Generic;
using System.Text;
using UnceasingFear.Domain.Combat.Entities;
using UnceasingFear.Domain.Shared.ValueObjects.Abilities;

namespace UnceasingFear.Domain.Combat.Services
{
    public interface ITargetResolver
    {
        public IEnumerable<Unit> ResolveTargets(Unit caster, Ability ability, int selectedSlot, IEnumerable<Unit> allUnits);
    }
}
