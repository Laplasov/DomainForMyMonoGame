using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame_Game_Library.Input;
using MonoGameGum;
using System.Diagnostics;
using UnceasingFear.Application.Combat.Snapshots;
using UnceasingFear.Application.Commands;
using UnceasingFear.Application.World;
using UnceasingFear.Domain.Combat.Events;
using UnceasingFear.Domain.Combat.ValueObjects;
using UnceasingFear.Domain.Shared.Events;
using UnceasingFear.Presentation.Data;
using UnceasingFear.Presentation.Input;
using UnceasingFear.Presentation.Render.Battle;

namespace UnceasingFear.Presentation.Render
{
    /// <summary>
    /// Thin orchestrator. Owns the lifetime of the battle UI and
    /// delegates all work to focused collaborators.
    ///
    /// Responsibilities:
    ///   - Create collaborators with their dependencies
    ///   - Initialise on first Draw (lazy, so GraphicsDevice is ready)
    ///   - Route Draw() calls to the right renderer
    ///   - Tear down Gum widgets on BattleExitEvent
    /// </summary>
    public class BattleView
    {
        // ── Infrastructure ───────────────────────────────────────────────────
        private readonly SpriteBatch _spriteBatch;
        private readonly GraphicsDeviceManager _graphics;
        private readonly GumService _gumService;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ICommandDispatcher _commandDispatcher;

        // ── Collaborators (null until first Draw) ────────────────────────────
        private BattleLayout? _layout;
        private BattleHudHandles? _hudHandles;
        private BattleHudUpdater? _hudUpdater;
        private BattleUnitRenderer? _unitRenderer;

        private bool _initialised = false;

        private readonly SlotInputHandler _slotInput;
        private readonly MouseInfo _mouse = new();

        public BattleView(
            SpriteBatch spriteBatch,
            GraphicsDeviceManager graphics,
            GumService gumService,
            IEventDispatcher eventDispatcher,
            ICommandDispatcher commandDispatcher)
        {
            _spriteBatch = spriteBatch;
            _graphics = graphics;
            _gumService = gumService;
            _eventDispatcher = eventDispatcher;
            _commandDispatcher = commandDispatcher;

            _slotInput = new(commandDispatcher);

            _eventDispatcher.Subscribe<CombatEvents.BattleExitEvent>(OnBattleExit);
        }

        // ── Public ───────────────────────────────────────────────────────────

        public void Draw(BattleSnapshot snapshot)
        {
            if (!_initialised) Initialise();

            _graphics.GraphicsDevice.Clear(Color.DarkSlateGray);

            _spriteBatch.Begin();
            _unitRenderer!.Draw(snapshot);
            _spriteBatch.End();

            _hudUpdater!.Update(snapshot);
        }

        public void HandleInput()
        {
            if (!_initialised || _layout is null) return;

            _mouse.Update();
            _slotInput.Update(_mouse, _layout);
        }

        // ── Private ──────────────────────────────────────────────────────────

        private void Initialise()
        {
            _layout = new BattleLayout(
                _graphics.PreferredBackBufferWidth,
                _graphics.PreferredBackBufferHeight);

            var pixel = new Texture2D(_graphics.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            var builder = new BattleHudBuilder(_gumService, _layout, _commandDispatcher, _slotInput);
            _hudHandles = builder.Build();
            _hudUpdater = new BattleHudUpdater(_hudHandles, _eventDispatcher);
            _unitRenderer = new BattleUnitRenderer(_spriteBatch, pixel, _layout, _slotInput);

            _slotInput.Register(_layout.AllySlotRects);  
            _slotInput.Register(_layout.EnemySlotRects);

            _initialised = true;
        }

        private void OnBattleExit(CombatEvents.BattleExitEvent e)
        {
            _initialised = false;
            _gumService.Root.Children.Clear();
            _hudHandles = null;
            _hudUpdater = null;
            _unitRenderer = null;
        }
    }

}
