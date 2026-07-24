using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.Shared.ValueObjects;

namespace UnceasingFear.Domain.Alchemy.Services
{
    public readonly record struct NameData(
    IReadOnlyDictionary<Element, string> ElementSuffixes,
    IReadOnlyDictionary<UnitType, string> TypeSuffixes,
    IReadOnlyList<string> TierSuffixes,
    string DefaultName
);

    // Domain.Alchemy.Services.NameGenerator.cs
    public static class NameGenerator
    {
        public static string Generate(Identity identity, NameData data)
        {
            // DOMAIN LOGIC: How to combine the parts based on Identity
            string elementSuffixes = data.ElementSuffixes.TryGetValue(identity.Element, out var p) ? p : "";
            string typeSuffixes = data.TypeSuffixes.TryGetValue(identity.Type, out var s) ? s : "";
            string tireSuffixes = data.TierSuffixes[identity.Tier];

            if (string.IsNullOrEmpty(elementSuffixes) && string.IsNullOrEmpty(typeSuffixes) && string.IsNullOrEmpty(tireSuffixes))
                return data.DefaultName;

            return $"{elementSuffixes}{typeSuffixes}{tireSuffixes}";
        }
    }
}
