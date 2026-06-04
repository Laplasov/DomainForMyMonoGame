using Microsoft.Xna.Framework;
using MonoGame_Game_Library.Input;
using System.Diagnostics;
using UnceasingFear.Application.Combat;
using UnceasingFear.Application.Commands;
using UnceasingFear.Presentation.Render.Battle;

namespace UnceasingFear.Presentation.Input
{
    public class SlotInputHandler
    {
        private readonly List<Rectangle> _rects = new();
        private readonly ICommandDispatcher _commandDispatcher;

        private int? _pendingAbilitySlot;

        public bool IsAwaitingTarget => _pendingAbilitySlot.HasValue;
        public Rectangle? HoveredRect { get; private set; }

        public SlotInputHandler(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        public void Register(Rectangle rect) => _rects.Add(rect);

        public void Register(IEnumerable<Rectangle> rects)
        {
            foreach (var r in rects) _rects.Add(r);
        }

        public void Clear()
        {
            _rects.Clear();
            _pendingAbilitySlot = null;
            HoveredRect = null;
        }

        /// <summary>
        /// Arms the handler. Next slot click will dispatch SelectAbilityEventCommand.
        /// </summary>
        public void AwaitTarget(int abilitySlot) => _pendingAbilitySlot = abilitySlot;

        public void Cancel()
        {
            _pendingAbilitySlot = null;
            HoveredRect = null;
        }

        /// <summary>
        /// Call once per Update. Tracks hover and consumes clicks when awaiting a target.
        /// </summary>
        public void Update(MouseInfo mouse, BattleLayout layout)
        {
            var pos = mouse.Position;

            HoveredRect = null;
            foreach (var rect in _rects)
            {
                if (rect.Contains(pos))
                {
                    HoveredRect = rect;
                    break;
                }
            }

            if (!_pendingAbilitySlot.HasValue) return;

            if (mouse.WasButtonJustPressed(MouseButton.Right))
            {
                Cancel();
                return;
            }

            if (!mouse.WasButtonJustPressed(MouseButton.Left)) return;
            if (HoveredRect is null) return;

            int slotIndex = layout.SlotIndexOf(HoveredRect.Value);
            if (slotIndex < 0) return;

            _commandDispatcher.Dispatch(
                new SelectAbilityEventCommand(slotIndex, _pendingAbilitySlot.Value));

            _pendingAbilitySlot = null;
            HoveredRect = null;
        }
    }
}