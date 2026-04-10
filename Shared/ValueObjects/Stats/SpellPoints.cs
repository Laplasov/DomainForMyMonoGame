using System;
using System.Collections.Generic;
using System.Text;

namespace UnceasingFear.Domain.Shared.ValueObjects.Stats
{
    public readonly record struct SpellPoints
    {
        public int Current { get; }
        public int Max { get; }

        public SpellPoints(int current, int max)
        {
            if (max <= 0) throw new ArgumentException("Max SP must be positive");
            Current = Math.Clamp(current, 0, max);
            Max = max;
        }

        public SpellPoints WithSpend(int amount)
            => new SpellPoints(Math.Max(0, Current - amount), Max);

        public SpellPoints WithRestore(int amount)
            => new SpellPoints(Math.Min(Max, Current + amount), Max);
    }
}
