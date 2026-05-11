using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Application.Commands;
using UnceasingFear.Domain.Combat.Aggregates;
using UnceasingFear.Domain.Combat.Services;
using UnceasingFear.Domain.Shared.Events;
using static UnceasingFear.Domain.Shared.Events.SharedEvents;

namespace UnceasingFear.Application.Combat
{
    public class BattleServiceProvider
    {
        public BattleApplicationService? ActiveService { get; private set; }
        public IEventDispatcher? EventDispatcher { get; private set; }
        public ICommandDispatcher? CommandDispatcher { get; private set; }

        public void Initialize(IEventDispatcher dispatcher, ICommandDispatcher cmdDispatcher)
        {
            EventDispatcher = dispatcher;
            CommandDispatcher = cmdDispatcher;

            // ✅ Subscribe ONCE. Service created LAZILY only when event fires.
            dispatcher.Subscribe<EnterBattleEvent>(e =>
            {
                CommandDispatcher.Unsubscribe<SelectAbilityEventCommand>();

                ActiveService = new BattleApplicationService(
                    e.AllyProfiles,
                    e.EnemyProfiles,
                    dispatcher,
                    cmdDispatcher,
                    new TurnOrderService(),
                    new TargetResolver(),
                    new Battle()
                    );
            });

            // Optional: Clean up when battle ends
            dispatcher.Subscribe<OutOfBattleEvent>(_ => ActiveService = null);
        }
    }
}
