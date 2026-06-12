using UnceasingFear.Application.Collision;
using UnceasingFear.Application.Commands;
using UnceasingFear.Application.Repository;
using UnceasingFear.Application.World.Snapshots;
using UnceasingFear.Domain.Combat.Events;
using UnceasingFear.Domain.Shared.Events;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Domain.World.Aggregates;
using UnceasingFear.Domain.World.Entities;
using UnceasingFear.Domain.World.ValueObjects;
using static UnceasingFear.Application.Commands.SharedCommands;
using static UnceasingFear.Domain.Shared.Events.SharedEvents;

namespace UnceasingFear.Application.World
{
    public record struct MovePlayerCommand(float InputX, float InputY, float DeltaTime);
    public record struct SwapPartySlotsCommand(int SlotA, int SlotB);
    public record struct RequestTransitionCommand();
    public record struct EndBattleCommand();

    public class WorldApplicationService
    {
        private Scene _scene;
        private TileCoord _currentTileCoordPlayer;
        private Group _currentPlayer;
        private Group? _activeEnemy;
        public Scene CurrentScene => _scene;
        public WorldPosition PlayerPosition => _currentPlayer.CurrentPosition;
        
        private bool _battleTriggered = false;
        public bool IsPaused { get; private set; } = false;

        public WorldPosition _lastPlayerPosition;

        private readonly ISceneProvider _sceneProvider;
        public IEventDispatcher EventDispatcher { get; }
        public ICommandDispatcher CommandDispatcher { get; }

        public WorldApplicationService(Scene scene, Group currentPlayer, IEventDispatcher eventDispatcher, ICommandDispatcher commandDispatcher, ISceneProvider sceneProvider)
        {
            _scene = scene;
            _currentPlayer = currentPlayer;
            EventDispatcher = eventDispatcher;
            CommandDispatcher = commandDispatcher;
            _sceneProvider = sceneProvider;
            _lastPlayerPosition = currentPlayer.CurrentPosition; 

            CommandDispatcher.Register<MovePlayerCommand>(UpdatePositions);
            CommandDispatcher.Register<RequestTransitionCommand>(UpdateTransition);
            CommandDispatcher.Register<SwapPartySlotsCommand>(SwapPartySlots);

            EventDispatcher.Subscribe<OutOfBattleEvent>(EndBattle);
            EventDispatcher.Subscribe<PauseGame>((e) => IsPaused = e.ShouldPause);
        }
        private void EndBattle(OutOfBattleEvent e)
        {
            _activeEnemy?.Defeat();
            _battleTriggered = false;

            // 1. Get the profiles of units who fought (Slots 1-6)
            var updatedBattleProfiles = e.AllyProfiles.ToList();

            // 2. Get the full 9-slot roster
            var fullRoster = _currentPlayer.Template.Profiles.ToList();

            // 3. Loop through the full roster and apply changes
            for (int i = 0; i < fullRoster.Count; i++)
            {
                var rosterUnit = fullRoster[i];

                // If this unit was in the active battle party (Slots 1-6), update their battle stats (HP/SP)
                if (rosterUnit.SlotIndex <= 6)
                {
                    var battleUnit = updatedBattleProfiles.FirstOrDefault(p => p.SlotIndex == rosterUnit.SlotIndex);
                    if (!string.IsNullOrEmpty(battleUnit.Name)) // If we found a match
                    {
                        rosterUnit = battleUnit; // Apply the battle damage/changes
                    }
                }

                // 4. Add loot to the "Player" specifically, even if they were in reserve (Slots 7-9)
                if (rosterUnit.Name == "Player")
                {
                    rosterUnit = rosterUnit.AddLoot(e.CollectedLoot);
                }

                fullRoster[i] = rosterUnit;
            }

            // 5. Save the fully merged and updated roster back to the group
            _currentPlayer.UpdateProfiles(fullRoster.AsReadOnly());
        }
        private void SwapPartySlots(SwapPartySlotsCommand cmd)
        {
            // Copy current profiles to a mutable list
            var profiles = _currentPlayer.Template.Profiles.ToList();

            // Find the units currently in these slots (they might be empty/default!)
            var unitA = profiles.FirstOrDefault(p => p.SlotIndex == cmd.SlotA);
            var unitB = profiles.FirstOrDefault(p => p.SlotIndex == cmd.SlotB);

            // Remove both from the list
            profiles.RemoveAll(p => p.SlotIndex == cmd.SlotA || p.SlotIndex == cmd.SlotB);

            // Re-add them with swapped slots (if they actually existed)
            if (!string.IsNullOrEmpty(unitA.Name))
                profiles.Add(unitA.AssignToSlot(cmd.SlotB));

            if (!string.IsNullOrEmpty(unitB.Name))
                profiles.Add(unitB.AssignToSlot(cmd.SlotA));

            // Update the group with the newly ordered profiles
            _currentPlayer.UpdateProfiles(profiles.OrderBy(p => p.SlotIndex).ToList().AsReadOnly());
        }

