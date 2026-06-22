using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Managers;
using Gum.Wireframe;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using UnceasingFear.Application.Commands;
using UnceasingFear.Application.World;
using UnceasingFear.Application.World.Snapshots;
using UnceasingFear.Domain.Shared.Events;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Domain.Shared.ValueObjects.Abilities;
using UnceasingFear.Domain.World.ValueObjects;
using UnceasingFear.Presentation.Render.World.Tabs;
using static UnceasingFear.Domain.Shared.Events.SharedEvents;

namespace UnceasingFear.Presentation.Render
{
    public enum MenuTab { Character, Set, Items, Skills, Settings}

    public class PlayerMenu
    {
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ICommandDispatcher _commandDispatcher;

        private ColoredRectangleRuntime? _mainPanel;
        private MenuTab _activeTab = MenuTab.Character;
        private readonly Dictionary<MenuTab, Button> _tabButtons = new();
        private readonly Dictionary<MenuTab, ColoredRectangleRuntime> _tabContents = new();

        private readonly Dictionary<int, Button> _slotButtons = new();

        private int? _chosenSlotIndex = null;
        private int? _hoveredSlotIndex = null;

        private readonly UnitsStatsUI _statsUI = new();
        private readonly ItemDetailsUI _itemDetailsUI;

        private readonly List<Button> _itemButtons = new();

        private Item? _selectedItem = null;
        private Item? _hoveredItem = null;

        private ScrollViewer? _itemContainer;
        private ColoredRectangleRuntime? _statsContainer;
        private ColoredRectangleRuntime? _itemDetailsContainer;

        private Label? _goldLabel;

        // ── Skills Tab Fields ──────────────────────────────────────────────
        private ScrollViewer? _skillsListContainer;
        private readonly List<Button> _allAbilityButtons = new();
        private int _totalAbilityButtons = 0;

        private Ability? _selectedAbility = null;
        private Ability? _hoveredAbility = null;
        private readonly AbilityDetailsUI _abilityDetailsUI = new();
        private ColoredRectangleRuntime? _abilityDetailsContainer;
        private WorldSnapshot? _lastSnapshot;

        private ScrollViewer? _unitsListContainer;
        private readonly List<Button> _allUnitButtons = new();
        private int _totalUnitButtons = 0;

        public bool UnitSelection { get; set; } = false;
        public bool IsItemTabDirty { get; set; } = false;

        public WorldSnapshot? LastSnapshot => _lastSnapshot;

        public PlayerMenu(IEventDispatcher eventDispatcher, ICommandDispatcher commandDispatcher)
        {
            _eventDispatcher = eventDispatcher;
            _commandDispatcher = commandDispatcher;
            _itemDetailsUI = new(this, _commandDispatcher);
        }

        public bool IsVisible => _mainPanel != null;
        public void Update(WorldSnapshot snapshot)
        {
            if (!IsVisible) return;

            _lastSnapshot = snapshot;

            if (_itemContainer != null) _itemContainer.IsVisible = !UnitSelection;
            if (_unitsListContainer != null) _unitsListContainer.IsVisible = UnitSelection;
            if (_goldLabel != null) _goldLabel.IsVisible = !UnitSelection;

            if (UnitSelection)
            {
                RefreshUnitsTab();
                return;
            }

            if (_activeTab == MenuTab.Set)
            {
                RefreshSetTab(snapshot);
            }
            else if (_activeTab == MenuTab.Items)
            {
                RefreshItemsTab(snapshot);
            }
            else if (_activeTab == MenuTab.Skills)
            {
                RefreshSkillsTab();
            }
        }

