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
                    new XAttribute("maxHp", profile.Stats.Health.Max),
                    new XAttribute("maxSp", profile.Stats.SpellPoints.Max),
                    new XAttribute("physic", profile.Stats.Physic),
                    new XAttribute("defense", profile.Stats.Defense),
                    new XAttribute("magic", profile.Stats.Magic),
                    new XAttribute("speed", profile.Stats.Speed)),
                new XElement("Abilities",
                    profile.Abilities.Select(a =>
                        new XElement("AbilityRef", new XAttribute("id", a.Id))))
            );

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

            // ✅ Use index + 1 so default slots are 1, 2, 3... matching BattleView's expectation
            return UnitProfile.Create(name, index + 1, stats, abilities);
        }

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