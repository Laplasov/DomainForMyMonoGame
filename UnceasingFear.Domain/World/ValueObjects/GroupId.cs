using System;
using System.Collections.Generic;
using System.Text;

namespace UnceasingFear.Domain.World.ValueObjects
{
    public readonly record struct EntityId(string Value)
    {
        public static EntityId From(string name) => new(name);
        public override string ToString() => Value;
    }
}
