using System;
using System.Collections.Generic;
using System.Text;
using UnceasingFear.Domain.Combat.Entities;
using UnceasingFear.Domain.Combat.ValueObjects;

namespace UnceasingFear.Domain.Combat.Services
{
    public interface IStateService
    {
        public BattleState OnNextUnit(BattleState currentState, Unit nextUnit, IEnumerable<Unit> allUnits);
    }
}
