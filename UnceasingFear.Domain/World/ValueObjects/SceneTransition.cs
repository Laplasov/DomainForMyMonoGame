using System;
using System.Collections.Generic;
using System.Text;
using UnceasingFear.Domain.Shared;

namespace UnceasingFear.Domain.World.ValueObjects
{
    public readonly record struct SceneTransition
    {
        public TileCoord TriggerTile { get; }
        public SceneId TargetScene { get; }
        public WorldPosition NextSceneTile { get; }

        public SceneTransition(TileCoord triggerTile, SceneId targetScene, WorldPosition nextSceneTile)
        {
            TriggerTile = triggerTile;
            TargetScene = targetScene;
            NextSceneTile = nextSceneTile;
        }
    }
}
