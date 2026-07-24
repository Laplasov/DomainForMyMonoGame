using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.Shared.ValueObjects;

namespace UnceasingFear.Domain.Alchemy.ValueObjects
{
    public readonly record struct Recipe
    {
        public readonly record struct RecipeContainer(IReadOnlyList<Identity> Identities, IReadOnlyList<Item> Items);
        public RecipeContainer Inputs { get; }
        public RecipeContainer Outputs { get; }
        public Recipe(RecipeContainer inputs, RecipeContainer outputs) { Inputs = inputs; Outputs = outputs; }
       
    }
}
