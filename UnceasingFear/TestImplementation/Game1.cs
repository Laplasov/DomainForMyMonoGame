using GumRuntime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameGum;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnceasingFear.Application.Collision;
using UnceasingFear.Application.Combat;
using UnceasingFear.Application.Combat.Snapshots;
using UnceasingFear.Application.Commands;
using UnceasingFear.Application.Repository;
using UnceasingFear.Application.World;
using UnceasingFear.Application.World.Snapshots;
using UnceasingFear.Domain.Combat.Events;
using UnceasingFear.Domain.Shared.Events;
using UnceasingFear.Domain.World.Aggregates;
using UnceasingFear.Domain.World.Entities;
using UnceasingFear.Domain.World.Enums;
using UnceasingFear.Domain.World.ValueObjects;
using UnceasingFear.Persistence;
using UnceasingFear.Persistence.XML;
using UnceasingFear.Presentation.Data;
using UnceasingFear.Presentation.Render;
using static UnceasingFear.Domain.Shared.Events.SharedEvents;

namespace UnceasingFear.TestImplementation;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    public IEventDispatcher EventDispatcher { get; } = new DomainEventDispatcher();
    public ICommandDispatcher CommandDispatcher { get; } = new CommandDispatcher();

    private const float PlayerSpeed = 200f;

    private WorldSnapshot _worldSnapshot;
    private BattleSnapshot _battleSnapshot;

    private WorldApplicationService _appServiceWorld;
    private BattleServiceProvider _battleServiceProvider;

    private WorldView _worldView;
    private BattleView _battleView;

    private SceneId _lastSceneId;
    private GameTime _gameTime = new GameTime();

    private SpriteFactory _spriteFactory;
    private GumService Gum => GumService.Default;
    public bool IsPaused { get; private set; } = false;

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
        Gum.Initialize(this);

        var spriteRepo = new XmlSpriteRepository(Path.Combine("Content", "DB", "ViewData", "sprite_map.xml"));
        _spriteFactory = new SpriteFactory(Content, spriteRepo);

        ISceneProvider SceneProvider = new XmlSceneProvider(Path.Combine("Content", "DB"));
        Scene scene = SceneProvider.GetById(SceneId.From("TestScene"));

        Group playerGroup = scene.Groups.First(g => g.UnitBehavior == UnitBehavior.PlayerControlled);

        _lastSceneId = scene.Id;

        _battleServiceProvider = new BattleServiceProvider();
        _battleServiceProvider.Initialize(EventDispatcher, CommandDispatcher);
        _appServiceWorld = new WorldApplicationService(scene, playerGroup, EventDispatcher, CommandDispatcher, SceneProvider);

        EventDispatcher.Subscribe<OutOfBattleEvent>((e) => _worldSnapshot = _appServiceWorld.GetSnapshot());
        EventDispatcher.Subscribe<ExitGame>((e) => Exit());
        EventDispatcher.Subscribe<PauseGame>((e) => IsPaused = e.ShouldPause);

        base.Initialize();
    }
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _worldSnapshot = _appServiceWorld.GetSnapshot();

        _worldView = new WorldView(_spriteBatch, GraphicsDevice, _spriteFactory, _gameTime, Gum, EventDispatcher, CommandDispatcher);
        _battleView = new BattleView(_spriteBatch, _graphics, Gum, EventDispatcher, CommandDispatcher);
    }

    protected override void Update(GameTime gameTime)
    {
        Gum.Update(gameTime);

        _gameTime = gameTime;

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

        _worldView.HandleInput(keyboard);

        float dx = 0, dy = 0;
        if (keyboard.IsKeyDown(Keys.W)) dy -= PlayerSpeed * delta;
        if (keyboard.IsKeyDown(Keys.S)) dy += PlayerSpeed * delta;
        if (keyboard.IsKeyDown(Keys.A)) dx -= PlayerSpeed * delta;
        if (keyboard.IsKeyDown(Keys.D)) dx += PlayerSpeed * delta;

        CommandDispatcher.Dispatch(new MovePlayerCommand(dx, dy, delta));

        if (keyboard.IsKeyDown(Keys.C))
            CommandDispatcher.Dispatch(new RequestTransitionCommand());

        if (keyboard.IsKeyDown(Keys.E))
            CommandDispatcher.Dispatch(new InteractCommand());

        _worldSnapshot = _appServiceWorld.GetSnapshot();

        if (_lastSceneId != _worldSnapshot.CurrentScene)
        {
            _lastSceneId = _worldSnapshot.CurrentScene;
        }
    }

    public void UpdateBattle(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _battleView.HandleInput();

        CommandDispatcher.Dispatch(new UpdateCommand(delta));
        if(_battleServiceProvider.ActiveService != null)
            _battleSnapshot = _battleServiceProvider.ActiveService.GetSnapshot();
    }

    protected override void Draw(GameTime gameTime)
    {
        if (!_worldSnapshot.BattleTriggered) 
        {
            _worldView.Draw(_worldSnapshot);
        }
        else if (_battleSnapshot.Units != null)
        {
            _battleView.Draw(_battleSnapshot);
        }
        Gum.Draw();
        base.Draw(gameTime);
    }
}