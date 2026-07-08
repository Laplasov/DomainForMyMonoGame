using UnceasingFear.Application.Collision;
using UnceasingFear.Application.Commands;
using UnceasingFear.Application.Repository;
using UnceasingFear.Application.World.Snapshots;
using UnceasingFear.Domain.Combat.Events;
using UnceasingFear.Domain.Shared.Enums;
using UnceasingFear.Domain.Shared.Events;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Domain.World.Aggregates;
using UnceasingFear.Domain.World.Entities;
using UnceasingFear.Domain.World.Enums;
using UnceasingFear.Domain.World.ValueObjects;
using static UnceasingFear.Application.Commands.SharedCommands;
using static UnceasingFear.Domain.Shared.Events.SharedEvents;

namespace UnceasingFear.Application.World
{
    public record struct MovePlayerCommand(float InputX, float InputY, float DeltaTime);
    public record struct SwapPartySlotsCommand(int SlotA, int SlotB);
    public record struct RequestTransitionCommand();
    public record struct EndBattleCommand();
    public record struct EquipItemCommand(Item item, UnitProfile owner);
    public record struct UnequipItemCommand(Item item, UnitProfile owner);
    public record struct InteractCommand();
    public record struct AdvanceDialogueCommand(DialogueChoice Choice);

    public class WorldApplicationService
    {
        private Scene _scene;
        private TileCoord _currentTileCoordPlayer;
        private Group _currentPlayer;
        private Group? _activeEnemy;
        private Group? _activeDialogueTarget;
        public Scene CurrentScene => _scene;
        public WorldPosition PlayerPosition => _currentPlayer.CurrentPosition;
        
        private bool _battleTriggered = false;
        private DialogueChoice? _pendingBattleChoice = null;
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
            CommandDispatcher.Register<EquipItemCommand>(EquipItem);
            CommandDispatcher.Register<UnequipItemCommand>(UnequipItem);
            CommandDispatcher.Register<InteractCommand>(Interact);
            CommandDispatcher.Register<AdvanceDialogueCommand>(AdvanceDialogue);


