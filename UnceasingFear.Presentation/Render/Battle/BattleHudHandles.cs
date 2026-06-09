using Gum.Forms.Controls;
using Gum.Wireframe;
using MonoGameGum.GueDeriving;

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
        public required UnitBarHandles[] AllyBars { get; init; }   // length 6
        public required UnitBarHandles[] EnemyBars { get; init; }
        public required Button[] ActionButtons { get; init; }

    }

    public record UnitBarHandles(
       ColoredRectangleRuntime Container,
       Label NameLabel,
       ColoredRectangleRuntime HpBar,
       ColoredRectangleRuntime SpBar,
       ColoredRectangleRuntime TurnBar
   );
}