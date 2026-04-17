using UnceasingFear.Application.Commands;
using UnceasingFear.Application.World.Snapshots;
using UnceasingFear.Domain.Shared.Events;
using UnceasingFear.Domain.World.Aggregates;
using UnceasingFear.Domain.World.Entities;
using UnceasingFear.Domain.World.ValueObjects;

namespace UnceasingFear.Application.World
{
    public record struct MovePlayerCommand(float InputX, float InputY, float DeltaTime);
    public record struct RequestTransitionCommand();

    public class WorldApplicationService
    {
        private Scene _scene;
        private TileCoord _currentTileCoordPlayer;
        private Group _currentPlayer;
        public Scene CurrentScene => _scene;
        public WorldPosition PlayerPosition => _currentPlayer.CurrentPosition;
        public IEventDispatcher EventDispatcher { get; }
        public ICommandDispatcher CommandDispatcher { get; }
        public WorldApplicationService(Scene scene, Group currentPlayer, IEventDispatcher eventDispatcher, ICommandDispatcher commandDispatcher)
        {
            _scene = scene;
            _currentPlayer = currentPlayer;
            EventDispatcher = eventDispatcher;
            CommandDispatcher = commandDispatcher;

            CommandDispatcher.Register<MovePlayerCommand>(UpdatePositions);
            CommandDispatcher.Register<RequestTransitionCommand>(UpdateTransition);
        }

        public void UpdatePositions(MovePlayerCommand cmd)
        {
            var playerInput = new WorldPosition(cmd.InputX, cmd.InputY);
            _currentPlayer.MoveTo(playerInput);
            _currentTileCoordPlayer = _scene.MapMetadata.WorldToTile(playerInput);

            foreach (var group in _scene.Groups)
            {
                if (group.IsDefeated) continue;
                if (!group.TryAggro(_currentPlayer.CurrentPosition)) continue;

                var groupTile = _scene.MapMetadata.WorldToTile(group.CurrentPosition);
                if (groupTile == _currentTileCoordPlayer && group != _currentPlayer)
                {
                    EventDispatcher.Dispatch(new SharedEvents.EnterBattleEvent(
                        _currentPlayer.Template.Profiles,
                        group.Template.Profiles));
                    return;
                }

                var velocity = group.ComputeVelocity(playerInput);
                if (velocity.IsZero) continue;

                group.MoveTo(velocity.Apply(group.CurrentPosition, cmd.DeltaTime));
            }
        }

        public void UpdateTransition(RequestTransitionCommand cmd)
        {
            var transition = _scene.TryTriggerTransition(_currentTileCoordPlayer);

            if (transition != null)
            {
                var target = transition.Value;
                _scene = new Scene(
                    id: target.TargetScene,
                    mapMetadata: _scene.MapMetadata
                );
                _scene.AddGroup(_currentPlayer);
                _currentPlayer.MoveTo(target.NextSceneTile);
                _scene.PlayerEntered(target.NextSceneTile);
            }
        }

        public WorldSnapshot GetSnapshot() => new(
            _scene.Id,
            PlayerPosition,
            _scene.Groups.Select(g => new GroupSnapshot(g.Id, g.CurrentPosition, g.IsDefeated, g.TryAggro(PlayerPosition))).ToList(),
            _scene.Transitions.Select(t => t.TriggerTile).ToList()
        );
    }
}
