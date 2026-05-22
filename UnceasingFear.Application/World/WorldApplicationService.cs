using UnceasingFear.Application.Collision;
using UnceasingFear.Application.Commands;
using UnceasingFear.Application.Repository;
using UnceasingFear.Application.World.Snapshots;
using UnceasingFear.Domain.Shared.Events;
using UnceasingFear.Domain.World.Aggregates;
using UnceasingFear.Domain.World.Entities;
using UnceasingFear.Domain.World.ValueObjects;
using static UnceasingFear.Application.Commands.SharedCommands;
using static UnceasingFear.Domain.Shared.Events.SharedEvents;

namespace UnceasingFear.Application.World
{
    public record struct MovePlayerCommand(float InputX, float InputY, float DeltaTime);
    public record struct RequestTransitionCommand();
    public record struct EndBattleCommand();

    public class WorldApplicationService
    {
        private Scene _scene;
        private TileCoord _currentTileCoordPlayer;
        private Group _currentPlayer;
        private Group? _activeEnemy;
        public Scene CurrentScene => _scene;
        public WorldPosition PlayerPosition => _currentPlayer.CurrentPosition;
        
        private bool _battleTriggered = false;

        public WorldPosition _lastPlayerPosition;

        private readonly ISceneProvider _sceneProvider;
        public IEventDispatcher EventDispatcher { get; }
        public ICommandDispatcher CommandDispatcher { get; }

        public WorldApplicationService(Scene scene, Group currentPlayer, IEventDispatcher eventDispatcher, ICommandDispatcher commandDispatcher, ISceneProvider sceneProvider)
        {
            _scene = scene;
            _currentPlayer = currentPlayer;
            EventDispatcher = eventDispatcher;
            CommandDispatcher = commandDispatcher;
            _sceneProvider = sceneProvider;
            _lastPlayerPosition = currentPlayer.CurrentPosition; 

            CommandDispatcher.Register<MovePlayerCommand>(UpdatePositions);
            CommandDispatcher.Register<RequestTransitionCommand>(UpdateTransition);
            CommandDispatcher.Register<EndBattleCommand>(EndBattle);

            EventDispatcher.Subscribe<OutOfBattleEvent>(_ => _battleTriggered = false);
        }
        private void EndBattle(EndBattleCommand cmd)
        {
            _activeEnemy?.Defeat();
            _battleTriggered = false;
        }

        private void UpdatePositions(MovePlayerCommand cmd)
        {
            var finalPosition = _lastPlayerPosition;
            var lastTile = _scene.MapMetadata.WorldToTile(_lastPlayerPosition);

            var testPosX = new WorldPosition(_lastPlayerPosition.X + cmd.InputX, _lastPlayerPosition.Y);
            var testTileX = _scene.MapMetadata.WorldToTile(testPosX);
            if (_scene.Collision.IsWalkable(testTileX, lastTile).x)
                finalPosition = new WorldPosition(testPosX.X, finalPosition.Y);

            var testPosY = new WorldPosition(finalPosition.X, _lastPlayerPosition.Y + cmd.InputY);
            var testTileY = _scene.MapMetadata.WorldToTile(testPosY);
            if (_scene.Collision.IsWalkable(testTileY, lastTile).y)
                finalPosition = new WorldPosition(finalPosition.X, testPosY.Y);

            _lastPlayerPosition = finalPosition;
            _currentPlayer.MoveTo(finalPosition);
            _currentTileCoordPlayer = _scene.MapMetadata.WorldToTile(finalPosition);

            foreach (var group in _scene.Groups)
            {
                if (group.IsDefeated) continue;
                if (!group.TryAggro(_currentPlayer.CurrentPosition)) continue;

                var groupTile = _scene.MapMetadata.WorldToTile(group.CurrentPosition);
                if (groupTile == _currentTileCoordPlayer && group != _currentPlayer)
                {
                    _activeEnemy = group;
                    EventDispatcher.Dispatch(new EnterBattleEvent(_currentPlayer.Template.Profiles, group.Template.Profiles));
                    _battleTriggered = true;
                    return;
                }

                var velocity = group.ComputeVelocity(finalPosition);
                if (velocity.IsZero) continue;

                group.MoveTo(velocity.Apply(group.CurrentPosition, cmd.DeltaTime));
            }
        }

        private void UpdateTransition(RequestTransitionCommand cmd)
        {
            var transition = _scene.TryTriggerTransition(_currentTileCoordPlayer);

            if (transition != null)
            {
                var target = transition.Value;

                var newScene = _sceneProvider.GetById(target.TargetScene);
                if (newScene == null) return;

                newScene.RemoveGroup(_currentPlayer.Id);
                newScene.AddGroup(_currentPlayer);

                _currentPlayer.MoveTo(target.NextSceneTile);
                _lastPlayerPosition = target.NextSceneTile;

                _scene = newScene;
                _scene.PlayerEntered(target.NextSceneTile);
            }
        }

        public WorldSnapshot GetSnapshot() => new(
            _scene.Id,
            PlayerPosition,
            _scene.MapMetadata,
            _scene.Groups.Select(g => new GroupSnapshot(g.Id, g.CurrentPosition, g.IsDefeated, g.TryAggro(PlayerPosition))).ToList(),
            _scene.Transitions.Select(t => t.TriggerTile).ToList(),
            _battleTriggered
        );
    }
}
