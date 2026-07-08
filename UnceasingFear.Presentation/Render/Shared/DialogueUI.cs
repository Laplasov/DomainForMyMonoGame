using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Managers;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using System;
using System.Collections.Generic;
using System.Linq;
using UnceasingFear.Application.Commands;
using UnceasingFear.Application.World;
using UnceasingFear.Domain.Shared.Events;
using UnceasingFear.Domain.Shared.ValueObjects;
using static UnceasingFear.Domain.Shared.Events.SharedEvents;

namespace UnceasingFear.Presentation.Render.Shared
{
    public class DialogueUI
    {
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ICommandDispatcher _commandDispatcher;

        private ColoredRectangleRuntime? _mainPanel;
        private Label? _speakerLabel;
        private Label? _textLabel;
        private ScrollViewer? _buttonsContainer;

        private readonly List<Button> _choiceButtons = new();

        // ✅ IsVisible now checks if the panel actually exists in the Gum root
        public bool IsVisible => _mainPanel != null && _mainPanel.Parent != null;

        // ✅ Flag to track if the UI is supposed to be on screen
        private bool _shouldBeVisible = false;

        // Cache the last known state
        private string _pendingSpeaker = string.Empty;
        private string _pendingText = string.Empty;
        private IReadOnlyList<DialogueChoice>? _pendingChoices;

        public DialogueUI(IEventDispatcher eventDispatcher, ICommandDispatcher commandDispatcher)
        {
            _eventDispatcher = eventDispatcher;
            _commandDispatcher = commandDispatcher;

            _eventDispatcher.Subscribe<DialogueStartedEvent>(OnDialogueStarted);
            _eventDispatcher.Subscribe<DialogueAdvancedEvent>(OnDialogueAdvanced);
            _eventDispatcher.Subscribe<DialogueEndEvent>(OnDialogueEnd);

            // Hide the dialogue when a nested battle starts
            _eventDispatcher.Subscribe<EnterBattleEvent>((_) =>
            {
                if (_shouldBeVisible) Hide();
            });
        }

        // ✅ Call this from WorldView.Draw to ensure the UI survives BattleView's root clearing
        public void EnsureRooted()
        {
            // If we should be visible, but the panel is missing or was destroyed by BattleView
            if (_shouldBeVisible && (_mainPanel == null || _mainPanel.Parent == null))
            {
                BuildUI();
                if (_pendingChoices != null)
                {
                    UpdateContent(_pendingSpeaker, _pendingText, _pendingChoices);
                }
            }
        }

        private void OnDialogueStarted(DialogueStartedEvent e)
        {
            _pendingSpeaker = e.Speaker;
            _pendingText = e.Text;
            _pendingChoices = e.Choices;
            _shouldBeVisible = true;

            BuildUI();
            UpdateContent(e.Speaker, e.Text, e.Choices);
        }

        private void OnDialogueAdvanced(DialogueAdvancedEvent e)
        {
            _pendingSpeaker = e.Speaker;
            _pendingText = e.Text;
            _pendingChoices = e.Choices;
            _shouldBeVisible = true;

            // Try to show immediately. If BattleView destroys it later this frame, 
            // EnsureRooted() will fix it on the next frame.
            if (_mainPanel == null || _mainPanel.Parent == null)
            {
                BuildUI();
            }

            UpdateContent(e.Speaker, e.Text, e.Choices);
        }

        private void OnDialogueEnd(DialogueEndEvent e)
        {
            _shouldBeVisible = false;
            Hide();
        }

        public void Hide()
        {
            _shouldBeVisible = false;
            if (_mainPanel == null) return;

            _mainPanel.RemoveFromRoot();
            _mainPanel = null;
            _choiceButtons.Clear();
            _speakerLabel = null;
            _textLabel = null;
            _buttonsContainer = null;
        }

        private void BuildUI()
        {
            _mainPanel = new ColoredRectangleRuntime();

            _mainPanel.X = 0;
            _mainPanel.Y = 600;
            _mainPanel.Width = 1280;
            _mainPanel.Height = 200;
            _mainPanel.Color = new Color(10, 10, 20, 240);
            _mainPanel.AddToRoot();

            _buttonsContainer = new ScrollViewer();
            _buttonsContainer.Visual.X = 20;
            _buttonsContainer.Visual.Width = 400;
            _buttonsContainer.Visual.HeightUnits = DimensionUnitType.RelativeToParent;
            _buttonsContainer.Visual.Height = -40;
            _buttonsContainer.Visual.Y = 20;

            _buttonsContainer.InnerPanel.ChildrenLayout = ChildrenLayout.TopToBottomStack;
            _buttonsContainer.InnerPanel.StackSpacing = 5;
            _buttonsContainer.InnerPanel.WidthUnits = DimensionUnitType.RelativeToParent;
            _buttonsContainer.InnerPanel.Width = 0;

            _mainPanel.Children.Add(_buttonsContainer.Visual);

            var rightPanel = new ColoredRectangleRuntime();
            rightPanel.X = 440;
            rightPanel.Width = 820;
            rightPanel.HeightUnits = DimensionUnitType.RelativeToParent;
            rightPanel.Height = -40;
            rightPanel.Y = 20;
            rightPanel.Color = Color.Transparent;
            rightPanel.ChildrenLayout = ChildrenLayout.TopToBottomStack;
            rightPanel.StackSpacing = 10;
            _mainPanel.Children.Add(rightPanel);

            _speakerLabel = new Label();
            _speakerLabel.Text = "Speaker";
            _speakerLabel.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
            _speakerLabel.Visual.Width = 0;
            _speakerLabel.Height = 40;
            rightPanel.Children.Add(_speakerLabel.Visual);

            _textLabel = new Label();
            _textLabel.Text = "Dialogue text...";
            _textLabel.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
            _textLabel.Visual.Width = 0;
            _textLabel.Height = 0;
            _textLabel.Visual.HeightUnits = DimensionUnitType.RelativeToParent;
            rightPanel.Children.Add(_textLabel.Visual);
        }

        private void UpdateContent(string speaker, string text, IReadOnlyList<DialogueChoice> choices)
        {
            if (_speakerLabel != null) _speakerLabel.Text = speaker;
            if (_textLabel != null) _textLabel.Text = text;

            if (_buttonsContainer != null)
            {
                foreach (var btn in _choiceButtons)
                {
                    _buttonsContainer.InnerPanel.Children.Remove(btn.Visual);
                }
            }
            _choiceButtons.Clear();

            foreach (var choice in choices)
            {
                var btn = new Button();
                btn.Text = choice.Text;

                btn.Visual.Height = 30;
                btn.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
                btn.Visual.Width = 0;

                var capturedChoice = choice;
                btn.Click += (_, _) =>
                {
                    _commandDispatcher.Dispatch(new AdvanceDialogueCommand(capturedChoice));
                };

                _buttonsContainer?.InnerPanel.Children.Add(btn.Visual);
                _choiceButtons.Add(btn);
            }
        }
    }
}