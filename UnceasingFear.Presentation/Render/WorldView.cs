using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UnceasingFear.Application.World.Snapshots;
using UnceasingFear.Domain.World.ValueObjects;

namespace UnceasingFear.Presentation.Render
{
    public class WorldView
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly Texture2D _whitePixel;
        private readonly TileMapMetadata _metadata;
        private readonly GraphicsDevice _graphicsDevice;

        public WorldView(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Texture2D whitePixel, TileMapMetadata metadata)
        {
            _spriteBatch = spriteBatch;
            _graphicsDevice = graphicsDevice;
            _whitePixel = whitePixel;
            _metadata = metadata;
        }

        public void Draw(WorldSnapshot snapshot)
        {
            _graphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            foreach (var tile in snapshot.TransitionTiles)
            {
                var center = _metadata.TileToWorld(tile);
                var rect = new Rectangle(
                    (int)center.X - _metadata.TileWidth / 2,
                    (int)center.Y - _metadata.TileHeight / 2,
                    _metadata.TileWidth, _metadata.TileHeight);
                _spriteBatch.Draw(_whitePixel, rect, Color.Yellow * 0.5f);
            }

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
    }
}