        private void RefreshSkillsTab()
        {
            if (_skillsListContainer == null || _lastSnapshot == null) return;

            // Calculate total abilities to know if we need to rebuild the UI
            int totalAbilities = _lastSnapshot.Value.PartyProfiles
                .Where(p => !string.IsNullOrEmpty(p.Name))
                .Sum(p => p.Abilities.Count);

            // Only rebuild if the number of abilities has changed
            if (_totalAbilityButtons != totalAbilities)
            {
                _skillsListContainer.InnerPanel.Children.Clear();
                _allAbilityButtons.Clear();

                foreach (var profile in _lastSnapshot.Value.PartyProfiles)
                {
                    if (string.IsNullOrEmpty(profile.Name)) continue;

                    // 1. Add Unit Header (The "Block")
                    var unitLabel = new Label();
                    unitLabel.Text = $"--- {profile.Name} ---";
                    unitLabel.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
                    unitLabel.Visual.Width = 0; // 100% width
                    unitLabel.Visual.Height = 30;
                    _skillsListContainer.AddChild(unitLabel);

                    // 2. Add Ability Buttons for this Unit
                    foreach (var ability in profile.Abilities)
                    {
                        var btn = new Button();
                        // Indent visually so it looks nested under the unit
                        btn.Text = $"   ↳ {ability.Name}";
                        btn.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
                        btn.Visual.Width = 0;
                        btn.Visual.Height = 30;

                        var capturedAbility = ability;
                        btn.Click += (_, _) =>
                        {
                            _selectedAbility = capturedAbility;
                            _abilityDetailsUI.UpdateDetails(capturedAbility);
                        };

                        if (btn.Visual is InteractiveGue interactive)
                        {
                            interactive.RollOn += (_, _) =>
                            {
                                _hoveredAbility = capturedAbility;
                                if (_selectedAbility == null) _abilityDetailsUI.UpdateDetails(capturedAbility);
                            };

                            interactive.RollOff += (_, _) =>
                            {
                                _hoveredAbility = null;
                                if (_selectedAbility == null) _abilityDetailsUI.ClearDetails();
                                else _abilityDetailsUI.UpdateDetails(_selectedAbility.Value);
                            };
                        }

                        _skillsListContainer.AddChild(btn);
                        _allAbilityButtons.Add(btn);
                    }
                }
                _totalAbilityButtons = totalAbilities;
            }
        }

        private void RefreshSetTab(WorldSnapshot snapshot)
        {
            if (!IsVisible || !_tabContents.ContainsKey(MenuTab.Set)) return;

            for (int i = 1; i <= 9; i++)
            {
                if (!_slotButtons.TryGetValue(i, out var btn)) continue;

                var unit = snapshot.PartyProfiles.FirstOrDefault(p => p.SlotIndex == i);

                if (string.IsNullOrEmpty(unit.Name))
                {
                    // Empty slot
                    btn.Text = $"{i}: Empty";

                    // ✅ Only allow clicking an empty slot if we have a unit ready to move!
                    btn.IsEnabled = (_chosenSlotIndex != null);
                }
                else
                {
                    // Occupied slot
                    btn.Text = $"{i}: {unit.Name}";

                    // ✅ Always allow clicking an occupied slot (to select it, swap with it, or cancel selection)
                    btn.IsEnabled = true;
                }

                int? displaySlot = _chosenSlotIndex ?? _hoveredSlotIndex;
                if (displaySlot != null)
                {
                    var hoveredUnit = snapshot.PartyProfiles.FirstOrDefault(p => p.SlotIndex == displaySlot.Value);
                    _statsUI.UpdateStats(hoveredUnit);
                }
                else 
                {
                    _statsUI.ClearStats();
                }
            }
        }
        private void RefreshItemsTab(WorldSnapshot snapshot)
        {
            if (_itemContainer == null) return;
            var inventory = snapshot.PlayerInventory;

            int goldAmount = 0;
            var regularItems = new List<Item>();

            foreach (var item in inventory)
            {
                if (item.Type == "Money") goldAmount += item.Quantity;
                else regularItems.Add(item);
            }
            foreach (var profile in snapshot.PartyProfiles)
            {
                if (string.IsNullOrEmpty(profile.Name)) continue;
                foreach (var item in profile.EquippedItems)
                {
                    if (item.Type == "Money") continue;
                    else regularItems.Add(item);
                }
            }

            if (_goldLabel != null) _goldLabel.Text = $"Gold: {goldAmount}";

            // ✅ Rebuild if count changed OR dirty flag is set
            if (_itemButtons.Count != regularItems.Count || IsItemTabDirty)
            {
                foreach (var btn in _itemButtons) _itemContainer.RemoveChild(btn);
                _itemButtons.Clear();

                for (int i = 0; i < regularItems.Count; i++)
                {
                    var currentItem = regularItems[i]; // Only capturing the Item, NOT a UnitProfile!
                    var btn = new Button();
                    btn.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
                    btn.Visual.Width = 0;
                    btn.Visual.Height = 30;

                    btn.Click += (_, _) =>
                    {
                        _selectedItem = currentItem;
                        _itemDetailsUI.UpdateDetails(currentItem); // ✅ No owner passed
                    };

                    if (btn.Visual is InteractiveGue interactive)
                    {
                        interactive.RollOn += (_, _) =>
                        {
                            _hoveredItem = currentItem;
                            if (_selectedItem == null) _itemDetailsUI.UpdateDetails(currentItem);
                        };

                        interactive.RollOff += (_, _) =>
                        {
                            _hoveredItem = null;
                            if (_selectedItem == null) _itemDetailsUI.ClearDetails();
                            else _itemDetailsUI.UpdateDetails(_selectedItem.Value); // ✅ Fresh lookup
                        };
                    }

                    _itemContainer.AddChild(btn);
                    _itemButtons.Add(btn);
                }
                IsItemTabDirty = false;
            }

            // Always update text
            for (int i = 0; i < regularItems.Count; i++)
            {
                _itemButtons[i].Text = $"{regularItems[i].Name} (x{regularItems[i].Quantity})";
            }
        }

