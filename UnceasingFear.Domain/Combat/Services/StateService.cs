using System;
using System.Collections.Generic;
using System.Text;
using UnceasingFear.Domain.Combat.Entities;
using UnceasingFear.Domain.Combat.ValueObjects;

namespace UnceasingFear.Domain.Combat.Services
{
    public class StateService : IStateService
    {
        public BattleState OnNextUnit(BattleState currentState, Unit nextUnit, IEnumerable<Unit> allUnits)
        {
            BattleState phase = nextUnit.IsAlly ? new BattleState.PlayerTurn() : new BattleState.EnemyTurn();

            var enemyUnits = allUnits.Where(u => u.IsAlive && !u.IsAlly);
            var allyUnits = allUnits.Where(u => u.IsAlive && u.IsAlly);
            if (!enemyUnits.Any())
            {
                return new BattleState.Victory();
            }

            if (!allyUnits.Any())
            {
                return new BattleState.Lost();
            }
            return phase;
        }
    }
}
