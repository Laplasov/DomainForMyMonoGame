using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using UnceasingFear.Application.World;
using UnceasingFear.Domain.Shared.Events;
using UnceasingFear.Domain.World.Aggregates;
using UnceasingFear.Domain.World.Entities;
using UnceasingFear.Domain.World.ValueObjects;

namespace UnceasingFear.TestImplementation;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    public IEventDispatcher EventDispatcher { get; } = new DomainEventDispatcher();

    // Domain objects
    private Scene _scene;
    private Vector2 _playerPosition;
    private const float PlayerSpeed = 200f;

    // Visual helpers
    private Texture2D _whitePixel;
    private Rectangle _playerRect;
    private List<Rectangle> _groupRects = new();

    TileMapMetadata _metadata;

    private Group _playerGroup;

    bool _isBattle = false;
    BattleInstance _battle;


    private WorldApplicationService _appServiceWorld;

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
        // Create domain scene
        _metadata = new TileMapMetadata(
            Width: 20,
            Height: 20,
            TileWidth: 64,
            TileHeight: 64,
            LayerScale: 1f
        );

        _scene = new Scene(
            id: SceneId.From("TestScene"),
            mapMetadata: _metadata
        );


        // Add test groups
        var group1 = GroupFactory.CreateGroup1Goblin();
        var group2 = GroupFactory.CreateGroup2Slime();
        _playerGroup = GroupFactory.CreateGroupPlayer();

        _scene.AddGroup(group1);
        _scene.AddGroup(group2);
        _scene.AddGroup(_playerGroup);

        // Add test transition
        var transition = new SceneTransition(
            triggerTile: new TileCoord(5, 4),   // world center ~(352, 288)
            targetScene: SceneId.From("NextScene"),
            nextSceneTile: new WorldPosition(600, 600)
        );
        _scene.AddTransition(transition);

        // Initialize player
        _playerPosition = new Vector2(_playerGroup.SpawnPosition.X, _playerGroup.SpawnPosition.Y);

        EventDispatcher.Subscribe<SharedEvents.EnterBattleEvent>(e =>
        {
            _isBattle = true;
            _battle = new BattleInstance(_graphics, _spriteBatch, this, EventDispatcher, e.AllyProfiles, e.EnemyProfiles);
        });


        _appServiceWorld = new WorldApplicationService(_scene, _playerGroup, EventDispatcher);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Create 1x1 white texture for drawing rectangles
        _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
        _whitePixel.SetData(new[] { Color.White });

        UpdateRectangles();

    }

    protected override void Update(GameTime gameTime)
    {
        if(!_isBattle)
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

        _appServiceWorld.UpdatePositions(_playerPosition.X, _playerPosition.Y, delta);
        if (keyboard.IsKeyDown(Keys.C))
        {
            _appServiceWorld.UpdateTransition();
            _scene = _appServiceWorld.CurrentScene;
        }

        UpdateRectangles();
    }
    protected override void Draw(GameTime gameTime)
    {
        if (!_isBattle)
            DrawWorld(gameTime);
        else
            _battle.Draw(gameTime);

        base.Draw(gameTime);
    }
    protected void DrawWorld(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        // Draw transitions (yellow tiles)
        foreach (var transition in _scene.Transitions)
        {
            var tileCenter = _scene.MapMetadata.TileToWorld(transition.TriggerTile);
            var transitionRect = new Rectangle(
                (int)tileCenter.X - _scene.MapMetadata.TileWidth / 2,
                (int)tileCenter.Y - _scene.MapMetadata.TileHeight / 2,
                _scene.MapMetadata.TileWidth,
                _scene.MapMetadata.TileHeight
            );
            _spriteBatch.Draw(_whitePixel, transitionRect, Color.Yellow * 0.5f);
        }

        // Draw groups (red rectangles)
        foreach (var group in _scene.Groups)
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
        foreach (var group in _scene.Groups)
        {
            var aggroed = group.TryAggro(new WorldPosition(_playerPosition.X, _playerPosition.Y));
            if (aggroed)
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
        foreach (var group in _scene.Groups)
        {
            _groupRects.Add(new Rectangle(
                (int)group.CurrentPosition.X - 25,
                (int)group.CurrentPosition.Y - 25,
                50, 50
            ));
        }
    }


}