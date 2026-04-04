using System;
using System.Collections.Generic;
using System.Text;

namespace UnceasingFear.Domain.World.ValueObjects
{
    public readonly record struct TileId(int Value);
    public readonly record struct MovementSpeed(float Value);
    public readonly record struct Template(string TemplateName, IReadOnlyList<string> UnitNames);
}
