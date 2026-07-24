using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Domain.World.ValueObjects;

namespace UnceasingFear.Domain.World.Interfaces
{
    public interface IWorldEntity
    {
        EntityId Id { get; }
        WorldPosition CurrentPosition { get; }
    }

    public interface IInteractable : IWorldEntity
    {
        ProximityRange DetectionRange { get; }
        DialogueTree DialogueTree { get; }
        void SetDialogueTree(DialogueTree tree);
    }
}
