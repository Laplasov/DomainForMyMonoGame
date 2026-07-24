using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.Shared.ValueObjects;

namespace UnceasingFear.Domain.Alchemy.ValueObjects
{
    public readonly record struct CauldronResult(IReadOnlyList<Item> Items, IReadOnlyList<UnitProfile> Profiles)
    {
        public static CauldronResult Empty => new(Array.Empty<Item>(), Array.Empty<UnitProfile>());
        public CauldronResult AddItem(Item item) => this with { Items = Items.Append(item).ToList().AsReadOnly() };
        public CauldronResult AddProfile(UnitProfile profile) => this with { Profiles = Profiles.Append(profile).ToList().AsReadOnly() };
    }
}
