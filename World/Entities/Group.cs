using System;
using System.Collections.Generic;
using System.Text;
using UnceasingFear.Domain.Shared;
using UnceasingFear.Domain.World.Enums;
using UnceasingFear.Domain.World.ValueObjects;
using UnceasingFear.Domain.World.Events;

namespace UnceasingFear.Domain.World.Entities
{
    public class Group : Entity
    {
        public GroupId Id { get; }
        public Template TemplateName { get; }
        public MovementPattern MovementPattern { get; }
        public WorldPosition SpawnPosition { get; }
        public AggroRange AggroRange { get; }
        public MovementSpeed Speed { get; }
        public WorldPosition CurrentPosition { get; private set; }
        public bool IsDefeated { get; private set; }
        public Group(
            GroupId id,
            Template templateName,
            MovementPattern movementPattern,
            AggroRange aggroRange,
            MovementSpeed speed,
            WorldPosition startPosition)
        {
            Id = id;
            TemplateName = templateName;
            MovementPattern = movementPattern;
            AggroRange = aggroRange;
            Speed = speed;
            SpawnPosition = startPosition;
            CurrentPosition = startPosition;
            IsDefeated = false;
        }

        public void MoveTo(WorldPosition position) 
            => CurrentPosition = position;
        public void Defeat()
        {
            IsDefeated = true;
            AddDomainEvent(new GroupDefeatedEvent(Id, CurrentPosition));
        }
        public bool TryAggro(WorldPosition playerPosition)
        {
            if (!IsAggroedBy(playerPosition)) return false;
            AddDomainEvent(new GroupAggroedEvent(Id, playerPosition));
            return true;
        }
        private bool IsAggroedBy(WorldPosition playerPosition)
            => !IsDefeated && AggroRange.IsInRange(CurrentPosition, playerPosition);
        private Velocity ComputeTerritorialVelocity(WorldPosition playerPosition)
        {
            if (AggroRange.IsInRange(SpawnPosition, playerPosition))
                return Velocity.Toward(CurrentPosition, playerPosition, Speed);

            if (CurrentPosition.DistanceTo(SpawnPosition) > 2f)
                return Velocity.Toward(CurrentPosition, SpawnPosition, Speed);

            return Velocity.Zero;
        }

        public Velocity ComputeVelocity(WorldPosition playerPosition)
        {
            return MovementPattern switch
            {
                MovementPattern.Chase => Velocity.Toward(CurrentPosition, playerPosition, Speed),
                MovementPattern.Stationary => Velocity.Zero,
                MovementPattern.Territorial => ComputeTerritorialVelocity(playerPosition),
                _ => Velocity.Zero
            };
        }
    }
}
