using System;
using System.Collections.Generic;
using System.Text;

namespace UnceasingFear.Domain.Shared.ValueObjects.Stats
{
    public readonly record struct Health
    {
        public int Current { get; }
        public int Max { get; }
        public bool IsAlive => Current > 0;

        public Health(int current, int max)
        {
            if (max <= 0) throw new ArgumentException("Max health must be positive");
            Current = Math.Clamp(current, 0, max);
            Max = max;
        }

        public Health WithDamage(int amount)
            => new Health(Math.Max(0, Current - amount), Max);

        public Health WithHealing(int amount)
            => new Health(Math.Min(Max, Current + amount), Max);
    }
}
