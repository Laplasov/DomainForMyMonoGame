using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Application.Combat.Snapshots;

namespace UnceasingFear.Presentation.Render
{
    public class BattleView
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly GraphicsDeviceManager _graphics;

        private Texture2D _whitePixel;

        // Slot positions (calculated once)
        private Rectangle[] _allySlotRects = new Rectangle[6];
        private Rectangle[] _enemySlotRects = new Rectangle[6];

        private bool init = false;

        private SpriteFont _debugFont;

        public BattleView(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            _spriteBatch = spriteBatch;
            _graphics = graphics;

            _whitePixel = new Texture2D(_graphics.GraphicsDevice, 1, 1);
            _whitePixel.SetData(new[] { Color.White });
        }
        private void InitializeSlotPositions()
        {
            init = true;

            int slotSize = 64;
            int spacing = 16;
            int startX = 50;
            int allyY = _graphics.PreferredBackBufferHeight - 100;
            int enemyY = 50;

            // Ally slots (bottom left)
            for (int i = 0; i < 6; i++)
            {
                int row = i / 3;
                int col = i % 3;
                _allySlotRects[i] = new Rectangle(
                    startX + col * (slotSize + spacing),
                    allyY - row * (slotSize + spacing),
                    slotSize, slotSize);
            }

            // Enemy slots (top right)
            for (int i = 0; i < 6; i++)
            {
                int row = i / 3;
                int col = i % 3;
                _enemySlotRects[i] = new Rectangle(
                    _graphics.PreferredBackBufferWidth - startX - slotSize - col * (slotSize + spacing),
                    enemyY + row * (slotSize + spacing),
                    slotSize, slotSize);
            }
        }

        public void Draw(BattleSnapshot snapshot)
        {
            if (!init) InitializeSlotPositions();

            _graphics.GraphicsDevice.Clear(Color.DarkSlateGray);
            _spriteBatch.Begin();

            // 1. Draw all slot backgrounds first
            for (int i = 0; i < 6; i++)
            {
                _spriteBatch.Draw(_whitePixel, _allySlotRects[i], Color.DarkGreen * 0.5f);
                _spriteBatch.Draw(_whitePixel, _enemySlotRects[i], Color.DarkRed * 0.5f);
            }

            foreach (var unit in snapshot.Units)
            {
                // Select correct slot array based on faction
                var rects = unit.IsAlly ? _allySlotRects : _enemySlotRects;
                int arrayIndex = unit.SlotIndex - 1;

                // Visual state: alive/dead + faction color
                var baseColor = unit.IsAlly ? Color.Lime : Color.OrangeRed;
                var color = unit.IsAlive ? baseColor : Color.Gray;

                if (arrayIndex >= 0 && arrayIndex < 6)
                {
                    // ✅ Use 'rects' instead of hardcoded '_allySlotRects'
                    var slotRect = rects[arrayIndex];
                    var unitRect = new Rectangle(
                        slotRect.X + 8,
                        slotRect.Y + 8,
                        slotRect.Width - 16,
                        slotRect.Height - 16);

                    _spriteBatch.Draw(_whitePixel, unitRect, color);

                    // Optional: HP bar overlay
                    if (unit.IsAlive && unit.MaxHp > 0)
                    {
                        float hpPercent = (float)unit.CurrentHp / unit.MaxHp;
                        var hpBar = new Rectangle(unitRect.X, unitRect.Y - 4,
                            (int)(unitRect.Width * hpPercent), 3);
                        _spriteBatch.Draw(_whitePixel, hpBar, Color.Red);
                    }
                }
                if (arrayIndex >= 0 && arrayIndex < 6)
                {
                    var slotRect = rects[arrayIndex];

                    // Draw slot number using bitmap digits (no font needed!)
                    DebugTextDrawer.DrawNumber(
                        _spriteBatch,
                        _whitePixel,
                        unit.SlotIndex,  // e.g., 1 or 5
                        new Vector2(slotRect.X + 20, slotRect.Y + 20),
                        Color.White,
                        scale: 2);
                }
            }


            _spriteBatch.End();
        }
    }
}