        private void RefreshUnitsTab()
        {
            if (_unitsListContainer == null || _lastSnapshot == null) return;

            int totalUnits = _lastSnapshot.Value.PartyProfiles
                .Count(p => !string.IsNullOrEmpty(p.Name));

            if (_totalUnitButtons != totalUnits)
            {
                _unitsListContainer.InnerPanel.Children.Clear();
                _allUnitButtons.Clear();

                foreach (var profile in _lastSnapshot.Value.PartyProfiles)
                {
                    if (string.IsNullOrEmpty(profile.Name)) continue;

                    var btn = new Button();
                    btn.Text = profile.Name;
                    btn.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
                    btn.Visual.Width = 0;
                    btn.Visual.Height = 30;

                    // ✅ Capture the SlotIndex, then look up the FRESH profile at click time
                    var capturedSlotIndex = profile.SlotIndex;
                    btn.Click += (_, _) =>
                    {
                        if (_lastSnapshot != null)
                        {
                            var freshProfile = _lastSnapshot.Value.PartyProfiles
                                .FirstOrDefault(p => p.SlotIndex == capturedSlotIndex);
                            if (!string.IsNullOrEmpty(freshProfile.Name))
                                _itemDetailsUI.SendCommand(freshProfile);
                        }
                    };

                    _unitsListContainer.InnerPanel.Children.Add(btn.Visual);
                    _allUnitButtons.Add(btn);
                }
                _totalUnitButtons = totalUnits;
            }
        }


        public void Show()
        {
            if (IsVisible) return;
            BuildUI();
        }

        public void Hide()
        {
            if (!IsVisible) return;
            _activeTab = MenuTab.Character;

            _mainPanel?.RemoveFromRoot();
            _mainPanel = null;
            _chosenSlotIndex = null;
            _hoveredSlotIndex = null;

            _selectedItem = null;
            _hoveredItem = null;
            if (_itemContainer != null)
                foreach (var btn in _itemButtons) _itemContainer.RemoveChild(btn);
            
            _itemButtons.Clear();

            _itemContainer = null;
            _statsContainer = null;
            _itemDetailsContainer = null;

            // Skills tab cleanup
            _selectedAbility = null;
            _hoveredAbility = null;
            _lastSnapshot = null;
            _allAbilityButtons.Clear();

            _itemContainer = null;
            _statsContainer = null;
            _itemDetailsContainer = null;
            _goldLabel = null;

            _skillsListContainer = null;
            _abilityDetailsContainer = null;

            _totalUnitButtons = 0;
            _allUnitButtons.Clear();
            _unitsListContainer = null;
        }

        public void HandleInput()
        {
            // Future: handle mouse clicks or controller input for menu navigation
        }

