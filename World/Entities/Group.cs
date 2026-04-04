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

        public bool IsAggroedBy(WorldPosition playerPosition)
            => !IsDefeated && AggroRange.IsInRange(CurrentPosition, playerPosition);
    }
}
