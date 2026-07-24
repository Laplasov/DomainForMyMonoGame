using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.Alchemy.Interfaces;
using UnceasingFear.Domain.Alchemy.Services;
using UnceasingFear.Domain.Alchemy.ValueObjects;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Domain.Shared.ValueObjects.Abilities;

namespace UnceasingFear.Persistence.XML
{
    public class XmlAlchemyContentRepository : IAlchemyContentRepository
    {
        private readonly XmlAbilityRepository _abilityRepo; // shared instance, injected — not owned/constructed here
        private readonly XmlRecipeRepository _recipeRepo;

        public XmlAlchemyContentRepository(XmlAbilityRepository abilityRepo, string recipesFilePath)
        {
            _abilityRepo = abilityRepo;
            _recipeRepo = new XmlRecipeRepository(recipesFilePath);
        }

        public IReadOnlyList<Recipe> GetRecipes() => _recipeRepo.GetRecipes();
        public IReadOnlyDictionary<Identity, Ability> GetBaseAbilities() => _abilityRepo.GetBaseAbilitiesByIdentity();

        // Stub until suffix content exists — TransmutationEngine already tolerates empty dictionaries/DefaultName.
        public NameData GetNameData() => new(
            ElementSuffixes: new Dictionary<Element, string>(),
            TypeSuffixes: new Dictionary<UnitType, string>(),
            TierSuffixes: Array.Empty<string>(),
            DefaultName: "Unstable Essence");
    }
}
