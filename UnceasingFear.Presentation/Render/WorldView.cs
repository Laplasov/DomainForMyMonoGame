// File: UnceasingFear.Presentation/Render/WorldView.cs

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UnceasingFear.Application.World.Snapshots;
using UnceasingFear.Domain.World.ValueObjects;

public class WorldView
{
    private readonly SpriteBatch _spriteBatch;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Texture2D _whitePixel;

    public WorldView(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        _spriteBatch = spriteBatch;
        _graphicsDevice = graphicsDevice;
        _whitePixel = new Texture2D(graphicsDevice, 1, 1);
        _whitePixel.SetData(new[] { Color.White });
    }

    public void Draw(WorldSnapshot snapshot)
    {
        _graphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();

        // ✅ 1. Draw tile grid numbers (for debugging)
        DrawTileGrid(snapshot);

        // 2. Draw transition tiles
        foreach (var tile in snapshot.TransitionTiles)
        {
            var center = snapshot.TileMapMetadata.TileToWorld(tile);
            var rect = new Rectangle(
                (int)center.X - snapshot.TileMapMetadata.TileWidth / 2,
                (int)center.Y - snapshot.TileMapMetadata.TileHeight / 2,
                snapshot.TileMapMetadata.TileWidth, snapshot.TileMapMetadata.TileHeight);
            _spriteBatch.Draw(_whitePixel, rect, Color.Yellow * 0.5f);
        }

        // 3. Draw groups
        foreach (var group in snapshot.Groups)
        {
            var rect = new Rectangle(
                (int)group.CurrentPosition.X - 25,
                (int)group.CurrentPosition.Y - 25, 50, 50);
            _spriteBatch.Draw(_whitePixel, rect, Color.Red);

            if (group.IsAggroed)
            {
                var indicator = new Rectangle(
                    (int)group.CurrentPosition.X - 30,
                    (int)group.CurrentPosition.Y - 30, 60, 60);
                _spriteBatch.Draw(_whitePixel, indicator, Color.Orange * 0.3f);
            }
        }

        _spriteBatch.End();
    }

    // ✅ NEW: Draw tile coordinate numbers on the grid
    private void DrawTileGrid(WorldSnapshot snapshot)
    {
        var metadata = snapshot.TileMapMetadata;
        int scaledW = (int)(metadata.TileWidth * metadata.LayerScale);
        int scaledH = (int)(metadata.TileHeight * metadata.LayerScale);

        // ✅ Show full grid (or adjust as needed)
        int maxDisplayX = metadata.Width;   // Was: Math.Min(10, metadata.Width)
        int maxDisplayY = metadata.Height;  // Was: Math.Min(10, metadata.Height)

        // Optional: Reduce scale so numbers fit better on 20×20 grid
        int textScale = 1; // Keep small for dense grids

        for (int y = 0; y < maxDisplayY; y++)
        {
            for (int x = 0; x < maxDisplayX; x++)
            {
                var tileCenter = metadata.TileToWorld(new TileCoord(x, y));
                var screenPos = new Vector2(
                    tileCenter.X - 8,  // Slightly adjusted offset
                    tileCenter.Y - 6);

                // Draw with shadow for readability
                DebugTextDrawer.DrawTileCoord(
                    _spriteBatch, _whitePixel,
                    new TileCoord(x, y),
                    screenPos + new Vector2(1, 1),
                    Color.Black * 0.7f,  // Shadow
                    scale: textScale);

                DebugTextDrawer.DrawTileCoord(
                    _spriteBatch, _whitePixel,
                    new TileCoord(x, y),
                    screenPos,
                    Color.White * 0.9f,  // Slightly dimmed main text
                    scale: textScale);
            }
        }
    }
}