using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using UnceasingFear.Application.Combat;
using UnceasingFear.Application.Combat.Snapshots;
using UnceasingFear.Application.Commands;
using UnceasingFear.Application.World.Snapshots;
using UnceasingFear.Domain.Shared.Events;
using UnceasingFear.Domain.Shared.ValueObjects;

namespace UnceasingFear.TestImplementation
{
    public class BattleInstance
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly SpriteBatch _spriteBatch;
        private readonly Game _game1;
        private readonly Texture2D _whitePixel;

        private readonly BattleApplicationService _battleService;

        private readonly IEventDispatcher _dispatcher;
        private readonly ICommandDispatcher _commandDispatcher;

        // Slot positions (calculated once)
        private Rectangle[] _allySlotRects = new Rectangle[6];
        private Rectangle[] _enemySlotRects = new Rectangle[6];

        private BattleSnapshot _battleSnapshot;

        public BattleInstance(GraphicsDeviceManager graphics, 
            SpriteBatch spriteBatch, 
            Game game1, 
            IEventDispatcher dispatcher,
            ICommandDispatcher commandDispatcher
            )
        {
            _graphics = graphics;
            _spriteBatch = spriteBatch;
            _game1 = game1;
            _dispatcher = dispatcher;
            _commandDispatcher = commandDispatcher;

            _whitePixel = new Texture2D(_game1.GraphicsDevice, 1, 1);
            _whitePixel.SetData(new[] { Color.White });

            _battleService = new BattleApplicationService(dispatcher, commandDispatcher);
            _battleSnapshot = _battleService.GetSnapshot();

            // Calculate slot positions once
            InitializeSlotPositions();
        }

        private void InitializeSlotPositions()
        {
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

        public void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
                _game1.Exit();

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _battleService.Update(delta);

            _battleSnapshot = _battleService.GetSnapshot();
        }

        public void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(Color.DarkSlateGray);
            _spriteBatch.Begin();

            // 1. Draw all slot backgrounds first
            for (int i = 0; i < 6; i++)
            {
                _spriteBatch.Draw(_whitePixel, _allySlotRects[i], Color.DarkGreen * 0.5f);
                _spriteBatch.Draw(_whitePixel, _enemySlotRects[i], Color.DarkRed * 0.5f);
            }

            foreach (var unit in _battleSnapshot.Units)
            {
                // Select correct slot array based on faction
                var rects = unit.IsAlly ? _allySlotRects : _enemySlotRects;
                int arrayIndex = unit.SlotIndex;

                // Visual state: alive/dead + faction color
                var baseColor = unit.IsAlly ? Color.Lime : Color.OrangeRed;
                var color = unit.IsAlive ? baseColor : Color.Gray;

                if (arrayIndex >= 0 && arrayIndex < 6)
                {
                    // ✅ Use 'rects' instead of hardcoded '_allySlotRects'
                    var slotRect = rects[arrayIndex - 1];
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
            }

            _spriteBatch.End();
        }
    }
}
