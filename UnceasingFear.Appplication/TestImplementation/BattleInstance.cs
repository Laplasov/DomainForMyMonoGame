using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Application.Combat;
using UnceasingFear.Domain.Combat.Aggregates;
using UnceasingFear.Domain.Combat.Entities;
using UnceasingFear.Domain.Combat.Services;
using UnceasingFear.Domain.Combat.ValueObjects;
using UnceasingFear.Domain.Shared.Events;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Domain.Shared.ValueObjects.Abilities;
using UnceasingFear.Domain.World.Events;
using static UnceasingFear.Domain.Combat.Events.CombatEvents;
using static UnceasingFear.Domain.Combat.ValueObjects.BattleState;

namespace UnceasingFear.TestImplementation
{
    public class BattleInstance
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly SpriteBatch _spriteBatch;
        private readonly IEventDispatcher _dispatcher;
        private readonly Game _game1;
        private readonly Texture2D _whitePixel;

        private readonly BattleApplicationService _battleService;

        // Domain
        private readonly Battle _battle;
        private readonly ITurnOrderService _turnOrder;
        private readonly ITargetResolver _targetResolver;
        private Unit? _currentActor;


        // Battle data
        private IReadOnlyList<UnitProfile> _allyProfiles;
        private IReadOnlyList<UnitProfile> _enemyProfiles;

        // Slot positions (calculated once)
        private Rectangle[] _allySlotRects = new Rectangle[6];
        private Rectangle[] _enemySlotRects = new Rectangle[6];

        public BattleInstance(GraphicsDeviceManager graphics, 
            SpriteBatch spriteBatch, 
            Game game1, 
            IEventDispatcher dispatcher,
            IReadOnlyList<UnitProfile> allyProfiles, 
            IReadOnlyList<UnitProfile> enemyProfiles)
        {
            _graphics = graphics;
            _spriteBatch = spriteBatch;
            _game1 = game1;
            _allyProfiles = allyProfiles;
            _enemyProfiles = enemyProfiles;
            _dispatcher = dispatcher;

            _whitePixel = new Texture2D(_game1.GraphicsDevice, 1, 1);
            _whitePixel.SetData(new[] { Color.White });

            _turnOrder = new TurnOrderService();
            _targetResolver = new TargetResolver();
            _battle = new Battle();

            _battleService = new BattleApplicationService(
            allyProfiles,
            enemyProfiles,
            new TurnOrderService(),
            new TargetResolver(),
            dispatcher);

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
                return;

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _battleService.Update(delta);
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

            // 2. Draw units in their CORRECT slots based on SlotIndex property
            if (_allyProfiles != null)
            {
                foreach (var profile in _allyProfiles)
                {
                    // Convert 1-based SlotIndex to 0-based array index
                    int arrayIndex = profile.SlotIndex - 1;

                    if (arrayIndex >= 0 && arrayIndex < 6)
                    {
                        var unitRect = new Rectangle(
                            _allySlotRects[arrayIndex].X + 8,
                            _allySlotRects[arrayIndex].Y + 8,
                            _allySlotRects[arrayIndex].Width - 16,
                            _allySlotRects[arrayIndex].Height - 16);
                        _spriteBatch.Draw(_whitePixel, unitRect, Color.Lime);
                    }
                }
            }

            if (_enemyProfiles != null)
            {
                foreach (var profile in _enemyProfiles)
                {
                    int arrayIndex = profile.SlotIndex - 1;

                    if (arrayIndex >= 0 && arrayIndex < 6)
                    {
                        var unitRect = new Rectangle(
                            _enemySlotRects[arrayIndex].X + 8,
                            _enemySlotRects[arrayIndex].Y + 8,
                            _enemySlotRects[arrayIndex].Width - 16,
                            _enemySlotRects[arrayIndex].Height - 16);
                        _spriteBatch.Draw(_whitePixel, unitRect, Color.OrangeRed);
                    }
                }
            }

            _spriteBatch.End();
        }
    }
}
