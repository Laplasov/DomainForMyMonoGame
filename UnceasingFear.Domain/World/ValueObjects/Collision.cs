using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace UnceasingFear.Domain.World.ValueObjects
{
    public readonly record struct Collision
    {

        private readonly bool[,] _grid;
        public int Width { get; }
        public int Height { get; }
        public record struct Walkable(bool x, bool y);
        public Collision(bool[,] grid)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            _grid = grid;
            Width = grid.GetLength(1); 
            Height = grid.GetLength(0); 
        }


        public Walkable IsWalkable(TileCoord tile, TileCoord lastTile)
        {
            // Treat the outer edge tiles as walls — valid interior is [1, Width-2]
            bool xInBounds = tile.X > 0 && tile.X < Width - 1;
            bool yInBounds = tile.Y > 0 && tile.Y < Height - 1;

            bool xWalkable = xInBounds && !_grid[lastTile.Y, tile.X];
            bool yWalkable = yInBounds && !_grid[tile.Y, lastTile.X];

            return new Walkable(xWalkable, yWalkable);
        }

    }
}
