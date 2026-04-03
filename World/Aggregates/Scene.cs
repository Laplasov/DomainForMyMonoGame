using System;
using System.Collections.Generic;
using System.Text;
using UnceasingFear.Domain.Shared;
using UnceasingFear.Domain.World.Entities;
using UnceasingFear.Domain.World.ValueObjects;

namespace UnceasingFear.Domain.World.Aggregates
{
    public class Scene : Entity
    {
        public SceneId Id { get; }
        public WorldPosition PlayerStartPosition { get; }

        private readonly List<SceneTransition> _transitions = new();
        private readonly List<EnemyGroup> _enemyGroups = new();

        public IReadOnlyList<SceneTransition> Transitions => _transitions.AsReadOnly();
        public IReadOnlyList<EnemyGroup> EnemyGroups => _enemyGroups.AsReadOnly();

        public void AddTransition(SceneTransition transition)
        {
            if (_transitions.Any(t => t.TriggerTile == transition.TriggerTile))
                throw new InvalidOperationException("Tile already has a transition");
            _transitions.Add(transition);
        }

        public void AddEnemyGroup(EnemyGroup group)
            => _enemyGroups.Add(group);
        public SceneTransition? FindTriggeredTransition(TileId playerTile)
            => _transitions.FirstOrDefault(t => t.TriggerTile == playerTile);
        public IEnumerable<EnemyGroup> GetAggroedGroups(WorldPosition playerPosition)
            => _enemyGroups.Where(g => g.IsAggroedBy(playerPosition));
        public EnemyGroup? FindGroupAtTile(TileId tile)
            => _enemyGroups.FirstOrDefault(g => g.SpawnTile == tile);
    }
}
