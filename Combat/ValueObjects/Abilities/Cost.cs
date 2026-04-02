using UnceasingFear.Domain.Combat.Enums;

namespace UnceasingFear.Domain.Combat.ValueObjects.Abilities
{
    public readonly record struct Cost
    {
        public CostType Stat { get; }
        public float Value { get; }

        public Cost(CostType stat, float value)
        {
            Stat = stat;
            Value = value;
        }

    }
}