        private void UpdatePositions(MovePlayerCommand cmd)
        {
            if (IsPaused) return;

            var finalPosition = _lastPlayerPosition;
            var lastTile = _scene.MapMetadata.WorldToTile(_lastPlayerPosition);

            var testPosX = new WorldPosition(_lastPlayerPosition.X + cmd.InputX, _lastPlayerPosition.Y);
            var testTileX = _scene.MapMetadata.WorldToTile(testPosX);
            if (_scene.Collision.IsWalkable(testTileX, lastTile).x)
                finalPosition = new WorldPosition(testPosX.X, finalPosition.Y);

            var testPosY = new WorldPosition(finalPosition.X, _lastPlayerPosition.Y + cmd.InputY);
            var testTileY = _scene.MapMetadata.WorldToTile(testPosY);
            if (_scene.Collision.IsWalkable(testTileY, lastTile).y)
                finalPosition = new WorldPosition(finalPosition.X, testPosY.Y);

            _lastPlayerPosition = finalPosition;
            _currentPlayer.MoveTo(finalPosition);
            _currentTileCoordPlayer = _scene.MapMetadata.WorldToTile(finalPosition);

            foreach (var group in _scene.Groups)
            {
                if (group.IsDefeated) continue;
                if (!group.TryAggro(_currentPlayer.CurrentPosition)) continue;

                var groupTile = _scene.MapMetadata.WorldToTile(group.CurrentPosition);
                if (groupTile == _currentTileCoordPlayer && group != _currentPlayer)
                {
                    _activeEnemy = group;

                    var activeParty = _currentPlayer.Template.Profiles
                        .Where(p => p.SlotIndex <= 6)
                        .ToList()
                        .AsReadOnly();

                    EventDispatcher.Dispatch(new EnterBattleEvent(activeParty, group.Template.Profiles));
                    _battleTriggered = true;
                    return;
                }

                var velocity = group.ComputeVelocity(finalPosition);
                if (velocity.IsZero) continue;

                group.MoveTo(velocity.Apply(group.CurrentPosition, cmd.DeltaTime));
            }
        }

        private void UpdateTransition(RequestTransitionCommand cmd)
        {
            var transition = _scene.TryTriggerTransition(_currentTileCoordPlayer);

            if (transition != null)
            {
                var target = transition.Value;

                var newScene = _sceneProvider.GetById(target.TargetScene);
                if (newScene == null) return;

                newScene.RemoveGroup(_currentPlayer.Id);
                newScene.AddGroup(_currentPlayer);

                _currentPlayer.MoveTo(target.NextSceneTile);
                _lastPlayerPosition = target.NextSceneTile;

                _scene = newScene;
                _scene.PlayerEntered(target.NextSceneTile);
            }
        }

        public WorldSnapshot GetSnapshot()
        {
            var playerProfile = _currentPlayer.Template.Profiles.FirstOrDefault(p => p.Name == "Player");
            var inventory = playerProfile.LootDrops ?? new List<Loot>().AsReadOnly();

            return new(
            _scene.Id,
            PlayerPosition,
            _scene.MapMetadata,
            _scene.Groups.Select(g => new GroupSnapshot(g.Id, g.CurrentPosition, g.IsDefeated, g.TryAggro(PlayerPosition))).ToList(),
            _scene.Transitions.Select(t => t.TriggerTile).ToList(),
            _battleTriggered,
            inventory,
            _currentPlayer.Template.Profiles
            );
        }
    }
}
