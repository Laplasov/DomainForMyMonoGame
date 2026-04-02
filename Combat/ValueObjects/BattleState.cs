using System;
using System.Collections.Generic;
using System.Text;
using UnceasingFear.Domain.Combat.ValueObjects.Abilities;

namespace UnceasingFear.Domain.Combat.ValueObjects
{
    public abstract record BattleState
    {
        public sealed record Initializing() : BattleState;
        public sealed record PlayerTurn() : BattleState;
        public sealed record EnemyTurn() : BattleState;
        public sealed record Interlude() : BattleState;
        public sealed record ResulveBattle() : BattleState;
        public sealed record Victory() : BattleState;
        public sealed record Lost() : BattleState;
    }
}
