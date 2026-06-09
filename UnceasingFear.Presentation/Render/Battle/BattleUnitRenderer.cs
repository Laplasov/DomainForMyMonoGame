using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UnceasingFear.Application.Combat.Snapshots;
using UnceasingFear.Presentation.Input;

namespace UnceasingFear.Presentation.Render.Battle
{
    /// <summary>
    /// Draws unit slots and per-unit bars via SpriteBatch.
    /// No Gum, no layout math — both come in from outside.
    /// </summary>
    public class BattleUnitRenderer
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly Texture2D _pixel;
        private readonly BattleLayout _layout;
        private readonly SlotInputHandler _slotInput;

        // ── Slot tint colors ─────────────────────────────────────────────────
        private static readonly Color AllySlotBg = Color.DarkGreen * 0.4f;
        private static readonly Color EnemySlotBg = Color.DarkRed * 0.4f;
        private static readonly Color AllyUnitColor = Color.LimeGreen;
        private static readonly Color EnemyUnitColor = Color.OrangeRed;
        private static readonly Color DeadUnitColor = Color.Gray;
        private static readonly Color ActiveOutline = Color.Gold;

        // ── Bar colors ───────────────────────────────────────────────────────
        private static readonly Color HpBarColor = Color.Red;
        private static readonly Color SpBarColor = Color.CornflowerBlue;
        private static readonly Color TurnBarColor = Color.Yellow;
        private static readonly Color BarBackground = Color.White * 0.1f;

        public BattleUnitRenderer(SpriteBatch spriteBatch, Texture2D pixel, BattleLayout layout, SlotInputHandler slotInput)
        {
            _spriteBatch = spriteBatch;
            _pixel = pixel;
            _layout = layout;
            _slotInput = slotInput;
        }

        public void Draw(BattleSnapshot snapshot)
        {
            DrawSlotBackgrounds();
            DrawUnits(snapshot);
        }

        // ── Slot backgrounds ─────────────────────────────────────────────────

        private void DrawSlotBackgrounds()
        {
            for (int i = 0; i < 6; i++)
            {
                _spriteBatch.Draw(_pixel, _layout.AllySlotRects[i], AllySlotBg);
                _spriteBatch.Draw(_pixel, _layout.EnemySlotRects[i], EnemySlotBg);
            }
        }

        // ── Units ────────────────────────────────────────────────────────────

        private void DrawUnits(BattleSnapshot snapshot)
        {
            foreach (var unit in snapshot.Units)
            {
                int arrayIndex = unit.SlotIndex - 1;
                if (arrayIndex < 0 || arrayIndex > 5) continue;

                var slotRect = unit.IsAlly
                    ? _layout.AllySlotRects[arrayIndex]
                    : _layout.EnemySlotRects[arrayIndex];

                var unitRect = BattleLayout.UnitRect(slotRect);

                DrawUnitBody(unit, unitRect, snapshot.CurrentActorId);
                DrawBars(unit, unitRect);
            }
        }

        private void DrawUnitBody(UnitSnapshot unit, Rectangle unitRect, Guid? currentActorId)
        {
            // Active outline
            if (unit.Id == currentActorId)
            {
                var outline = new Rectangle(
                    unitRect.X - 2, unitRect.Y - 2,
                    unitRect.Width + 4, unitRect.Height + 4);
                _spriteBatch.Draw(_pixel, outline, ActiveOutline * 0.6f);
            }

            // Hover highlight (Works for both field slots AND Gum bars!)
            if (_slotInput.IsAwaitingTarget && _slotInput.HoveredSlotIndex == unit.SlotIndex)
            {
                var hover = new Rectangle(unitRect.X - 1, unitRect.Y - 1,
                    unitRect.Width + 2, unitRect.Height + 2);
                _spriteBatch.Draw(_pixel, hover, Color.White * 0.4f);
            }

            // Unit fill
            var color = !unit.IsAlive
                ? DeadUnitColor
                : unit.IsAlly ? AllyUnitColor : EnemyUnitColor;

            _spriteBatch.Draw(_pixel, unitRect, color);
        }

        private void DrawBars(UnitSnapshot unit, Rectangle unitRect)
        {
            if (!unit.IsAlive) return;

            float hp = unit.MaxHp > 0 ? (float)unit.CurrentHp / unit.MaxHp : 0f;
            float sp = unit.MaxSp > 0 ? (float)unit.CurrentSp / unit.MaxSp : 0f;
            float gauge = Math.Clamp(unit.TurnProgress / 100f, 0f, 1f);

            DrawBar(unitRect, barIndex: 0, fillPercent: 1f, BarBackground);
            DrawBar(unitRect, barIndex: 0, fillPercent: hp, HpBarColor);

            DrawBar(unitRect, barIndex: 1, fillPercent: 1f, BarBackground);
            DrawBar(unitRect, barIndex: 1, fillPercent: sp, SpBarColor);

            DrawBar(unitRect, barIndex: 2, fillPercent: 1f, BarBackground);
            DrawBar(unitRect, barIndex: 2, fillPercent: gauge, TurnBarColor);
        }

        private void DrawBar(Rectangle unitRect, int barIndex, float fillPercent, Color color)
        {
            var rect = BattleLayout.BarRect(unitRect, barIndex, fillPercent);
            if (rect.Width > 0)
                _spriteBatch.Draw(_pixel, rect, color);
        }
    }
}