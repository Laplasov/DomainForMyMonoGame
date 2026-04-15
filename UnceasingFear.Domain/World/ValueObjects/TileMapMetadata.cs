using System;
using System.Collections.Generic;
using System.Text;
using UnceasingFear.Domain.World.ValueObjects;

namespace UnceasingFear.Domain.World.ValueObjects
{
    public readonly record struct TileMapMetadata(
        int Width,
        int Height,
        int TileWidth,
        int TileHeight,
        float LayerScale
        )
    {
        public TileCoord WorldToTile(WorldPosition pos)
        {
            var scaledW = TileWidth * LayerScale;
            var scaledH = TileHeight * LayerScale;
            return new TileCoord((int)(pos.X / scaledW), (int)(pos.Y / scaledH));
        }

        public WorldPosition TileToWorld(TileCoord coord)
        {
            var scaledW = TileWidth * LayerScale;
            var scaledH = TileHeight * LayerScale;
            return new WorldPosition(
                coord.X * scaledW + scaledW / 2,
                coord.Y * scaledH + scaledH / 2
            );
        }
    }
}
