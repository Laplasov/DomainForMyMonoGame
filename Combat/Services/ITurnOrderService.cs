using System;
using System.Collections.Generic;
using System.Text;
using UnceasingFear.Domain.Combat.Entities;

namespace UnceasingFear.Domain.Combat.Services
{
    public interface ITurnOrderService
    {
        /// <summary>
        /// Advances all units' gauges by base amount modified by their Speed
        /// </summary>
        void AdvanceGauges(IEnumerable<Unit> units, float deltaTime);

        /// <summary>
        /// </summary>
        IEnumerable<Unit> GetReadyUnitsInOrder(IEnumerable<Unit> units);
    }
}
