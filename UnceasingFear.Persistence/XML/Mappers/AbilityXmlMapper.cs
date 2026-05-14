using System.Xml.Linq;
using UnceasingFear.Domain.Combat.Enums;
using UnceasingFear.Domain.Shared.ValueObjects.Abilities;

namespace UnceasingFear.Persistence.Xml.Mappers
{
    public static class AbilityXmlMapper
    {
        public static XElement ToXml(Ability ability) =>
            new XElement("Ability",
                new XAttribute("id", ability.Id),
                new XAttribute("name", ability.Name),
                new XAttribute("description", ability.Description),
                new XAttribute("range", ability.Range),
                new XAttribute("target", ability.Target),
                new XElement("Scales",
                    ability.Scales.Select(s => new XElement("Scale",
                        new XAttribute("stat", s.Stat),
                        new XAttribute("percentage", s.Percentage)))),
                new XElement("Costs",
                    ability.Costs.Select(c => new XElement("Cost",
                        new XAttribute("stat", c.Stat),
                        new XAttribute("value", c.Value)))),
                new XElement("StatusEffects",
                    ability.StatusEffects.Select(se => new XElement("Status",
                        new XAttribute("stat", se.Stat),
                        new XAttribute("value", se.Value))))
            );

        public static Ability FromXml(XElement el)
        {
            var scales = el.Element("Scales")!.Elements("Scale").Select(s =>
                new Scale(
                    Enum.Parse<StatType>(s.Attribute("stat")!.Value),
                    float.Parse(s.Attribute("percentage")!.Value)
                )).ToList();

            var costs = el.Element("Costs")!.Elements("Cost").Select(c =>
                new Cost(
                    Enum.Parse<CostType>(c.Attribute("stat")!.Value),
                    float.Parse(c.Attribute("value")!.Value)
                )).ToList();

            var statusEffects = el.Element("StatusEffects")!.Elements("Status").Select(se =>
                new Status(
                    Enum.Parse<StatusEffectType>(se.Attribute("stat")!.Value),
                    float.Parse(se.Attribute("value")!.Value)
                )).ToList();

            return Ability.Create(
                id: el.Attribute("id")!.Value,
                name: el.Attribute("name")!.Value,
                description: el.Attribute("description")!.Value,
                range: Enum.Parse<TargetRange>(el.Attribute("range")!.Value),
                target: Enum.Parse<Target>(el.Attribute("target")!.Value),
                scales: scales,
                costs: costs,
                statusEffects: statusEffects
            );
        }
    }
}