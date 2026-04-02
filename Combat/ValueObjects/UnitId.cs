using System;
using System.Collections.Generic;
using System.Text;

namespace UnceasingFear.Domain.Combat.ValueObjects
{
    public readonly record struct UnitId(Guid Value)
    {
        public static UnitId New() => new(Guid.NewGuid());
        public override string ToString() => Value.ToString();
    }
}
