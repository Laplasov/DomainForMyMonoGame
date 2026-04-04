using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using UnceasingFear.Domain.Shared;
using UnceasingFear.Domain.World.Entities;
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
        public SceneTransition? FindTriggeredTransition(TileCoord playerTile)
            => _transitions.FirstOrDefault(t => t.TriggerTile == playerTile);
        public IEnumerable<Group> GetAggroedGroups(WorldPosition playerPosition)
            => _groups.Where(g => g.IsAggroedBy(playerPosition));
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
