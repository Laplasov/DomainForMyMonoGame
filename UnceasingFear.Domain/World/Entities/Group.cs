using System;
using System.Collections.Generic;
using System.Text;
using UnceasingFear.Domain.Shared;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Domain.World.Enums;
using UnceasingFear.Domain.World.Events;
using UnceasingFear.Domain.World.Interfaces;
using UnceasingFear.Domain.World.ValueObjects;

namespace UnceasingFear.Domain.World.Entities
{
    public class Group : Entity, IInteractable
    {
        public EntityId Id { get; }
        public Template Template { get; private set; }
        public UnitBehavior UnitBehavior { get; }
        public WorldPosition SpawnPosition { get; private set; }
        public ProximityRange DetectionRange { get; }
        public MovementSpeed Speed { get; }
        public WorldPosition CurrentPosition { get; private set; }
        public bool IsDefeated { get; private set; }
        public DialogueTree DialogueTree { get; private set; }
        public Group(
            EntityId id,
            Template template,
            UnitBehavior unitBehavior,
            ProximityRange detectionRange,
            MovementSpeed speed,
            WorldPosition startPosition,
            DialogueTree dialogueTree)
        {
            Id = id;
            Template = template;
            UnitBehavior = unitBehavior;
            DetectionRange = detectionRange;
            Speed = speed;
            SpawnPosition = startPosition;
            CurrentPosition = startPosition;
            DialogueTree = dialogueTree;
            IsDefeated = false;
        }
        public Group Clone() => new Group(
            id: Id,
            template: Template,
            unitBehavior: UnitBehavior,
            detectionRange: DetectionRange,
            speed: Speed,
            startPosition: SpawnPosition,
            dialogueTree: DialogueTree
        );
        public void ChangeSpawn(WorldPosition position) => SpawnPosition = position;
        public void MoveTo(WorldPosition position) => CurrentPosition = position;
        public void SetDialogueTree(DialogueTree tree) => DialogueTree = tree;
        public void UpdateProfiles(IReadOnlyList<UnitProfile> newProfiles) => Template = new Template(Template.TemplateName, newProfiles);
        public IReadOnlyList<UnitProfile> CreateActiveParty() => Template.Profiles.Where(p => p.SlotIndex <= 6).ToList().AsReadOnly();
        public void Defeat()
        {
            IsDefeated = true;
            AddDomainEvent(new GroupDefeatedEvent(Id, CurrentPosition));
        }

        public void UpdateDefeatedState()
        {
            // A group is only defeated if ALL of its units are dead (HP <= 0)
            bool allDead = Template.Profiles.All(p => !p.Stats.IsAlive);

            // If they are all dead, and we haven't already marked them as defeated
            if (allDead && !IsDefeated)
            {
                IsDefeated = true;
                AddDomainEvent(new GroupDefeatedEvent(Id, CurrentPosition));
            }
        }

        public bool TryAggro(WorldPosition playerPosition)
        {
            if (!IsAggroedBy(playerPosition)) return false;
            AddDomainEvent(new GroupAggroedEvent(Id, playerPosition));
            return true;
        }
        public bool IsAggroedBy(WorldPosition playerPosition)
            => !IsDefeated && DetectionRange.IsInRange(CurrentPosition, playerPosition);
        private Velocity ComputeTerritorialVelocity(WorldPosition playerPosition)
        {
            if (DetectionRange.IsInRange(SpawnPosition, playerPosition))
                return Velocity.Toward(CurrentPosition, playerPosition, Speed);

            if (CurrentPosition.DistanceTo(SpawnPosition) > 2f)
                return Velocity.Toward(CurrentPosition, SpawnPosition, Speed);

            return Velocity.Zero;
        }

        public Velocity ComputeVelocity(WorldPosition playerPosition)
        {
            return UnitBehavior switch
            {
                UnitBehavior.Chase => Velocity.Toward(CurrentPosition, playerPosition, Speed),
                UnitBehavior.Stationary => Velocity.Zero,
                UnitBehavior.Territorial => ComputeTerritorialVelocity(playerPosition),
                UnitBehavior.PlayerControlled => Velocity.FromInput(playerPosition.X, playerPosition.Y, Speed),
                _ => Velocity.Zero
            };
        }
    }
}
