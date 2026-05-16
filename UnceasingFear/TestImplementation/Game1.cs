using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnceasingFear.Application.Combat;
using UnceasingFear.Application.Combat.Snapshots;
using UnceasingFear.Application.Commands;
using UnceasingFear.Application.Repository;
using UnceasingFear.Application.World;
using UnceasingFear.Application.World.Snapshots;
using UnceasingFear.Domain.Shared.Events;
using UnceasingFear.Domain.World.Aggregates;
using UnceasingFear.Domain.World.Entities;
using UnceasingFear.Domain.World.Enums;
using UnceasingFear.Domain.World.ValueObjects;
using UnceasingFear.Persistence;
using UnceasingFear.Persistence.XML;
using UnceasingFear.Presentation.Render;
using static UnceasingFear.Domain.Shared.Events.SharedEvents;

namespace UnceasingFear.TestImplementation;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    public IEventDispatcher EventDispatcher { get; } = new DomainEventDispatcher();
    public ICommandDispatcher CommandDispatcher { get; } = new CommandDispatcher();

    private Vector2 _playerPosition;
    private const float PlayerSpeed = 200f;

    private WorldSnapshot _worldSnapshot;
    private BattleSnapshot _battleSnapshot;

    private WorldApplicationService _appServiceWorld;
    private BattleServiceProvider _battleServiceProvider;

    private WorldView _worldView;
    private BattleView _battleView;

    private SceneId _lastSceneId;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 800;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }
    
    protected override void Initialize()
    {
        ISceneProvider SceneProvider = new XmlSceneProvider(Path.Combine("Content", "DB"));

        Scene scene = SceneProvider.GetById(SceneId.From("TestScene"));
        Group playerGroup = scene.Groups.First(g => g.MovementPattern == MovementPattern.PlayerControlled);

        _playerPosition = new Vector2(playerGroup.CurrentPosition.X, playerGroup.CurrentPosition.Y);
        _lastSceneId = scene.Id;

        _battleServiceProvider = new BattleServiceProvider();
        _battleServiceProvider.Initialize(EventDispatcher, CommandDispatcher);

        _appServiceWorld = new WorldApplicationService(scene, playerGroup, EventDispatcher, CommandDispatcher, SceneProvider);

        base.Initialize();
    }
    
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _worldView = new WorldView(_spriteBatch, GraphicsDevice);
        _battleView = new BattleView(_spriteBatch, _graphics);

        _worldSnapshot = _appServiceWorld.GetSnapshot();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (!_worldSnapshot.BattleTriggered)
            UpdateWorld(gameTime);
        else
            UpdateBattle(gameTime);

        base.Update(gameTime);
    }

    protected void UpdateWorld(GameTime gameTime)
    {
        // Player movement
        var keyboard = Keyboard.GetState();
        var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (keyboard.IsKeyDown(Keys.W)) _playerPosition.Y -= PlayerSpeed * delta;
        if (keyboard.IsKeyDown(Keys.S)) _playerPosition.Y += PlayerSpeed * delta;
        if (keyboard.IsKeyDown(Keys.A)) _playerPosition.X -= PlayerSpeed * delta;
        if (keyboard.IsKeyDown(Keys.D)) _playerPosition.X += PlayerSpeed * delta;

        CommandDispatcher.Dispatch(new MovePlayerCommand(_playerPosition.X, _playerPosition.Y, delta));

        if (keyboard.IsKeyDown(Keys.C))
            CommandDispatcher.Dispatch(new RequestTransitionCommand());

        _worldSnapshot = _appServiceWorld.GetSnapshot();

        if (_lastSceneId != _worldSnapshot.CurrentScene)
        {
            _playerPosition = new Vector2(_worldSnapshot.PlayerPosition.X, _worldSnapshot.PlayerPosition.Y);
            _lastSceneId = _worldSnapshot.CurrentScene;
        }

    }

    public void UpdateBattle(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _battleServiceProvider.ActiveService.Update(delta);

        _battleSnapshot = _battleServiceProvider.ActiveService.GetSnapshot();

        if (keyboard.IsKeyDown(Keys.C))
        {
            CommandDispatcher.Dispatch(new EndBattleCommand());
            _worldSnapshot = _appServiceWorld.GetSnapshot();
        }

    }

    protected override void Draw(GameTime gameTime)
    {
        if (!_worldSnapshot.BattleTriggered)
            _worldView.Draw(_worldSnapshot);
        else if (_battleSnapshot.Units != null)
            _battleView.Draw(_battleSnapshot);

        base.Draw(gameTime);
    }
}