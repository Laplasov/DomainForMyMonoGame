using Microsoft.Xna.Framework;

namespace UnceasingFear.Presentation.Render.Battle
{
    /// <summary>
    /// Pure data class. Computes all screen regions from screen dimensions.
    /// No Gum, no SpriteBatch, no game logic.
    /// </summary>
    public class BattleLayout
    {
        public int ScreenWidth { get; }
        public int ScreenHeight { get; }

        // ── Slot grid ────────────────────────────────────────────────────────
        public Rectangle[] AllySlotRects { get; } = new Rectangle[6];
        public Rectangle[] EnemySlotRects { get; } = new Rectangle[6];
        public Rectangle[] ActionButtonsRects { get; } = new Rectangle[4];

        // ── Bottom panels ────────────────────────────────────────────────────
        public Rectangle StatsPanel { get; }
        public Rectangle AbilityPanel { get; }
        public Rectangle LogPanel { get; }

        // ── Shared constants ─────────────────────────────────────────────────
        public const int SlotSize = 64;
        public const int SlotSpacing = 10;
        public const int BarHeight = 3;
        public const int BarSpacing = 5;
        public const int BarOffsetY = 4;

        public BattleLayout(int screenWidth, int screenHeight)
        {
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;

            const int slotStartX = 40;
            const int fieldTopY = 40;
            const int panelPad = 8;

            // Bottom strip = 1/3 of screen height
            int bottomHeight = screenHeight / 3;
            int bottomY = screenHeight - bottomHeight - 10;

            // ── Ally slots: two rows of 3, sit just above the bottom strip ───
            int allyFrontY = bottomY - SlotSize - SlotSpacing;
            int allyBackY = allyFrontY - SlotSize - SlotSpacing;

            for (int i = 0; i < 6; i++)
            {
                int col = i % 3;
                int row = i / 3;
                AllySlotRects[i] = new Rectangle(
                    slotStartX + col * (SlotSize + SlotSpacing),
                    row == 0 ? allyFrontY : allyBackY,
                    SlotSize, SlotSize);
            }

            // ── Enemy slots: top-right, two rows of 3 ───────────────────────
            int enemyFrontY = fieldTopY;
            int enemyBackY = fieldTopY + SlotSize + SlotSpacing;

            for (int i = 0; i < 6; i++)
            {
                int col = i % 3;
                int row = i / 3;
                EnemySlotRects[i] = new Rectangle(
                    screenWidth - slotStartX - SlotSize - col * (SlotSize + SlotSpacing),
                    row == 0 ? enemyFrontY : enemyBackY,
                    SlotSize, SlotSize);
            }

            // ── Bottom strip: 25% stats | 50% abilities | 25% log ───────────
            int usableWidth = screenWidth - panelPad * 4; // 3 panels, 4 outer/inner gaps
            int statsWidth = (int)(usableWidth * 0.25f);
            int abilityWidth = (int)(usableWidth * 0.50f);
            int logWidth = usableWidth - statsWidth - abilityWidth;

            StatsPanel = new Rectangle(
                panelPad,
                bottomY,
                statsWidth,
                bottomHeight);

            AbilityPanel = new Rectangle(
                panelPad * 2 + statsWidth,
                bottomY,
                abilityWidth,
                bottomHeight);

            LogPanel = new Rectangle(
                panelPad * 3 + statsWidth + abilityWidth,
                bottomY,
                logWidth,
                bottomHeight);


            const int ActionBtnWidth = 80;
            const int ActionBtnHeight = 30;
            const int ActionBtnGap = 6;

            int totalActionW = 4 * ActionBtnWidth + 3 * ActionBtnGap;
            int actionStartX = AbilityPanel.X + (AbilityPanel.Width - totalActionW) / 2;
            int actionY = AbilityPanel.Y - ActionBtnHeight - 4; // 4px padding above abilities

            for (int i = 0; i < 4; i++)
            {
                ActionButtonsRects[i] = new Rectangle(
                    actionStartX + i * (ActionBtnWidth + ActionBtnGap),
                    actionY,
                    ActionBtnWidth,
                    ActionBtnHeight);
            }

        }

        /// <summary>Returns the inner rect for a unit inside its slot (inset by 8px).</summary>
        public static Rectangle UnitRect(Rectangle slot)
            => new Rectangle(slot.X + 8, slot.Y + 8, slot.Width - 16, slot.Height - 16);

        /// <summary>
        /// Returns the rect for one of the 4 ability buttons in a single row.
        /// Width and height are both derived from the panel dimensions.
        /// </summary>
        public Rectangle AbilityButtonRect(int index)
        {
            const int pad = 10;
            const int gap = 8;
            const int headerH = 30;

            int innerX = AbilityPanel.X + pad;
            int innerY = AbilityPanel.Y + pad + headerH;
            int totalW = AbilityPanel.Width - pad * 2;
            int totalH = AbilityPanel.Height - pad * 2 - headerH;
            int btnW = (totalW - gap * 3) / 4;

            return new Rectangle(
                innerX + index * (btnW + gap),
                innerY,
                btnW,
                totalH);
        }

        /// <summary>Returns the rect for one of the three bars below a unit rect.</summary>
        /// <param name="barIndex">0 = HP, 1 = SP, 2 = TurnGauge</param>
        public static Rectangle BarRect(Rectangle unitRect, int barIndex, float fillPercent)
        {
            int y = unitRect.Y + unitRect.Height + BarOffsetY + barIndex * BarSpacing;
            return new Rectangle(
                unitRect.X,
                y,
                (int)(unitRect.Width * Math.Clamp(fillPercent, 0f, 1f)),
                BarHeight);
        }

        /// <summary>
        /// Returns the 1-based slot index for the given rectangle, or -1 if not found.
        /// </summary>
        public int SlotIndexOf(Rectangle rect)
        {
            for (int i = 0; i < AllySlotRects.Length; i++)
                if (AllySlotRects[i] == rect || EnemySlotRects[i] == rect)
                    return i + 1;

            return -1;
        }
    }
}