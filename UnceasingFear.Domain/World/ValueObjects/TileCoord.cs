using System;
using System.Collections.Generic;
using System.Text;

namespace UnceasingFear.Domain.World.ValueObjects
{
    public readonly record struct TileCoord(int X, int Y)
    {
        public static TileCoord Zero => new(0, 0);
        public override string ToString() => $"({X},{Y})";
    }
}
