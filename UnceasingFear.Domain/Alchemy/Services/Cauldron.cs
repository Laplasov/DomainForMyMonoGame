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
using UnceasingFear.Domain.Shared.ValueObjects.Stats;
using static UnceasingFear.Domain.Alchemy.Services.StatsBuilder;

namespace UnceasingFear.Domain.Alchemy.Services
{
    public static class Cauldron
    {
        public static CauldronResult Transmute(
            IReadOnlyList<IIngredient> ingredients,
            IAlchemyContentRepository content,
            StatFormulaConfig statFormula)
        {
            var result = CauldronResult.Empty;
            var matches = RecipeMatcher.MatchAll(ingredients, content.GetRecipes());

            foreach (var match in matches)
            {
                foreach (var item in match.Recipe.Outputs.Items)
                    result = result.AddItem(item);

                foreach (var outputPattern in match.Recipe.Outputs.Identities)
                {
                    var consumedVessels = match.Consumed.OfType<VesselIngredient>().ToList();
                    var identity = ResolveOutputIdentity(outputPattern, consumedVessels);

                    var ability = AbilitySelector.SelectBaseAbility(identity, content.GetBaseAbilities());
                    if (ability is null) continue; // no Any/Any/Any base ability in your data = misconfigured content, not a runtime bug

                    var name = NameGenerator.Generate(identity, content.GetNameData());

                    var inheritedDeltas = consumedVessels.Select(v => v.InheritedStats).ToList();
                    var stats = StatsBuilder.CreateNewFromEssences(inheritedDeltas, identity.Tier, statFormula);

                    var profile = UnitProfile.Create(
                        name: name,
                        slot: 0,
                        stats: stats,
                        abilities: new List<Ability>() { (Ability)ability }.AsReadOnly(),
                        lootDrops: Array.Empty<Item>(),
                        equippedItems: Array.Empty<Item>(),
                        consumedEssences: ConsumedEssence.Empty,
                        identity: identity);

                    result = result.AddProfile(profile);
                }
            }

            return result;
        }

        private static Identity ResolveOutputIdentity(Identity outputPattern, IReadOnlyList<VesselIngredient> consumedVessels)
        {
            if (outputPattern.Tier != 0)
                return outputPattern;

            if (consumedVessels.Count == 0)
                return outputPattern; 

            var lowestTier = consumedVessels.Min(v => v.Identity.Tier);
            bool allEqual = consumedVessels.All(x => x.Identity.Tier == consumedVessels[0].Identity.Tier);

            return allEqual ? outputPattern with { Tier = lowestTier + 1 } : outputPattern with { Tier = lowestTier };
        }
    }
}