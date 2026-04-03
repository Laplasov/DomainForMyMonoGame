using System;
using System.Collections.Generic;
using System.Text;
using UnceasingFear.Domain.Shared.Events;
using UnceasingFear.Domain.World.Entities;
using UnceasingFear.Domain.World.ValueObjects;

namespace UnceasingFear.Domain.World.Events
{
    public record PlayerEnteredSceneEvent(SceneId SceneId, WorldPosition Position) : IDomainEvent;
    public record PlayerExitedSceneEvent(SceneId From, SceneId To) : IDomainEvent;
    public record EnemyGroupAggroedEvent(EnemyGroupId GroupId, WorldPosition PlayerPosition) : IDomainEvent;
    public record EnemyGroupDefeatedEvent(EnemyGroupId GroupId, TileId SpawnTile) : IDomainEvent;
    public record SceneTransitionTriggeredEvent(SceneId From, SceneId To) : IDomainEvent;
}
