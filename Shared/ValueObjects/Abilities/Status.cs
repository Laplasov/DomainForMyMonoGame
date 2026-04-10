using UnceasingFear.Domain.Combat.Enums;

namespace UnceasingFear.Domain.Shared.ValueObjects.Abilities
{
    public readonly record struct Status
    {
        public StatusEffectType Stat { get; }
        public float Value { get; }

        public Status(StatusEffectType stat, float value)
        {
            Stat = stat;
            Value = value;
        }
    }
}
