using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.World.ValueObjects;

namespace UnceasingFear.Persistence.XML
{
    /*
    public class XmlTiledMapRepository
    {
        // Cache to avoid reloading and parsing the same large XML files multiple times
        private readonly Dictionary<string, TileMapLayered> _mapCache = new();

        public TileMapLayered GetMap(string tmxFilePath)
        {
            // Resolve to absolute path for consistent caching keys
            var fullPath = Path.GetFullPath(tmxFilePath);

            if (!_mapCache.TryGetValue(fullPath, out var map))
            {
                // Use the library's static loader
                map = TileMapLayered.LoadFromXml(fullPath);
                _mapCache[fullPath] = map;
            }

            return map;
        }

        public bool[,] GetCollisionGrid(string tmxFilePath, string layerName = "Collisions")
        {
            var map = GetMap(tmxFilePath);

            if (map.Layers.TryGetValue(layerName, out var layer))
            {
                // TileData is int[y, x]. We convert non-zero tiles to 'true' (collidable).
                // You can change this logic if specific IDs (like 101) are required.
                bool[,] grid = new bool[layer.TileData.GetLength(0), layer.TileData.GetLength(1)];

                for (int y = 0; y < layer.TileData.GetLength(0); y++)
                {
                    for (int x = 0; x < layer.TileData.GetLength(1); x++)
                    {
                        // Assuming any non-zero tile ID in the "Collisions" layer is solid
                        grid[y, x] = layer.TileData[y, x] != 0;
                    }
                }
                return grid;
            }

            // Return empty grid if layer not found
            return new bool[0, 0];
        }

        public TileMapMetadata GetMetadata(string tmxFilePath)
        {
            var map = GetMap(tmxFilePath);

            // LayerScale is typically a runtime/rendering concern, defaulting to 1.0f 
            // unless you store it in Tiled Custom Properties.
            return new TileMapMetadata(
                Width: map.Width,
                Height: map.Height,
                TileWidth: map.TileWidth,
                TileHeight: map.TileHeight,
                LayerScale: 1.0f
            );
        }
    }
}
    */
}
