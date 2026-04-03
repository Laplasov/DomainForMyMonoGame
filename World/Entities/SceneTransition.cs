using System;
using System.Collections.Generic;
using System.Text;
using UnceasingFear.Domain.Shared;
using UnceasingFear.Domain.World.ValueObjects;

namespace UnceasingFear.Domain.World.Entities
{
    public class SceneTransition : Entity
    {
        public TileId TriggerTile { get; }
        public SceneId TargetScene { get; }

        public SceneTransition(TileId triggerTile, SceneId targetScene)
        {
            TriggerTile = triggerTile;
            TargetScene = targetScene;
        }
    }
}
