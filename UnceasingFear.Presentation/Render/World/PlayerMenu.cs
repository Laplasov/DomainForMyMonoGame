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
using UnceasingFear.Presentation.Render.World;
using static UnceasingFear.Domain.Shared.Events.SharedEvents;

namespace UnceasingFear.Presentation.Render
{
    public enum MenuTab { Character, Set, Items, Skills, Settings }

    public class PlayerMenu
    {
        private ColoredRectangleRuntime? _mainPanel;
        private MenuTab _activeTab = MenuTab.Character;
        private readonly Dictionary<MenuTab, Button> _tabButtons = new();
        private readonly Dictionary<MenuTab, ColoredRectangleRuntime> _tabContents = new();

        private readonly IEventDispatcher _eventDispatcher;
        private readonly ICommandDispatcher _commandDispatcher;

        private readonly Dictionary<int, Button> _slotButtons = new();

        private int? _chosenSlotIndex = null;
        private int? _hoveredSlotIndex = null;

        private readonly UnitsStatsUI _statsUI = new();

        public PlayerMenu(IEventDispatcher eventDispatcher, ICommandDispatcher commandDispatcher)
        {
            _eventDispatcher = eventDispatcher;
            _commandDispatcher = commandDispatcher;
        }

        public bool IsVisible => _mainPanel != null;
        public void Update(WorldSnapshot snapshot)
        {
            RefreshSetTab(snapshot);
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
                else if (_chosenSlotIndex != null)
                {
                    _statsUI.ClearStats();
                }
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
            _mainPanel?.RemoveFromRoot();
            _mainPanel = null;
            _chosenSlotIndex = null;
            _hoveredSlotIndex = null;
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
                UpdateTabStyles(); 
                ShowActiveTabContent(); 
            };
            parent.Children.Add(btn.Visual);
            _tabButtons[tab] = btn;
        }

        private void CreateTabContent(GraphicalUiElement middleParent, GraphicalUiElement rightParent)
        {
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
                _tabContents[tab] = content;
            }
            _statsUI.CreatStatsUI(rightParent);

            ShowActiveTabContent();
        }

        private void ShowActiveTabContent()
        {
            foreach (var kvp in _tabContents) kvp.Value.Visible = (kvp.Key == _activeTab);
        }

        private void UpdateTabStyles()
        {
            foreach (var kvp in _tabButtons) kvp.Value.IsEnabled = (kvp.Key != _activeTab);
        }
    }
}