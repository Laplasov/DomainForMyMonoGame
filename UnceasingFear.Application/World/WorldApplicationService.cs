using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.Shared.Events;
using UnceasingFear.Domain.World.Aggregates;
using UnceasingFear.Domain.World.Entities;
using UnceasingFear.Domain.World.Enums;
using UnceasingFear.Domain.World.ValueObjects;

namespace UnceasingFear.Application.World
{
    public class WorldApplicationService
    {
        private Scene _scene;
        private TileCoord _currentTileCoordPlayer;
        private Group _currentPlayer;
        public Scene CurrentScene => _scene;
        public WorldPosition PlayerPosition => _currentPlayer.CurrentPosition;
        public IEventDispatcher EventDispatcher { get; }
        public WorldApplicationService(Scene scene, Group currentPlayer, IEventDispatcher eventDispatcher)
        {
            _scene = scene;
            _currentPlayer = currentPlayer;
            EventDispatcher = eventDispatcher;
        }

        public void UpdatePositions(float x, float y, float delta)
        {
            var playerInput = new WorldPosition(x, y);
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

                group.MoveTo(velocity.Apply(group.CurrentPosition, delta));
            }
        }

        public void UpdateTransition()
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

    }
}