            EventDispatcher.Subscribe<OutOfBattleEvent>(EndBattle);
            EventDispatcher.Subscribe<PauseGame>((e) => IsPaused = e.ShouldPause);

        }
        private void EndBattle(OutOfBattleEvent e)
        {
            _battleTriggered = false;

            // 1. Get the profiles of units who fought (Slots 1-6)
            var updatedBattleProfiles = e.AllyProfiles.ToList();

            // 2. Get the full 9-slot roster
            var fullRoster = _currentPlayer.Template.Profiles.ToList();

            // 3. Loop through the full roster and apply changes
            for (int i = 0; i < fullRoster.Count; i++)
            {
                var rosterUnit = fullRoster[i];

                if (rosterUnit.SlotIndex <= 6)
                {
                    var battleUnit = updatedBattleProfiles.FirstOrDefault(p => p.SlotIndex == rosterUnit.SlotIndex);
                    if (!string.IsNullOrEmpty(battleUnit.Name))
                    {
                        rosterUnit = battleUnit;
                    }
                }

                if (rosterUnit.Name == "Player")
                {
                    rosterUnit = rosterUnit.AddToStash(e.CollectedLoot);
                }

                fullRoster[i] = rosterUnit;
            }

            // 5. Save the fully merged and updated roster back to the group
            _currentPlayer.UpdateProfiles(fullRoster.AsReadOnly());

            // Apply enemy profile updates
            if (_activeEnemy != null && e.EnemyProfiles != null && e.EnemyProfiles.Count > 0)
            {
                _activeEnemy.UpdateProfiles(e.EnemyProfiles);
            }

            _activeEnemy?.UpdateDefeatedState();

            // 6. Handle Nested Dialogue
            if (_activeDialogueTarget != null && _pendingBattleChoice.HasValue)
            {
                var choice = _pendingBattleChoice.Value;
                bool enemyDefeated = _activeEnemy?.IsDefeated ?? false;

                // Determine next node based on battle outcome
                string nextNodeId = enemyDefeated ? choice.Target.Left : choice.Target.Right;

                // Update the dialogue tree to the next node
                var newTree = _activeDialogueTarget.DialogueTree.SetNode(nextNodeId);
                _activeDialogueTarget.SetDialogueTree(newTree);

                var newNode = newTree.CurrentNode;
                EventDispatcher.Dispatch(new DialogueAdvancedEvent(newNode.Speaker, newNode.Text, newNode.Choices));

                _pendingBattleChoice = null;
            }
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

            var finalPosition = MovePlayer(cmd.InputX, cmd.InputY);
            HandelGroups(finalPosition, cmd.DeltaTime);

        }
        private WorldPosition MovePlayer(float InputX, float InputY)
        {
            var finalPosition = _lastPlayerPosition;
            var lastTile = _scene.MapMetadata.WorldToTile(_lastPlayerPosition);

            var testPosX = new WorldPosition(_lastPlayerPosition.X + InputX, _lastPlayerPosition.Y);
            var testTileX = _scene.MapMetadata.WorldToTile(testPosX);
            if (_scene.Collision.IsWalkable(testTileX, lastTile).x)
                finalPosition = new WorldPosition(testPosX.X, finalPosition.Y);

            var testPosY = new WorldPosition(finalPosition.X, _lastPlayerPosition.Y + InputY);
            var testTileY = _scene.MapMetadata.WorldToTile(testPosY);
            if (_scene.Collision.IsWalkable(testTileY, lastTile).y)
                finalPosition = new WorldPosition(finalPosition.X, testPosY.Y);

            _lastPlayerPosition = finalPosition;
            _currentPlayer.MoveTo(finalPosition);
            _currentTileCoordPlayer = _scene.MapMetadata.WorldToTile(finalPosition);
            return finalPosition;
        }
        
        private void HandelGroups(WorldPosition finalPosition, float DeltaTime)
        {

            float interactionRadius = _scene.MapMetadata.TileWidth;

            foreach (var group in _scene.Groups)
            {
                if (group.IsDefeated || group == _currentPlayer) continue;


                //var groupTile = _scene.MapMetadata.WorldToTile(group.CurrentPosition);
                //if (groupTile == _currentTileCoordPlayer && group != _currentPlayer)

                bool isInRange = group.CurrentPosition.DistanceTo(_currentPlayer.CurrentPosition) <= interactionRadius;

                if (isInRange && group.UnitBehavior == UnitBehavior.Chase)
                {
                    BegineBattleWithGroup(group);
                    return;
                }
                if (group.TryAggro(_currentPlayer.CurrentPosition))
                {
                    var velocity = group.ComputeVelocity(finalPosition);
                    if (!velocity.IsZero)
                    {
                        group.MoveTo(velocity.Apply(group.CurrentPosition, DeltaTime));
                    }
                }
            }
        }
        private void Interact(InteractCommand cmd)
        {
            if (IsPaused || _battleTriggered) return;

            float interactionRadius = _scene.MapMetadata.TileWidth;
            var playerPos = _currentPlayer.CurrentPosition;

            Group? closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (var group in _scene.Groups)
            {
                if (group.IsDefeated || group == _currentPlayer) continue;

                float distance = group.CurrentPosition.DistanceTo(playerPos);
                if (distance <= interactionRadius && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = group;
                }
            }

            if (closestTarget == null) return;

            if (MovementPatternHelper.IsAggresive(closestTarget.UnitBehavior))
                BegineBattleWithGroup(closestTarget);
            else
                CommunicateWithGroup(closestTarget);
        }

