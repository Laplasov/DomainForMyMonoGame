using Gum.Wireframe;
using Microsoft.Xna.Framework;
using MonoGame_Game_Library.Input;
using UnceasingFear.Application.Combat;
using UnceasingFear.Application.Commands;

namespace UnceasingFear.Presentation.Input
{
    public class SlotInputHandler
    {
        private readonly Dictionary<Rectangle, int> _rectToSlotIndex = new();
        private readonly Dictionary<GraphicalUiElement, int> _gueToSlotIndex = new();
        private readonly ICommandDispatcher _commandDispatcher;

        private int? _pendingAbilitySlot;

        public bool IsAwaitingTarget => _pendingAbilitySlot.HasValue;
        public int? HoveredSlotIndex { get; private set; } // Replaces HoveredRect

        public SlotInputHandler(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        public void Register(Rectangle rect, int slotIndex) => _rectToSlotIndex[rect] = slotIndex;
        public void Register(GraphicalUiElement gue, int slotIndex) => _gueToSlotIndex[gue] = slotIndex;

        public void Clear()
        {
            _rectToSlotIndex.Clear();
            _gueToSlotIndex.Clear();
            _pendingAbilitySlot = null;
            HoveredSlotIndex = null;
        }

        public void AwaitTarget(int abilitySlot) => _pendingAbilitySlot = abilitySlot;

        public void Cancel()
        {
            _pendingAbilitySlot = null;
            HoveredSlotIndex = null;
        }

        public void Update(MouseInfo mouse)
        {
            var pos = mouse.Position;
            HoveredSlotIndex = null;

            // 1. Check static battle field rects
            foreach (var kvp in _rectToSlotIndex)
            {
                if (kvp.Key.Contains(pos))
                {
                    HoveredSlotIndex = kvp.Value;
                    break;
                }
            }

            // 2. If not hovering a field rect, check dynamic Gum elements
            if (HoveredSlotIndex == null)
            {
                foreach (var kvp in _gueToSlotIndex)
                {
                    var gue = kvp.Key;

                    // Skip hidden bars (empty slots)!
                    if (!gue.Visible) continue;

                    // Get the actual dynamic screen bounds from Gum
                    var rect = new Rectangle(
                        (int)gue.AbsoluteX,
                        (int)gue.AbsoluteY,
                        (int)gue.Width,
                        (int)gue.Height);

                    if (rect.Contains(pos))
                    {
                        HoveredSlotIndex = kvp.Value;
                        break;
                    }
                }
            }

            if (!_pendingAbilitySlot.HasValue) return;

            if (mouse.WasButtonJustPressed(MouseButton.Right))
            {
                Cancel();
                return;
            }

            if (!mouse.WasButtonJustPressed(MouseButton.Left)) return;
            if (HoveredSlotIndex == null) return;

            _commandDispatcher.Dispatch(
                new SelectAbilityEventCommand(HoveredSlotIndex.Value, _pendingAbilitySlot.Value));

            _pendingAbilitySlot = null;
            HoveredSlotIndex = null;
        }
    }
}