using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Domain.Shared.ValueObjects.Abilities;
using UnceasingFear.Domain.Shared.ValueObjects.Stats;

namespace UnceasingFear.Domain.Alchemy.ValueObjects
{
    public interface IIngredient
    {
        string Name { get; }
        int Tier { get; }
    }

    public readonly record struct VesselIngredient(UnitProfile Vessel) : IIngredient
    {
        public string Name => Vessel.Name;
        public int Tier => Vessel.Identity.Tier;
        public Identity Identity => Vessel.Identity;
        public StatDelta InheritedStats => Vessel.ConsumedEssences.CalculateTotalBonus();

        //public Ingredient.VesselProperties BodyDefinition => new(Vessel.ConsumedEssences.CalculateTotalBonus(), Vessel.Abilities, Vessel.Identity);
    }

    public readonly record struct ItemIngredient(Item Item) : IIngredient
    {
        public string Name => Item.Name;
        public int Tier => Item.Value;
    }

}
