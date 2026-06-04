using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame_Game_Library.Graphics;
using MonoGame_Game_Library.TileLogic;
using MonoGameGum;
using UnceasingFear.Application.World.Snapshots;
using UnceasingFear.Domain.World.ValueObjects;
using UnceasingFear.Presentation.Data;

public class WorldView
{
    private readonly SpriteBatch _spriteBatch;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Texture2D _whitePixel;
    private readonly SpriteFactory _spriteFactory;
    private readonly GameTime _gameTime;

    private readonly Dictionary<string, AnimatedSprite> _groupSprites = new();
    private TileMapLayered? _currentTilemap;
    private string _currentSceneId = string.Empty;
    private GumService _gumService;

    public WorldView(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice,
                     SpriteFactory spriteFactory, GameTime gameTime, GumService gumService)
    {
        _spriteBatch = spriteBatch;
        _graphicsDevice = graphicsDevice;
        _spriteFactory = spriteFactory;
        _gameTime = gameTime;
        _gumService = gumService;

        _whitePixel = new Texture2D(graphicsDevice, 1, 1);
        _whitePixel.SetData(new[] { Color.White });

    }

    public void Draw(WorldSnapshot snapshot)
    {
        _graphicsDevice.Clear(Color.CornflowerBlue);

        var sceneId = snapshot.CurrentScene.Value;
        if (sceneId != _currentSceneId)
        {
            _currentTilemap = _spriteFactory.CreateTileMap(sceneId);
            _currentSceneId = sceneId;
        }

        _spriteBatch.Begin(blendState: BlendState.NonPremultiplied);

        var testTexture = _currentTilemap?.Layers["Ground"].Tileset;
        if (testTexture != null)
        {
            // Draw tile 60: index 59, col=9, row=5
            var sourceRect = new Rectangle(576, 320, 64, 64);
            _spriteBatch.Draw(testTexture, new Vector2(100, 100), sourceRect, Color.White);
        }

        // 1. Ground layer
        _currentTilemap?.DrawLayer(_spriteBatch, "Ground", Vector2.Zero);

        DrawCollisionOverlay(snapshot);

        // 2. Debug tile grid on top of ground, under entities
        DrawTileGrid(snapshot);

        // 3. Transition tiles
        foreach (var tile in snapshot.TransitionTiles)
        {
            var center = snapshot.TileMapMetadata.TileToWorld(tile);
            var rect = new Rectangle(
                (int)center.X - snapshot.TileMapMetadata.TileWidth / 2,
                (int)center.Y - snapshot.TileMapMetadata.TileHeight / 2,
                snapshot.TileMapMetadata.TileWidth,
                snapshot.TileMapMetadata.TileHeight);
            _spriteBatch.Draw(_whitePixel, rect, Color.Yellow * 0.5f);
        }

        // 4. Groups
        foreach (var group in snapshot.Groups)
        {
            if (group.IsDefeated) continue;

            var sprite = GetOrCreateSprite(group);
            sprite.Update(_gameTime);

            var position = new Vector2(
                group.CurrentPosition.X - sprite.Width / 2f,
                group.CurrentPosition.Y - sprite.Height / 2f);

            sprite.Draw(_spriteBatch, position);

            if (group.IsAggroed)
            {
                var indicator = new Rectangle(
                    (int)group.CurrentPosition.X - 30,
                    (int)group.CurrentPosition.Y - 30,
                    60, 60);
                _spriteBatch.Draw(_whitePixel, indicator, Color.Orange * 0.3f);
            }
        }
        _spriteBatch.End();
    }

    private void DrawTileGrid(WorldSnapshot snapshot)
    {
        var metadata = snapshot.TileMapMetadata;

        for (int y = 0; y < metadata.Height; y++)
        {
            for (int x = 0; x < metadata.Width; x++)
            {
                var tileCenter = metadata.TileToWorld(new TileCoord(x, y));
                var screenPos = new Vector2(tileCenter.X - 8, tileCenter.Y - 6);

                DebugTextDrawer.DrawTileCoord(
                    _spriteBatch, _whitePixel,
                    new TileCoord(x, y),
                    screenPos + new Vector2(1, 1),
                    Color.Black * 0.7f, scale: 1);

                DebugTextDrawer.DrawTileCoord(
                    _spriteBatch, _whitePixel,
                    new TileCoord(x, y),
                    screenPos,
                    Color.White * 0.9f, scale: 1);
            }
        }
    }
    private AnimatedSprite GetOrCreateSprite(GroupSnapshot group)
    {
        if (!_groupSprites.TryGetValue(group.Id.Value, out var sprite))
        {
            sprite = _spriteFactory.CreateGroupSprite(group.Id.Value);
            _groupSprites[group.Id.Value] = sprite;
        }
        return sprite;
    }

    // Add this method inside your WorldView class
    private void DrawCollisionOverlay(WorldSnapshot snapshot)
    {
        // Only draw if we have a tilemap with a Collisions layer
        if (_currentTilemap?.Layers.TryGetValue("Collisions", out var collisionLayer) != true)
            return;

        var metadata = snapshot.TileMapMetadata;
        var tileW = metadata.TileWidth;
        var tileH = metadata.TileHeight;

        // Iterate through the collision layer data
        for (int y = 0; y < collisionLayer!.TileData.GetLength(0); y++)
        {
            for (int x = 0; x < collisionLayer.TileData.GetLength(1); x++)
            {
                // Your Tiled setup: tile ID 101 = collision (from marks.tsx)
                if (collisionLayer.TileData[y, x] == 101)
                {
                    var rect = new Rectangle(
                        x * tileW,
                        y * tileH,
                        tileW,
                        tileH);

                    // Draw semi-transparent red overlay
                    _spriteBatch.Draw(_whitePixel, rect, Color.Red * 0.3f);
                }
            }
        }
    }
}