        private void BegineBattleWithGroup(Group activeEnemy)
        {
            _activeEnemy = activeEnemy;
            var activeParty = _currentPlayer.Template.Profiles.Where(p => p.SlotIndex <= 6).ToList().AsReadOnly();
            EventDispatcher.Dispatch(new EnterBattleEvent(activeParty, activeEnemy.Template.Profiles));
            _battleTriggered = true;
        }
        private void CommunicateWithGroup(Group target)
        {
            _activeDialogueTarget = target;
            IsPaused = true;
            EventDispatcher.Dispatch(new PauseGame(true));

            // Get the current node from the Domain's state
            var node = target.DialogueTree.CurrentNode;

            // ✅ Fire the event with ONLY the data the UI needs
            EventDispatcher.Dispatch(new DialogueStartedEvent(node.Speaker, node.Text, node.Choices));

        }
        private void AdvanceDialogue(AdvanceDialogueCommand cmd)
        {
            if (_activeDialogueTarget == null) return;

            var profiles = _currentPlayer.Template.Profiles.ToList();
            var playerProfile = profiles.FirstOrDefault(p => p.Name == "Player");
            if (string.IsNullOrEmpty(playerProfile.Name)) return;

            var choice = cmd.Choice;

            switch (choice.Action)
            {
                case ChoiceAction.End:
                    // 1. Apply conditions/rewards and get the new tree
                    var (newTreeEnd, updatedProfileEnd) = _activeDialogueTarget.DialogueTree.UpdateCurrentNode(choice, playerProfile);
                    _activeDialogueTarget.SetDialogueTree(newTreeEnd);

                    // 2. Apply updated profile back to player using Select (No indexes!)
                    var newProfilesEnd = profiles.Select(p => p.Name == "Player" ? updatedProfileEnd : p).ToList();
                    _currentPlayer.UpdateProfiles(newProfilesEnd.AsReadOnly());

                    // 3. Clean up and unpause
                    _activeDialogueTarget = null;
                    IsPaused = false;
                    EventDispatcher.Dispatch(new DialogueEndEvent());
                    EventDispatcher.Dispatch(new PauseGame(false));
                    break;

                case ChoiceAction.Continue:
                    // 1. Apply conditions/rewards and get the new tree
                    var (newTreeContinue, updatedProfileContinue) = _activeDialogueTarget.DialogueTree.UpdateCurrentNode(choice, playerProfile);
                    _activeDialogueTarget.SetDialogueTree(newTreeContinue);

                    // 2. Apply updated profile back to player using Select (No indexes!)
                    var newProfilesContinue = profiles.Select(p => p.Name == "Player" ? updatedProfileContinue : p).ToList();
                    _currentPlayer.UpdateProfiles(newProfilesContinue.AsReadOnly());

                    // 3. Notify UI
                    var newNode = newTreeContinue.CurrentNode;
                    EventDispatcher.Dispatch(new DialogueAdvancedEvent(newNode.Speaker, newNode.Text, newNode.Choices));
                    break;

                case ChoiceAction.AttackCurrent:
                    // 1. Check conditions and apply mutations (like TakeItem)
                    var result = choice.CheckConditionsAndReciveItems(playerProfile);

                    // 2. Apply the resulting profile (whether it changed or not)
                    var newProfilesAttack = profiles.Select(p => p.Name == "Player" ? result.Profile : p).ToList();
                    _currentPlayer.UpdateProfiles(newProfilesAttack.AsReadOnly());

                    // 3. If conditions failed, route to Right target instead of fighting
                    if (result.Target == choice.Target.Right)
                    {
                        var failTree = _activeDialogueTarget.DialogueTree.SetNode(result.Target);
                        _activeDialogueTarget.SetDialogueTree(failTree);
                        var failNode = failTree.CurrentNode;
                        EventDispatcher.Dispatch(new DialogueAdvancedEvent(failNode.Speaker, failNode.Text, failNode.Choices));
                        break;
                    }

                    // 4. Conditions passed - start the battle
                    _pendingBattleChoice = choice;
                    _activeEnemy = _activeDialogueTarget;
                    var activeParty = _currentPlayer.Template.Profiles.Where(p => p.SlotIndex <= 6).ToList().AsReadOnly();
                    EventDispatcher.Dispatch(new EnterBattleEvent(activeParty, _activeEnemy.Template.Profiles));
                    _battleTriggered = true;
                    break;

                case ChoiceAction.UnitShop:
                    break;
                case ChoiceAction.ItemShop:
                    break;
                case ChoiceAction.AttackFromSource:
                    break;
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
        private void EquipItem(EquipItemCommand cmd)
        {
            var profiles = _currentPlayer.Template.Profiles.ToList();

            var ownerProfile = profiles.FirstOrDefault(p => cmd.owner == p);
            if (string.IsNullOrEmpty(ownerProfile.Name)) return;

            var playerProfile = profiles.FirstOrDefault(p => p.Name == "Player");
            if (string.IsNullOrEmpty(playerProfile.Name)) return;

            var stash = playerProfile.Stash.ToList();

            Item itemToEquip = stash.Find(i => i.Id == cmd.item.Id);

            if (itemToEquip.Id == Guid.Empty) return;

            stash.Remove(itemToEquip);

            var equipped = ownerProfile.EquippedItems.ToList();
            equipped.Add(itemToEquip);

            var updatedPlayer = playerProfile with { Stash = stash.AsReadOnly() };
            var updatedOwner = ownerProfile with { EquippedItems = equipped.AsReadOnly() };

            var newProfiles = profiles.Select(p =>
            {
                if (p == playerProfile && p == ownerProfile)
                    return updatedPlayer with { EquippedItems = updatedOwner.EquippedItems };

                if (p == playerProfile) return updatedPlayer;
                if (p == ownerProfile) return updatedOwner;
                return p;
            }).ToList();

            _currentPlayer.UpdateProfiles(newProfiles.AsReadOnly());
        }

        private void UnequipItem(UnequipItemCommand cmd)
        {
            var profiles = _currentPlayer.Template.Profiles.ToList();

            var ownerProfile = profiles.FirstOrDefault(p => cmd.owner == p);
            if (string.IsNullOrEmpty(ownerProfile.Name)) return;

            var playerProfile = profiles.FirstOrDefault(p => p.Name == "Player");
            if (string.IsNullOrEmpty(playerProfile.Name)) return;

            var equipped = ownerProfile.EquippedItems.ToList();

            // Find the item by its unique Guid Id
            Item itemToUnequip = equipped.Find(i => i.Id == cmd.item.Id);

            if (itemToUnequip.Id == Guid.Empty) return;

            equipped.Remove(itemToUnequip);

            // Add the item to the player's stash (captures the new struct)
            var updatedPlayer = playerProfile.AddToStash(new[] { itemToUnequip });
            var updatedOwner = ownerProfile with { EquippedItems = equipped.AsReadOnly() };

            var newProfiles = profiles.Select(p =>
            {
                if (p == playerProfile && p == ownerProfile)
                    return updatedPlayer with { EquippedItems = equipped.AsReadOnly() };

                if (p == playerProfile) return updatedPlayer;
                if (p == ownerProfile) return updatedOwner;
                return p;
            }).ToList();

            _currentPlayer.UpdateProfiles(newProfiles.AsReadOnly());
        }

        public WorldSnapshot GetSnapshot()
        {
            var playerProfile = _currentPlayer.Template.Profiles.FirstOrDefault(p => p.Name == "Player");
            var inventory = playerProfile.Stash ?? new List<Item>().AsReadOnly();
            var groups = _scene.Groups.Select(g => new EntitySnapshot(g.Id, g.CurrentPosition, EntityType.Group, g.IsDefeated, g.IsAggroedBy(PlayerPosition))).ToList();

            return new(
            _scene.Id,
            PlayerPosition,
            _scene.MapMetadata,
            groups,
            _scene.Transitions.Select(t => t.TriggerTile).ToList(),
            _battleTriggered,
            inventory,
            _currentPlayer.Template.Profiles
            );
        }
    }
}
