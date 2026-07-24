using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.Shared.Enums;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Domain.Shared.ValueObjects.Abilities;

namespace UnceasingFear.Domain.Alchemy.Services
{
    public static class AbilitySelector
    {
        public static Ability? SelectBaseAbility(Identity identity, IReadOnlyDictionary<Identity, Ability> abilities)
        {
            Ability? best = null;
            int bestScore = -1;

            foreach (var ability in abilities)
            {
                if (ability.Value.Inheritability != InheritableType.Base) continue;
                if (!ability.Key.Matches(identity)) continue;

                int score = Specificity(ability.Key);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = ability.Value;
                }
            }

            return best;
        }

        // More concrete fields = higher score. Any/Any/Any always scores 0,
        // so it only wins when nothing more specific matched — your fallback, for free.
        private static int Specificity(Identity pattern)
        {
            int score = 0;
            if (pattern.Element != Element.None) score++;
            if (pattern.Type != UnitType.None) score++;
            if (pattern.Tier != 0) score++;
            return score;
        }
    }
}