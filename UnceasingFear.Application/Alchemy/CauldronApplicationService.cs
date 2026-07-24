using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Application.Commands;
using UnceasingFear.Application.Repository;
using UnceasingFear.Domain.Alchemy.Interfaces;
using UnceasingFear.Domain.Alchemy.Services;
using UnceasingFear.Domain.Alchemy.ValueObjects;
using UnceasingFear.Domain.Alchemy.ValueObjects.UnceasingFear.Domain.Alchemy.ValueObjects;
using UnceasingFear.Domain.Shared.Events;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Domain.World.Aggregates;
using static UnceasingFear.Domain.Alchemy.Services.StatsBuilder;

namespace UnceasingFear.Application.Alchemy
{
    public readonly record struct AddIngredientCommand(IIngredient Ingredient)
    {
        public static AddIngredientCommand FromItem(Item item) => new(new ItemIngredient(item));
        public static AddIngredientCommand FromVessel(UnitProfile vessel) => new(new VesselIngredient(vessel));
    }
    public record struct RemoveIngredientCommand(int Index);
    public record struct TransmuteCommand();
    public class CauldronApplicationService
    {
        private CauldronInputs _inputs = CauldronInputs.Empty;

        private readonly IAlchemyContentRepository _content;
        private readonly StatFormulaConfig _statFormula;
        public IEventDispatcher EventDispatcher { get; }
        public ICommandDispatcher CommandDispatcher { get; }
        public CauldronApplicationService(IAlchemyContentRepository content, StatFormulaConfig statFormula, IEventDispatcher eventDispatcher, ICommandDispatcher commandDispatcher)
        {
            _content = content;
            _statFormula = statFormula;
            EventDispatcher = eventDispatcher;
            CommandDispatcher = commandDispatcher;

            CommandDispatcher.Register<AddIngredientCommand>(cmd => _inputs.TryAdd(cmd.Ingredient, out _inputs));
            CommandDispatcher.Register<RemoveIngredientCommand>(cmd => _inputs.TryRemove(cmd.Index, out _inputs));
            CommandDispatcher.Register<TransmuteCommand>(OnTransmute);
        }

        private void OnTransmute(TransmuteCommand cmd)
        {
            if (_inputs.Ingredients.Count == 0) return;

            var result = Cauldron.Transmute(_inputs.Ingredients, _content, _statFormula);
            _inputs = CauldronInputs.Empty;

            // hand result to whoever owns inventory/roster — that's a separate question
            // for whenever you decide where created items/units actually go
        }

        public IReadOnlyList<string> CurrentIngredientNames =>
            _inputs.Ingredients.Select(i => i.Name).ToList();
    }
}

