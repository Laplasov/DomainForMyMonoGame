using System;
using System.Collections.Generic;
using System.Text;

namespace UnceasingFear.Domain.World.ValueObjects
{
    public readonly record struct GroupId(string Value)
    {
        public static GroupId From(string name) => new(name);
        public override string ToString() => Value;
    }
}
