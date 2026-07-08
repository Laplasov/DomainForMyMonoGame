using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.Shared;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Domain.World.ValueObjects;

namespace UnceasingFear.Application.World.Snapshots
{
    public record struct WorldSnapshot(
            SceneId CurrentScene,
            WorldPosition PlayerPosition,
            TileMapMetadata TileMapMetadata,
            IReadOnlyList<EntitySnapshot> Entitis,
            IReadOnlyList<TileCoord> TransitionTiles,
            bool BattleTriggered,
            IReadOnlyList<Item> PlayerInventory,
            IReadOnlyList<UnitProfile> PartyProfiles
        );
    public enum EntityType { Group, Object }
    public record struct EntitySnapshot(
        EntityId Id,
        WorldPosition CurrentPosition,
        EntityType Type,
        bool IsDefeated,
        bool IsAggroed
    );

}