        private void BuildUI()
        {
            _mainPanel = new ColoredRectangleRuntime();

            // ✅ Centered Window instead of full screen
            _mainPanel.X = 190; // (1280 - 900) / 2
            _mainPanel.Y = 125; // (800 - 550) / 2
            _mainPanel.Width = 900;
            _mainPanel.Height = 550;
            _mainPanel.Color = new Color(10, 10, 20, 230);
            _mainPanel.AddToRoot();

            // ── LEFT PANEL: Tabs ──────────────────────────────────────────
            var leftPanel = new ColoredRectangleRuntime();
            leftPanel.Width = 150;
            leftPanel.HeightUnits = DimensionUnitType.RelativeToParent;
            leftPanel.Height = 0; // 0 with RelativeToParent means 100%
            leftPanel.Color = new Color(20, 20, 30, 240);
            leftPanel.ChildrenLayout = ChildrenLayout.TopToBottomStack;
            leftPanel.StackSpacing = 4;
            _mainPanel.Children.Add(leftPanel);

            var header = new Label();
            header.Text = "MENU";
            header.Visual.X = 10;
            header.Visual.Width = -20;
            header.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
            header.Height = 40;
            leftPanel.Children.Add(header.Visual);

            CreateTabButton(leftPanel, MenuTab.Character, "Character");
            CreateTabButton(leftPanel, MenuTab.Set, "Set");
            CreateTabButton(leftPanel, MenuTab.Items, "Items");
            CreateTabButton(leftPanel, MenuTab.Skills, "Skills");
            CreateTabButton(leftPanel, MenuTab.Settings, "Settings");

            var exitBtn = new Button();
            exitBtn.Text = "Exit";
            exitBtn.Visual.Width = -20;
            exitBtn.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
            exitBtn.Height = 40;
            exitBtn.Click += (_, _) => _eventDispatcher.Dispatch(new ExitGame());
            leftPanel.Children.Add(exitBtn.Visual);

            // ── MIDDLE PANEL: Content ─────────────────────────────────────
            var middlePanel = new ColoredRectangleRuntime();
            middlePanel.X = 150;
            middlePanel.Width = 450; // Split remaining space
            middlePanel.HeightUnits = DimensionUnitType.RelativeToParent;
            middlePanel.Height = 0;
            middlePanel.Color = new Color(30, 30, 40, 200);
            _mainPanel.Children.Add(middlePanel);

            // ── RIGHT PANEL: Details ──────────────────────────────────────
            var rightPanel = new ColoredRectangleRuntime();
            rightPanel.X = 600;
            rightPanel.Width = 300;
            rightPanel.HeightUnits = DimensionUnitType.RelativeToParent;
            rightPanel.Height = 0;
            rightPanel.Color = new Color(25, 25, 35, 200);
            _mainPanel.Children.Add(rightPanel);

            CreateTabContent(middlePanel, rightPanel);
            UpdateTabStyles();
        }

        private void CreateTabButton(GraphicalUiElement parent, MenuTab tab, string label)
        {
            var btn = new Button();
            btn.Text = label;
            btn.Visual.Width = -20;
            btn.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
            btn.Visual.Height = 40;
            btn.Click += (_, _) => 
            { 
                _activeTab = tab; 
                _chosenSlotIndex = null;

                _selectedItem = null;
                _itemDetailsUI.ClearDetails();

                UpdateTabStyles(); 
                ShowActiveTabContent(); 
            };
            parent.Children.Add(btn.Visual);
            _tabButtons[tab] = btn;
        }


