using Gum.Forms.Controls;

namespace UnceasingFear.Presentation.Render.Battle
{
    /// <summary>
    /// Plain bag of references to every Gum widget that needs updating each frame.
    /// Built once by BattleHudBuilder, read by BattleHudUpdater.
    /// </summary>
    public class BattleHudHandles
    {
        public required Label ActiveUnitName { get; init; }
        public required Label StatHp { get; init; }
        public required Label StatSp { get; init; }
        public required Label StatAtk { get; init; }
        public required Label StatDef { get; init; }
        public required Label StatMag { get; init; }
        public required Label StatSpd { get; init; }
        public required Button[] AbilityButtons { get; init; }  // length 4
        public required Label LogPlaceholder { get; init; }

    }
}