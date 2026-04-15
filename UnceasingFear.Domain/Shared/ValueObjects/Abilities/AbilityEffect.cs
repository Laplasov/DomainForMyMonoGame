using System;
using System.Collections.Generic;
using System.Text;
using UnceasingFear.Domain.Combat.Enums;

namespace UnceasingFear.Domain.Shared.ValueObjects.Abilities
{
    public readonly record struct AbilityEffect
    {
        public int Power { get; }
        public IReadOnlyList<StatType> DamageTypes { get; }
        public AbilityEffect(int power, IReadOnlyList<StatType> damageTypes)
        {
            Power = power;
            DamageTypes = damageTypes;
        }
    }
}
