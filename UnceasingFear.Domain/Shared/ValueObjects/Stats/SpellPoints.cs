using System;
using System.Collections.Generic;
using System.Text;
using UnceasingFear.Domain.Shared.ValueObjects.Stats;

namespace UnceasingFear.Domain.Shared.ValueObjects.Stats
{
    public readonly record struct SpellPoints(int Current)
    {
        public SpellPoints WithSpend(int amount, int effectiveMax)
            => new(Math.Clamp(Current - amount, 0, effectiveMax));

        public SpellPoints WithRestore(int amount, int effectiveMax)
            => new(Math.Clamp(Current + amount, 0, effectiveMax));
    }
}
