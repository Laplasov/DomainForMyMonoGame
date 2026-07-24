using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnceasingFear.Domain.Alchemy.ValueObjects
{
    namespace UnceasingFear.Domain.Alchemy.ValueObjects
    {
        public readonly record struct CauldronInputs(IReadOnlyList<IIngredient> Ingredients)
        {
            public static CauldronInputs Empty => new(Array.Empty<IIngredient>());

            public bool TryAdd(IIngredient ingredient, out CauldronInputs result)
            {
                if (Ingredients.Count >= 3) { result = this; return false; }
                result = new CauldronInputs(Ingredients.Append(ingredient).ToList().AsReadOnly());
                return true;
            }

            public bool TryRemove(int index, out CauldronInputs result)
            {
                if (index < 0 || index >= Ingredients.Count) { result = this; return false; }
                var list = Ingredients.ToList();
                list.RemoveAt(index);
                result = new CauldronInputs(list.AsReadOnly());
                return true;
            }
        }
    }
}
