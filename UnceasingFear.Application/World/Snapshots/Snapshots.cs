using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.World.ValueObjects;

namespace UnceasingFear.Application.World.Snapshots
{
    public record struct WorldSnapshot(
            SceneId CurrentScene,
            WorldPosition PlayerPosition,
            IReadOnlyList<GroupSnapshot> Groups,
            IReadOnlyList<TileCoord> TransitionTiles
        );
    public record struct GroupSnapshot(
        GroupId Id,
        WorldPosition CurrentPosition,
        bool IsDefeated,
        bool IsAggroed
    );
}
