using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.Alchemy.Services;
using UnceasingFear.Domain.Alchemy.ValueObjects;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Domain.Shared.ValueObjects.Abilities;

namespace UnceasingFear.Domain.Alchemy.Interfaces
{
    public interface IAlchemyContentRepository
    {
        IReadOnlyList<Recipe> GetRecipes();
        IReadOnlyDictionary<Identity, Ability> GetBaseAbilities();
        NameData GetNameData();
    }
}
