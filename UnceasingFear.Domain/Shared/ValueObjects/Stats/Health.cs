using System;
using System.Collections.Generic;
using System.Text;

namespace UnceasingFear.Domain.Shared.ValueObjects.Stats
{
    public readonly record struct Health(int Current)
    {
        public bool IsAlive => Current > 0;
        public Health WithDamage(int amount, int effectiveMax)
            => new(Math.Clamp(Current - amount, 0, effectiveMax));
        public Health WithHealing(int amount, int effectiveMax)
            => new(Math.Clamp(Current + amount, 0, effectiveMax));
    }
}
