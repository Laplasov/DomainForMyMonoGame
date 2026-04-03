using System;
using System.Collections.Generic;
using System.Text;
using UnceasingFear.Domain.Shared;
using UnceasingFear.Domain.World.Enums;
using UnceasingFear.Domain.World.ValueObjects;
using UnceasingFear.Domain.World.Events;

namespace UnceasingFear.Domain.World.Entities
{
    public class EnemyGroup : Entity
    {
        public EnemyGroupId Id { get; }
        public TileId SpawnTile { get; }
        public EnemyTemplate EnemyTemplateName { get; }
        public MovementPattern MovementPattern { get; }
        public AggroRange AggroRange { get; }
        public MovementSpeed Speed { get; }
        public WorldPosition CurrentPosition { get; private set; }
        public bool IsDefeated { get; private set; }
        public void MoveTo(WorldPosition position) 
            => CurrentPosition = position;
        public void Defeat()
        {
            IsDefeated = true;
            AddDomainEvent(new EnemyGroupDefeatedEvent(Id, SpawnTile));
        }

        public bool IsAggroedBy(WorldPosition playerPosition)
            => !IsDefeated && AggroRange.IsInRange(CurrentPosition, playerPosition);
    }
}
