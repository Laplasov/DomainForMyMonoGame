using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.Shared;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Domain.World.Enums;
using UnceasingFear.Domain.World.Interfaces;
using UnceasingFear.Domain.World.ValueObjects;

namespace UnceasingFear.Domain.World.Entities
{
    public enum WorldObjectType { Cauldron, Readable }
    public class WorldObject : Entity, IInteractable
    {
        public EntityId Id { get; }
        public WorldObjectType Type { get; }
        public WorldPosition CurrentPosition { get; }
        public ProximityRange DetectionRange { get; }
        public DialogueTree DialogueTree { get; private set; }
        public WorldObject(EntityId id, WorldObjectType type, WorldPosition position, ProximityRange detectionRange, DialogueTree dialogueTree)
        {
            Id = id;
            Type = type;
            CurrentPosition = position;
            DetectionRange = detectionRange;
            DialogueTree = dialogueTree;
        }
        public void SetDialogueTree(DialogueTree tree) => DialogueTree = tree;
        public WorldObject Clone() => new(Id, Type, CurrentPosition, DetectionRange, DialogueTree);
    }
}