        private void CreateTabContent(GraphicalUiElement middleParent, GraphicalUiElement rightParent)
        {
            // 1. Setup Right Panel Containers (to toggle between Stats and Item Details)
            _statsContainer = new ColoredRectangleRuntime();
            _statsContainer.Color = Color.Transparent;
            _statsContainer.WidthUnits = DimensionUnitType.RelativeToParent;
            _statsContainer.Width = 0;
            _statsContainer.HeightUnits = DimensionUnitType.RelativeToParent;
            _statsContainer.Height = 0;
            rightParent.Children.Add(_statsContainer);
            _statsUI.CreatStatsUI(_statsContainer);

            _itemDetailsContainer = new ColoredRectangleRuntime();
            _itemDetailsContainer.Color = Color.Transparent;
            _itemDetailsContainer.WidthUnits = DimensionUnitType.RelativeToParent;
            _itemDetailsContainer.Width = 0;
            _itemDetailsContainer.HeightUnits = DimensionUnitType.RelativeToParent;
            _itemDetailsContainer.Height = 0;
            _itemDetailsContainer.Visible = false; // Hidden by default
            rightParent.Children.Add(_itemDetailsContainer);
            _itemDetailsUI.CreateItemDetailsUI(_itemDetailsContainer);

            _abilityDetailsContainer = new ColoredRectangleRuntime();
            _abilityDetailsContainer.Color = Color.Transparent;
            _abilityDetailsContainer.WidthUnits = DimensionUnitType.RelativeToParent;
            _abilityDetailsContainer.Width = 0;
            _abilityDetailsContainer.HeightUnits = DimensionUnitType.RelativeToParent;
            _abilityDetailsContainer.Height = 0;
            _abilityDetailsContainer.Visible = false; // Hidden by default
            rightParent.Children.Add(_abilityDetailsContainer);
            _abilityDetailsUI.CreateAbilityDetailsUI(_abilityDetailsContainer);



            foreach (MenuTab tab in Enum.GetValues(typeof(MenuTab)))
            {
                var content = new ColoredRectangleRuntime();
                content.WidthUnits = DimensionUnitType.RelativeToParent;
                content.Width = 0;
                content.HeightUnits = DimensionUnitType.RelativeToParent;
                content.Height = 0;
                content.Color = Color.Transparent;
                content.Visible = false;
                middleParent.Children.Add(content);

                var placeholder = new Label();
                placeholder.Text = $"[{tab}] Content Area";
                placeholder.Visual.X = 10;
                placeholder.Visual.Y = 10;
                content.Children.Add(placeholder.Visual);

                if (tab == MenuTab.Set)
                {
                    content.ChildrenLayout = ChildrenLayout.TopToBottomStack;
                    content.StackSpacing = 10;
                    placeholder.Text = "Party Slots:";

                    // ✅ Create 3x3 Grid using 3 rows of LeftToRightStacks
                    for (int row = 0; row < 3; row++)
                    {
                        var rowContainer = new ColoredRectangleRuntime();
                        rowContainer.Color = Color.Transparent;
                        rowContainer.Width = -20; // 100% width minus 20px padding
                        rowContainer.WidthUnits = DimensionUnitType.RelativeToParent;
                        rowContainer.Height = 60;
                        rowContainer.ChildrenLayout = ChildrenLayout.LeftToRightStack;
                        rowContainer.StackSpacing = 10;
                        content.Children.Add(rowContainer);

                        for (int col = 0; col < 3; col++)
                        {
                            int slotIndex = row * 3 + col + 1;

                            var slotBtn = new Button();
                            slotBtn.Text = $"Slot {slotIndex}: Empty";

                            // Ratio width forces all 3 buttons to split the row equally
                            slotBtn.Visual.WidthUnits = DimensionUnitType.Ratio;
                            slotBtn.Visual.Width = 1;
                            slotBtn.Visual.Height = 0;
                            slotBtn.Visual.HeightUnits = DimensionUnitType.RelativeToParent;

                            int capturedSlot = slotIndex;
                            slotBtn.Click += (_, _) =>
                            {
                                if (_chosenSlotIndex == null) 
                                { 
                                    _chosenSlotIndex = capturedSlot; 
                                } 
                                else if (_chosenSlotIndex != capturedSlot)
                                {
                                    _commandDispatcher.Dispatch(new SwapPartySlotsCommand(_chosenSlotIndex.Value, capturedSlot));
                                    _chosenSlotIndex = null;
                                }

                            };

                            if (slotBtn.Visual is InteractiveGue interactive)
                            {
                                interactive.RollOn += (_, _) => _hoveredSlotIndex = capturedSlot;
                                interactive.RollOff += (_, _) =>
                                {
                                    if (_hoveredSlotIndex == capturedSlot) _hoveredSlotIndex = null;
                                };
                            }

                            rowContainer.Children.Add(slotBtn.Visual);
                            _slotButtons[slotIndex] = slotBtn;
                        }
                    }
                }
                else if (tab == MenuTab.Items)
                {
                    // ✅ Use Regular layout so we can manually position elements to prevent overflow
                    content.ChildrenLayout = ChildrenLayout.Regular;
                    placeholder.Text = "Inventory:";
                    placeholder.Visual.X = 10;
                    placeholder.Visual.Y = 10;

                    _goldLabel = new Label();
                    _goldLabel.Text = "Gold: 0";
                    _goldLabel.Visual.X = 10;
                    _goldLabel.Visual.Y = 40; // Position below placeholder
                    _goldLabel.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
                    _goldLabel.Visual.Width = -20;
                    _goldLabel.Visual.Height = 30;
                    content.Children.Add(_goldLabel.Visual);

                    _itemContainer = new ScrollViewer();
                    _itemContainer.Visual.X = 10;
                    _itemContainer.Visual.Y = 80; // Position below Gold label
                    _itemContainer.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
                    _itemContainer.Visual.Width = -20; // Padding
                    _itemContainer.Visual.HeightUnits = DimensionUnitType.RelativeToParent;
                    _itemContainer.Visual.Height = -90; // ✅ Subtract height to prevent overflow!

                    _itemContainer.InnerPanel.ChildrenLayout = ChildrenLayout.TopToBottomStack;
                    _itemContainer.InnerPanel.StackSpacing = 5;

                    content.Children.Add(_itemContainer.Visual);

                    _unitsListContainer = new ScrollViewer();
                    _unitsListContainer.Visual.X = 10;
                    _unitsListContainer.Visual.Y = 80;
                    _unitsListContainer.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
                    _unitsListContainer.Visual.Width = -20;
                    _unitsListContainer.Visual.HeightUnits = DimensionUnitType.RelativeToParent;
                    _unitsListContainer.Visual.Height = -90;

                    _unitsListContainer.InnerPanel.ChildrenLayout = ChildrenLayout.TopToBottomStack;
                    _unitsListContainer.InnerPanel.StackSpacing = 5;

                    content.Children.Add(_unitsListContainer.Visual);
                }
                else if (tab == MenuTab.Skills)
                {
                    // ✅ Use Regular layout
                    content.ChildrenLayout = ChildrenLayout.Regular;
                    placeholder.Text = "Skills:";
                    placeholder.Visual.X = 10;
                    placeholder.Visual.Y = 10;

                    _skillsListContainer = new ScrollViewer();
                    _skillsListContainer.Visual.X = 10;
                    _skillsListContainer.Visual.Y = 40; // Position below placeholder
                    _skillsListContainer.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
                    _skillsListContainer.Visual.Width = -20; // Padding
                    _skillsListContainer.Visual.HeightUnits = DimensionUnitType.RelativeToParent;
                    _skillsListContainer.Visual.Height = -50; // ✅ Subtract height to prevent overflow!

                    _skillsListContainer.InnerPanel.ChildrenLayout = ChildrenLayout.TopToBottomStack;
                    _skillsListContainer.InnerPanel.StackSpacing = 5;

                    content.Children.Add(_skillsListContainer.Visual);
                }

                _tabContents[tab] = content;
            }

            ShowActiveTabContent();
        }

        private void ShowActiveTabContent()
        {
            foreach (var kvp in _tabContents) kvp.Value.Visible = (kvp.Key == _activeTab);

            if (_statsContainer != null)
                _statsContainer.Visible = (_activeTab == MenuTab.Set);

            if (_itemDetailsContainer != null)
                _itemDetailsContainer.Visible = (_activeTab == MenuTab.Items);

            if (_abilityDetailsContainer != null)
                _abilityDetailsContainer.Visible = (_activeTab == MenuTab.Skills);

            if (_activeTab == MenuTab.Skills)
            {
                _selectedAbility = null;
                _abilityDetailsUI.ClearDetails();
            }
        }
        private void UpdateTabStyles()
        {
            foreach (var kvp in _tabButtons) kvp.Value.IsEnabled = (kvp.Key != _activeTab);
        }
    }
}