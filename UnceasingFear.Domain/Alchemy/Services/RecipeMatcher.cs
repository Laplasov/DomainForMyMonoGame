using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.Alchemy.ValueObjects;

namespace UnceasingFear.Domain.Alchemy.Services
{
    public static class RecipeMatcher
    {
        public readonly record struct RecipeMatch(Recipe Recipe, IReadOnlyList<IIngredient> Consumed);

        public static IReadOnlyList<RecipeMatch> MatchAll(IReadOnlyList<IIngredient> ingredients, IReadOnlyList<Recipe> recipes)
        {
            var results = new List<RecipeMatch>();
            var pool = ingredients.ToList();

            // Try recipes with more requirements first, so a 2-ingredient recipe
            // doesn't "steal" an ingredient a more specific 3-ingredient recipe needed.
            foreach (var recipe in recipes.OrderByDescending(RequirementCount))
            {
                if (TryMatch(recipe, pool, out var consumed))
                {
                    results.Add(new RecipeMatch(recipe, consumed));
                    foreach (var c in consumed) pool.Remove(c);
                }
            }

            return results;
        }

        private static int RequirementCount(Recipe r) => r.Inputs.Identities.Count + r.Inputs.Items.Count;

        private static bool TryMatch(Recipe recipe, List<IIngredient> pool, out IReadOnlyList<IIngredient> consumed)
        {
            var available = pool.ToList();
            var used = new List<IIngredient>();

            foreach (var requiredIdentity in recipe.Inputs.Identities)
            {
                int idx = available.FindIndex(ing =>
                    ing is VesselIngredient v && requiredIdentity.Matches(v.Identity));

                if (idx < 0) { consumed = Array.Empty<IIngredient>(); return false; }
                used.Add(available[idx]);
                available.RemoveAt(idx);
            }

            foreach (var requiredItem in recipe.Inputs.Items)
            {
                int idx = available.FindIndex(ing =>
                    ing is ItemIngredient i && i.Item.Type == requiredItem.Type && i.Item.Name == requiredItem.Name);

                if (idx < 0) { consumed = Array.Empty<IIngredient>(); return false; }
                used.Add(available[idx]);
                available.RemoveAt(idx);
            }

            consumed = used;
            return true;
        }
    }
}