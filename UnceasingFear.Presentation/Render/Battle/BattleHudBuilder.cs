using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Managers;
using Gum.Wireframe;
using GumRuntime;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using System.Timers;
using UnceasingFear.Application.Combat;
using UnceasingFear.Application.Commands;
using UnceasingFear.Presentation.Input;
using static UnceasingFear.Presentation.Render.Battle.BattleHudHandles;

namespace UnceasingFear.Presentation.Render.Battle
{
    /// <summary>
    /// Builds the Gum widget tree once and returns BattleHudHandles.
    ///
    /// Each panel is a ColoredRectangleRuntime that acts as a real parent container.
    /// Children are added via panel.Children.Add() so Gum's layout system handles
    /// all sizing and clipping — no manual pixel math for child positions.
    /// </summary>
    public class BattleHudBuilder
    {
        private readonly GumService _gum;
        private readonly BattleLayout _layout;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly SlotInputHandler _slotInput;

        private static readonly Color PanelBg = new Color(30, 30, 40, 210);

        public BattleHudBuilder(
            GumService gum,
            BattleLayout layout,
            ICommandDispatcher commandDispatcher,
            SlotInputHandler slotInput)
        {
            _gum = gum;
            _layout = layout;
            _commandDispatcher = commandDispatcher;
            _slotInput = slotInput;
        }

        public BattleHudHandles Build()
        {
            var barBuilder = new UnitBarBuilder(_layout);
            var handles = new BattleHudHandles
            {
                ActiveUnitName = BuildStatsPanel(out var hp, out var sp, out var atk, out var def, out var mag, out var spd, out var identity),
                Identity = identity,
                StatHp = hp,
                StatSp = sp,
                StatAtk = atk,
                StatDef = def,
                StatMag = mag,
                StatSpd = spd,
                AbilityButtons = BuildAbilityPanel(),
                LogPlaceholder = BuildLogPanel(),
                AllyBars = barBuilder.BuildSide(isAlly: true),
                EnemyBars = barBuilder.BuildSide(isAlly: false),
                ActionButtons = BuildActionButtons(),
            };

            return handles;
        }
        // ── Buttons ──────────────────────────────────────────────────────
        private Button[] BuildActionButtons()
        {
            var buttons = new Button[4];
            string[] labels = { "Pass", "Run", "Items", "Talk" };

            for (int i = 0; i < 4; i++)
            {
                var r = _layout.ActionButtonsRects[i];
                var btn = new Button();
                btn.Text = labels[i];

                btn.Visual.X = r.X;
                btn.Visual.Y = r.Y;
                btn.Visual.Width = r.Width;
                btn.Visual.Height = r.Height;

                btn.Visual.AddToRoot();
                btn.IsEnabled = false; // Start disabled until player's turn

                int index = i;
                btn.Click += (_, _) =>
                {
                    switch (index)
                    {
                        case 0: 
                            _commandDispatcher.Dispatch(new PassTurnCommand());
                            _slotInput.Cancel();
                            break;
                        case 1: 
                            _commandDispatcher.Dispatch(new RunFromBattleCommand());
                            _slotInput.Cancel();
                            break;
                        case 2: /* Items: Do nothing for now */ break;
                        case 3: /* Talk: Do nothing for now */ break;
                    }
                };

                buttons[i] = btn;
            }

            return buttons;
        }


        // ── Stats panel ──────────────────────────────────────────────────────
        private Label BuildStatsPanel(
            out Label hp, out Label sp, out Label atk,
            out Label def, out Label mag, out Label spd,
            out Label identity)
        {
            var panel = MakePanel(_layout.StatsPanel);

            // Vertical stack inside the panel
            panel.ChildrenLayout = ChildrenLayout.TopToBottomStack;
            panel.StackSpacing = 4;

            var name = AddChildLabel("—", panel);
            identity = AddChildLabel("Identity:  —", panel);
            hp = AddChildLabel("HP:  —", panel);
            sp = AddChildLabel("SP:  —", panel);
            atk = AddChildLabel("PHY: —", panel);
            def = AddChildLabel("DEF: —", panel);
            mag = AddChildLabel("MAG: —", panel);
            spd = AddChildLabel("SPD: —", panel);

            return name;
        }

        // ── Ability panel ────────────────────────────────────────────────────

        private Button[] BuildAbilityPanel()
        {
            var r = _layout.AbilityPanel;
            var panel = MakePanel(r);

            // Header label — fixed height, full width
            var header = new Label();
            header.Text = "Abilities";
            header.Visual.X = 10;
            header.Visual.Width = -20;
            header.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
            header.Height = 24;
            panel.Children.Add(header.Visual);

            // Button row container — fills remaining height, stacks children left-to-right
            var row = new ColoredRectangleRuntime();
            row.Color = Color.Transparent;
            row.X = 10;
            row.Y = 28;                                     // below header
            row.Width = -20;
            row.WidthUnits = DimensionUnitType.RelativeToParent;
            row.Height = r.Height - 28 - 8;                     // remaining height minus padding
            row.ChildrenLayout = ChildrenLayout.LeftToRightStack;
            row.StackSpacing = 6;
            panel.Children.Add(row);

            var buttons = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                int slot = i;
                var btn = new Button();
                btn.Text = "—";

                // Each button gets an equal ratio share of the row width
                btn.Visual.WidthUnits = DimensionUnitType.Ratio;
                btn.Visual.Width = 1;

                btn.Visual.Height = 0;
                btn.Visual.HeightUnits = DimensionUnitType.RelativeToParent;

                btn.IsEnabled = false;
                btn.Click += (_, _) =>  _slotInput.AwaitTarget(slot);

                row.Children.Add(btn.Visual);
                buttons[i] = btn;
            }

            return buttons;
        }

        // ── Log panel ────────────────────────────────────────────────────────

        private Label BuildLogPanel()
        {
            var panel = MakePanel(_layout.LogPanel);
            panel.ChildrenLayout = ChildrenLayout.TopToBottomStack;
            panel.StackSpacing = 4;

            AddChildLabel("Battle Log", panel);
            return AddChildLabel("...", panel);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private ColoredRectangleRuntime MakePanel(Microsoft.Xna.Framework.Rectangle r)
        {
            var panel = new ColoredRectangleRuntime();
            panel.X = r.X;
            panel.Y = r.Y;
            panel.Width = r.Width;
            panel.Height = r.Height;
            panel.Color = PanelBg;
            panel.AddToRoot();
            return panel;
        }

        private static Label AddChildLabel(string text, GraphicalUiElement parent)
        {
            var label = new Label();
            label.Text = text;
            label.Visual.X = 10;
            label.Visual.Width = -20;
            label.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
            parent.Children.Add(label.Visual);
            return label;
        }
    }
}