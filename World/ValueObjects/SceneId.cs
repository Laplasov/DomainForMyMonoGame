using System;
using System.Collections.Generic;
using System.Text;

namespace UnceasingFear.Domain.World.ValueObjects
{
    public readonly record struct SceneId(string Value)
    {
        public static SceneId From(string name) => new(name);
        public override string ToString() => Value;
    }
}
