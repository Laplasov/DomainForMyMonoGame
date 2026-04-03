using System;
using System.Collections.Generic;
using System.Text;

namespace UnceasingFear.Domain.World.ValueObjects
{
    public readonly record struct EnemyGroupId(string Value)
    {
        public static EnemyGroupId From(string name) => new(name);
        public override string ToString() => Value;
    }
}
