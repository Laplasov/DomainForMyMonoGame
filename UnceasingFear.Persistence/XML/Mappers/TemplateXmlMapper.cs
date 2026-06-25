using System.Xml.Linq;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Domain.Shared.ValueObjects.Abilities;
using UnceasingFear.Domain.Shared.ValueObjects.Stats;
using UnceasingFear.Domain.World.ValueObjects;

namespace UnceasingFear.Persistence.Xml.Mappers
{
    public static class TemplateXmlMapper
    {
        // ── Domain → XML ────────────────────────────────────────────────────

        public static XElement ToXml(Template template) =>
            new XElement("Template",
                new XAttribute("name", template.TemplateName),
                new XElement("Profiles",
                    template.Profiles.Select(ProfileToXml))
            );

        private static XElement ProfileToXml(UnitProfile profile) =>
            new XElement("UnitProfile",
                new XAttribute("name", profile.Name),
                new XElement("Stats",
                    new XAttribute("maxHp", profile.BaseStats.MaxHp),
                    new XAttribute("maxSp", profile.BaseStats.MaxSp),
                    new XAttribute("physic", profile.BaseStats.Physic),
                    new XAttribute("defense", profile.BaseStats.Defense),
                    new XAttribute("magic", profile.BaseStats.Magic),
                    new XAttribute("speed", profile.BaseStats.Speed)),
                new XElement("Abilities",
            profile.Abilities.Select(a =>
                new XElement("AbilityRef", new XAttribute("id", a.Id)))),
            new XElement("Stash", profile.Stash.Select(ItemToXml)),
            new XElement("EquippedItems", profile.EquippedItems.Select(ItemToXml)),
            new XElement("ConsumedEssences", ConsumedEssencesToXml(profile.ConsumedEssences))
            );

        private static IEnumerable<XElement> ConsumedEssencesToXml(ConsumedEssence ce)
        {
            foreach (var essence in ce.EssenceList)
                yield return new XElement("Essence",
                    new XAttribute("name", essence.Name),
                    new XAttribute("value", essence.Value));
        }

        private static ConsumedEssence ConsumedEssencesFromXml(XElement? parentEl)
        {
            if (parentEl == null) return ConsumedEssence.Empty;

            // Re-use domain logic by treating XML essences as temporary Items
            return parentEl.Elements("Essence")
                .Select(e => new Item(
                    Guid.NewGuid(),
                    Type: "Essence",
                    Name: e.Attribute("name")!.Value,
                    Quantity: 1,
                    Value: int.Parse(e.Attribute("value")!.Value),
                    Description: "",
                    IsStackable: false)
                )
                .Aggregate(ConsumedEssence.Empty, (ce, item) => ce.AddEssence(item));
        }

        private static XElement ItemToXml(Item item) =>
            new XElement("Item",
                new XAttribute("type", item.Type),
                new XAttribute("name", item.Name),
                new XAttribute("quantity", item.Quantity),
                new XAttribute("value", item.Value),
                new XAttribute("description", item.Description));

        // ── XML → Domain ────────────────────────────────────────────────────

        public static Template FromXml(XElement el, Func<string, Ability> resolveAbility)
        {
            var name = el.Attribute("name")!.Value;
            var profiles = el.Element("Profiles")!
                             .Elements("UnitProfile")
                             .Select((profileEl, index) => ProfileFromXml(profileEl, index, resolveAbility))
                             .ToList();
            return new Template(name, profiles);
        }

        private static UnitProfile ProfileFromXml(XElement el, int index, Func<string, Ability> resolveAbility)
        {
            var name = el.Attribute("name")!.Value;
            var stats = StatsFromXml(el.Element("Stats")!);

            var abilities = el.Element("Abilities")!
                .Elements("AbilityRef")
                .Select(a => resolveAbility(a.Attribute("id")!.Value))
                .ToList();

            var stash = el.Element("Stash")?
                .Elements("Item").Select(ItemFromXml).ToList() ?? new List<Item>();

            var equippedItems = el.Element("EquippedItems")?
                .Elements("Item").Select(ItemFromXml).ToList() ?? new List<Item>();

            var consumedEssences = ConsumedEssencesFromXml(el.Element("ConsumedEssences"));

            return UnitProfile.Create(name, index + 1, stats, abilities, stash, equippedItems, consumedEssences);
        }

        private static Item ItemFromXml(XElement el) =>
            new Item(
                Guid.NewGuid(),
                el.Attribute("type")!.Value,
                el.Attribute("name")!.Value,
                int.Parse(el.Attribute("quantity")!.Value),
                int.Parse(el.Attribute("value")!.Value),
                el.Attribute("description")!.Value,
                el.Attribute("IsStackable")?.Value == "1"
                );
        private static UnitStats StatsFromXml(XElement el) =>
            UnitStats.Create(
                maxHealth: int.Parse(el.Attribute("maxHp")!.Value),
                maxSP: int.Parse(el.Attribute("maxSp")!.Value),
                physic: int.Parse(el.Attribute("physic")!.Value),
                defense: int.Parse(el.Attribute("defense")!.Value),
                magic: int.Parse(el.Attribute("magic")!.Value),
                speed: int.Parse(el.Attribute("speed")!.Value)
            );
    }
}