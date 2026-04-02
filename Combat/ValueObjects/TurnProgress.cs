using System;
using System.Collections.Generic;
using System.Text;

namespace UnceasingFear.Domain.Combat.ValueObjects
{
    public readonly record struct TurnProgress
    {
        private const float MaxGauge = 100f;
        public float Value { get; }
        public bool IsReady => Value >= MaxGauge;

        public TurnProgress(float value = 0f)
            => Value = Math.Clamp(value, 0, float.MaxValue);

        public TurnProgress Advance(float amount, float speedModifier = 1f)
            => new TurnProgress(Value + amount * speedModifier);

        public TurnProgress Consume() => new TurnProgress(Value - MaxGauge);
    }
}
