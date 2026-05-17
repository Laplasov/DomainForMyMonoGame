using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace MonoGame_Game_Library.TileLogic;

public class TileMapLayered
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int TileWidth { get; private set; }
    public int TileHeight { get; private set; }

    private const float PIXEL_TO_WORLD_SCALE = 0.01f;
    public Dictionary<string, TileLayer> Layers { get; private set; }

    public TileMapLayered()
    {
        Layers = new Dictionary<string, TileLayer>();
    }

    public static TileMapLayered LoadFromXml(string xmlPath)
    {
        var tileMap = new TileMapLayered();
        XDocument doc = XDocument.Load(xmlPath);
        XElement mapElement = doc.Element("map");

        // Parse map properties
        tileMap.Width = int.Parse(mapElement.Attribute("width").Value);
        tileMap.Height = int.Parse(mapElement.Attribute("height").Value);
        tileMap.TileWidth = int.Parse(mapElement.Attribute("tilewidth").Value);
        tileMap.TileHeight = int.Parse(mapElement.Attribute("tileheight").Value);

        // Parse all layers
        var layerElements = mapElement.Elements("layer");
        foreach (var layerElement in layerElements)
        {
            string layerName = layerElement.Attribute("name").Value;
            var layer = new TileLayer
            {
                Name = layerName,
                TileData = new int[tileMap.Height, tileMap.Width],
                TileWidth = tileMap.TileWidth,
                TileHeight = tileMap.TileHeight
            };

            // Parse visibility attribute
            XAttribute visibleAttribute = layerElement.Attribute("visible");
            if (visibleAttribute != null && visibleAttribute.Value == "0")
            {
                layer.Visible = false;
            }

            // Parse layer data
            XElement dataElement = layerElement.Element("data");
            string csvData = dataElement.Value.Trim();

            // Parse CSV data
            string[] rows = csvData.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            for (int y = 0; y < rows.Length && y < tileMap.Height; y++)
            {
                string[] tiles = rows[y].Split(',');
                for (int x = 0; x < tiles.Length && x < tileMap.Width; x++)
                {
                    if (int.TryParse(tiles[x].Trim(), out int tileId))
                    {
                        layer.TileData[y, x] = tileId;
                    }
                }
            }

            tileMap.Layers[layerName] = layer;
        }

        return tileMap;
    }

    // Set tileset for a specific layer
    public void SetLayerTileset(string layerName, Texture2D tilesetTexture, int tileWidth, int tileHeight)
    {
        if (!Layers.ContainsKey(layerName))
        {
            throw new ArgumentException($"Layer '{layerName}' not found in tilemap.");
        }

        var layer = Layers[layerName];
        layer.Tileset = tilesetTexture;
        layer.TileWidth = tileWidth;
        layer.TileHeight = tileHeight;
        layer.TilesetColumns = tilesetTexture.Width / tileWidth;
    }

    // Set the same tileset for all layers (convenience method)
    public void SetTilesetForAllLayers(Texture2D tilesetTexture, int tileWidth, int tileHeight)
    {
        foreach (var layer in Layers.Values)
        {
            layer.Tileset = tilesetTexture;
            layer.TileWidth = tileWidth;
            layer.TileHeight = tileHeight;
            layer.TilesetColumns = tilesetTexture.Width / tileWidth;
        }
    }

    // Set tilesets for multiple layers at once
    public void SetLayerTilesets(Dictionary<string, Texture2D> layerTilesets, int tileWidth, int tileHeight)
    {
        foreach (var kvp in layerTilesets)
        {
            if (Layers.ContainsKey(kvp.Key))
            {
                SetLayerTileset(kvp.Key, kvp.Value, tileWidth, tileHeight);
            }
        }
    }

    // Draw all visible layers
    public void Draw(SpriteBatch spriteBatch, Vector2 position, float scale = 1f)
    {
        foreach (var layer in Layers.Values.Where(l => l.Visible))
        {
            DrawLayer(spriteBatch, layer.Name, position, scale);
        }
    }

    // Draw a specific layer by name
    public void DrawLayer(SpriteBatch spriteBatch, string layerName, Vector2 position, float scale = 1f)
    {
        if (!Layers.ContainsKey(layerName))
        {
            throw new ArgumentException($"Layer '{layerName}' not found in tilemap.");
        }

        var layer = Layers[layerName];

        if (!layer.Visible)
            return;

        if (layer.Tileset == null)
        {
            // Skip layers without tilesets (like collision/event layers)
            return;
        }

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                int tileId = layer.TileData[y, x];

                // Skip empty tiles (tileId 0 means no tile)
                if (tileId == 0)
                    continue;

                // Tiled uses 1-based indexing, so subtract 1
                int tileIndex = tileId - 1;

                // Calculate source rectangle in tileset
                int tileX = (tileIndex % layer.TilesetColumns) * layer.TileWidth;
                int tileY = (tileIndex / layer.TilesetColumns) * layer.TileHeight;


                Rectangle sourceRect = new Rectangle(tileX, tileY, layer.TileWidth, layer.TileHeight);

                // Calculate destination position
                Vector2 destPosition = new Vector2(
                    position.X + (x * TileWidth * scale),
                    position.Y + (y * TileHeight * scale)
                );

                // Draw the tile
                spriteBatch.Draw(
                    layer.Tileset,
                    destPosition,
                    sourceRect,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }
    }

    // Draw multiple specific layers in order
    public void DrawLayers(SpriteBatch spriteBatch, Vector2 position, float scale = 1f, params string[] layerNames)
    {
        foreach (var layerName in layerNames)
        {
            if (Layers.ContainsKey(layerName))
            {
                DrawLayer(spriteBatch, layerName, position, scale);
            }
        }
    }

    // Get tile ID at specific position in a layer
    public int GetTileAt(string layerName, int x, int y)
    {
        if (!Layers.ContainsKey(layerName))
            return 0;

        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return 0;

        return Layers[layerName].TileData[y, x];
    }

    // Check if a layer exists
    public bool HasLayer(string layerName) => Layers.ContainsKey(layerName);

    // Set layer visibility
    public void SetLayerVisible(string layerName, bool visible)
    {
        if (Layers.ContainsKey(layerName))
        {
            Layers[layerName].Visible = visible;
        }
    }

    /// <summary>
    /// Gets the center position of the tilemap in 3D space.
    /// </summary>
    /// <param name="yPosition">The Y (height) coordinate for the center.</param>
    /// <returns>The center of the tilemap as a Vector3.</returns>
    public Vector3 GetCenter(float yPosition = 0f)
    {
        // Calculate center in tile units, then convert to world units
        float centerX = (Width * TileWidth) / 2f;
        float centerZ = (Height * TileHeight) / 2f;

        return new Vector3(centerX, yPosition, centerZ);
    }
    public Vector3 GetPixelToWorldCenterScaled(float yPosition = 0f) => GetCenter(yPosition) * PIXEL_TO_WORLD_SCALE;

    /// <summary>
    /// Gets the center position in tile coordinates (grid units).
    /// </summary>
    /// <returns>The center tile coordinate as Vector2 (x, y).</returns>
    public Vector2 GetTileCenter()
    {
        return new Vector2(Width / 2f, Height / 2f);
    }

    /// <summary>
    /// Gets the dimensions of the entire tilemap in world units.
    /// </summary>
    /// <returns>The size as Vector3 (width, 0, height).</returns>
    public Vector3 GetWorldSize()
    {
        return new Vector3(Width * TileWidth, 0, Height * TileHeight);
    }

    /// <summary>
    /// Converts tile coordinates to world position.
    /// </summary>
    public Vector3 TileToWorld(int tileX, int tileY, float yPosition = 0f)
    {
        return new Vector3(
            tileX * TileWidth + (TileWidth / 2f),
            yPosition,
            tileY * TileHeight + (TileHeight / 2f)
        );
    }

    /// <summary>
    /// Converts world position to tile coordinates.
    /// </summary>
    public (int x, int y) WorldToTile(Vector3 worldPosition)
    {
        return (
            (int)(worldPosition.X / TileWidth),
            (int)(worldPosition.Z / TileHeight)
        );
    }
}