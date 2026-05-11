using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using UnceasingFear.Application.Combat;
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
using UnceasingFear.Presentation.Render;

namespace UnceasingFear.TestImplementation;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    public IEventDispatcher EventDispatcher { get; } = new DomainEventDispatcher();
    public ICommandDispatcher CommandDispatcher { get; } = new CommandDispatcher();

    // Domain objects
    private Vector2 _playerPosition;
    private const float PlayerSpeed = 200f;

    // Visual helpers
    private Texture2D _whitePixel;
    private Rectangle _playerRect;
    private List<Rectangle> _groupRects = new();

    TileMapMetadata _metadata;

    BattleInstance _battle;

    private WorldSnapshot _worldSnapshot;

    private WorldApplicationService _appServiceWorld;
    private BattleServiceProvider _battleServiceProvider;

    private WorldView _worldView;

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
        ISceneProvider SceneProvider = new SceneProviderTest();

        Scene scene = SceneProvider.GetById(SceneId.From("TestScene"));
        Group playerGroup = scene.Groups.First(g => g.MovementPattern == MovementPattern.PlayerControlled);

        _metadata = scene.MapMetadata;
        _playerPosition = new Vector2(playerGroup.SpawnPosition.X, playerGroup.SpawnPosition.Y);

        _battleServiceProvider = new BattleServiceProvider();
        _battleServiceProvider.Initialize(EventDispatcher, CommandDispatcher);

        _appServiceWorld = new WorldApplicationService(scene, playerGroup, EventDispatcher, CommandDispatcher);

        base.Initialize();
    }
    
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Create 1x1 white texture for drawing rectangles
        _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
        _whitePixel.SetData(new[] { Color.White });

        _worldView = new WorldView(_spriteBatch, GraphicsDevice, _whitePixel, _metadata);

        _worldSnapshot = _appServiceWorld.GetSnapshot();
        UpdateRectangles();
    }

    protected override void Update(GameTime gameTime)
    {
        if(!_worldSnapshot.BattleTriggered)
            UpdateWorld(gameTime);
        else
            _battle.Update(gameTime);

        base.Update(gameTime);
    }

    protected void UpdateWorld(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // Player movement
        var keyboard = Keyboard.GetState();
        var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (keyboard.IsKeyDown(Keys.W)) _playerPosition.Y -= PlayerSpeed * delta;
        if (keyboard.IsKeyDown(Keys.S)) _playerPosition.Y += PlayerSpeed * delta;
        if (keyboard.IsKeyDown(Keys.A)) _playerPosition.X -= PlayerSpeed * delta;
        if (keyboard.IsKeyDown(Keys.D)) _playerPosition.X += PlayerSpeed * delta;

        CommandDispatcher.Dispatch(new MovePlayerCommand(_playerPosition.X, _playerPosition.Y, delta));

        if (keyboard.IsKeyDown(Keys.C))
        {
            CommandDispatcher.Dispatch(new RequestTransitionCommand());
        }

        _worldSnapshot = _appServiceWorld.GetSnapshot();

        if (_worldSnapshot.BattleTriggered)
            _battle = new BattleInstance(_graphics, _spriteBatch, this, _battleServiceProvider);

        UpdateRectangles();
    }
    protected override void Draw(GameTime gameTime)
    {

        if (!_worldSnapshot.BattleTriggered)
            _worldView.Draw(_worldSnapshot);  //DrawWorld(gameTime);
        else
            _battle.Draw(gameTime);

        base.Draw(gameTime);
    }
    protected void DrawWorld(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        // Draw transitions (yellow tiles)
        foreach (var transition in _worldSnapshot.TransitionTiles)
        {
            var tileCenter = _metadata.TileToWorld(transition);
            var transitionRect = new Rectangle(
                (int)tileCenter.X - _metadata.TileWidth / 2,
                (int)tileCenter.Y - _metadata.TileHeight / 2,
                _metadata.TileWidth,
                _metadata.TileHeight
            );
            _spriteBatch.Draw(_whitePixel, transitionRect, Color.Yellow * 0.5f);
        }

        // Draw groups (red rectangles)
        foreach (var group in _worldSnapshot.Groups)
        {
            var rect = new Rectangle(
                (int)group.CurrentPosition.X - 25,
                (int)group.CurrentPosition.Y - 25,
                50, 50
            );
            _spriteBatch.Draw(_whitePixel, rect, Color.Red);
        }

        // Draw player (green rectangle)
        _spriteBatch.Draw(_whitePixel, _playerRect, Color.Green);

        // Draw aggro range circles (optional visual)
        foreach (var group in _worldSnapshot.Groups)
        {
            if (group.IsAggroed)
            {
                // Draw aggro indicator
                var indicatorRect = new Rectangle(
                    (int)group.CurrentPosition.X - 30,
                    (int)group.CurrentPosition.Y - 30,
                    60, 60
                );
                _spriteBatch.Draw(_whitePixel, indicatorRect, Color.Orange * 0.3f);
            }
        }

        _spriteBatch.End();

    }

    private void UpdateRectangles()
    {
        _groupRects.Clear();
        foreach (var group in _worldSnapshot.Groups)
        {
            _groupRects.Add(new Rectangle(
                (int)group.CurrentPosition.X - 25,
                (int)group.CurrentPosition.Y - 25,
                50, 50
            ));
        }
    }


}