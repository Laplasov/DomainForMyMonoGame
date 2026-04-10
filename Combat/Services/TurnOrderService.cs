using System;
using System.Collections.Generic;
using System.Text;
using UnceasingFear.Domain.Combat.Entities;

namespace UnceasingFear.Domain.Combat.Services
{
    public class TurnOrderService : ITurnOrderService
    {
        public void AdvanceGauges(IEnumerable<Unit> units, float deltaTime)
        {
            foreach (var unit in units)
            {
                var speedModifier = unit.Profile.Stats.Speed; 
                unit.AdvanceGauge(deltaTime, speedModifier);
            }
        }

        public IEnumerable<Unit> GetReadyUnitsInOrder(IEnumerable<Unit> units)
            => units
                .Where(u => u.CanAct)
                .OrderByDescending(u => u.Profile.Stats.Speed)
                .ThenByDescending(u => u.TurnProgress.Value);
    }
}
