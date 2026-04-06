using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using UnceasingFear.Domain.Shared;
using UnceasingFear.Domain.World.Entities;
using UnceasingFear.Domain.World.Events;
using UnceasingFear.Domain.World.ValueObjects;

namespace UnceasingFear.Domain.World.Aggregates
{
    public class Scene : Entity
    {
        public SceneId Id { get; }

        private readonly List<SceneTransition> _transitions = new();
        private readonly List<Group> _groups = new();

        public IReadOnlyList<SceneTransition> Transitions => _transitions.AsReadOnly();
        public IReadOnlyList<Group> Groups => _groups.AsReadOnly();
        public TileMapMetadata MapMetadata { get; }

        public Scene(SceneId id, TileMapMetadata mapMetadata)
        {
            Id = id;
            MapMetadata = mapMetadata;
        }

        public void AddTransition(SceneTransition transition)
        {
            if (_transitions.Any(t => t.TriggerTile == transition.TriggerTile))
                throw new InvalidOperationException("Tile already has a transition");
            _transitions.Add(transition);
        }

        public void AddGroup(Group group)
            => _groups.Add(group);

        public void PlayerEntered(WorldPosition position)
            => AddDomainEvent(new PlayerEnteredSceneEvent(Id, position));

        public SceneTransition? TryTriggerTransition(TileCoord playerTile)
        {
            var transition = FindTriggeredTransition(playerTile);
            if (transition.HasValue)
                AddDomainEvent(new SceneTransitionTriggeredEvent(Id, transition.Value.TargetScene));
            return transition;
        }
        private SceneTransition? FindTriggeredTransition(TileCoord playerTile)
        {
            return _transitions
                .Where(t => t.TriggerTile == playerTile)
                .Select(t => (SceneTransition?)t)
                .FirstOrDefault();
        }

        public IEnumerable<Group> GetAggroedGroups(WorldPosition playerPosition)
            => _groups.Where(g => g.TryAggro(playerPosition));

        public Group? FindGroupAtTile(WorldPosition tile)
            => _groups.FirstOrDefault(g => g.CurrentPosition == tile);
        public Group? FindGroupAtTile(TileCoord tile)
        {
            var worldPos = MapMetadata.TileToWorld(tile);
            return _groups.FirstOrDefault(g =>
                MapMetadata.WorldToTile(g.CurrentPosition) == tile);
        }
    }
}
