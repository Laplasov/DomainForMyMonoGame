using System.Xml.Linq;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Domain.World.Entities;
using UnceasingFear.Domain.World.Enums;
using UnceasingFear.Domain.World.ValueObjects;

namespace UnceasingFear.Persistence.Xml.Mappers
{
    public static class GroupXmlMapper
    {
        // ── Domain → XML ────────────────────────────────────────────────────

        public static XElement ToXml(Group group)
        {
            var el = new XElement("Group",
                new XAttribute("id", group.Id.Value),
                new XElement("MovementPattern", group.UnitBehavior),
                new XElement("DetectionRange", group.DetectionRange.Value),
                new XElement("Speed", group.Speed.Value),
                new XElement("SpawnPosition",
                    new XAttribute("x", group.SpawnPosition.X),
                    new XAttribute("y", group.SpawnPosition.Y))
            );

            foreach (var profile in group.Template.Profiles)
            {
                var refEl = new XElement("TemplateRef",
                    new XAttribute("id", profile.Name), // Assuming Profile.Name matches the Template ID
                    new XAttribute("slot", profile.SlotIndex));

                if (profile.EquippedItems.Count > 0)
                    refEl.Add(new XElement("EquippedItems", profile.EquippedItems.Select(ItemToXml)));

                if (profile.ConsumedEssences.EssenceList.Count > 0)
                    refEl.Add(new XElement("ConsumedEssences", ConsumedEssencesToXml(profile.ConsumedEssences)));

                el.Add(refEl);
            }

            return el;
        }

        // ── XML → Domain ────────────────────────────────────────────────────

        public static Group FromXml(XElement el, Func<string, Template> resolveTemplate)
        {
            var id = el.Attribute("id")!.Value;
            var movementPattern = Enum.Parse<UnitBehavior>(el.Element("MovementPattern")!.Value);
            var detectionRange = float.Parse(el.Element("DetectionRange")!.Value);
            var speed = float.Parse(el.Element("Speed")!.Value);

            var template = ResolveTemplate(el, resolveTemplate);

            return new Group(
                id: new EntityId(id),
                template: template,
                unitBehavior: movementPattern,
                detectionRange: new ProximityRange(detectionRange),
                speed: new MovementSpeed(speed),
                startPosition: WorldPosition.Zero,
                dialogueTree: DialogueTree.Empty
            );
        }

        private static Template ResolveTemplate(XElement el, Func<string, Template> resolveTemplate)
        {
            var refs = el.Elements("TemplateRef").ToList();
            if (refs.Count == 0)
                throw new InvalidDataException($"Group '{el.Attribute("id")?.Value}' has no TemplateRef.");

            var newProfiles = new List<UnitProfile>();

            foreach (var refEl in refs)
            {
                var templateId = refEl.Attribute("id")!.Value;
                var fullTemplate = resolveTemplate(templateId);
                var baseProfile = fullTemplate.Profiles[0];

                var equippedItems = refEl.Element("EquippedItems")?
                    .Elements("Item").Select(ItemFromXml).ToList() ?? baseProfile.EquippedItems.ToList();

                var consumedEssencesEl = refEl.Element("ConsumedEssences");
                var consumedEssences = consumedEssencesEl != null
                    ? ConsumedEssencesFromXml(consumedEssencesEl)
                    : baseProfile.ConsumedEssences; // Fallback to template if group doesn't specify

                var slotAttr = refEl.Attribute("slot");
                int slotIndex = slotAttr != null && int.TryParse(slotAttr.Value, out int s) ? s : baseProfile.SlotIndex;

                newProfiles.Add(baseProfile with
                {
                    SlotIndex = slotIndex,
                    EquippedItems = equippedItems.AsReadOnly(),
                    ConsumedEssences = consumedEssences
                });
            }

            var entityId = el.Attribute("id")!.Value;
            return new Template(entityId, newProfiles);
        }
        // ── Shared Helpers (Match TemplateXmlMapper) ────────────────────────

        private static XElement ItemToXml(Item item) =>
            new XElement("Item",
                new XAttribute("type", item.Type),
                new XAttribute("name", item.Name),
                new XAttribute("quantity", item.Quantity),
                new XAttribute("value", item.Value),
                new XAttribute("description", item.Description));

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

        private static IEnumerable<XElement> ConsumedEssencesToXml(ConsumedEssence ce)
        {
            foreach (var essence in ce.EssenceList)
                yield return new XElement("Essence",
                    new XAttribute("name", essence.Name),
                    new XAttribute("value", essence.Value));
        }

        private static ConsumedEssence ConsumedEssencesFromXml(XElement parentEl)
        {
            if (parentEl == null) return ConsumedEssence.Empty;

            return parentEl.Elements("Essence")
                .Select(e => new Item(
                    Id: Guid.NewGuid(),
                    Type: "Essence",
                    Name: e.Attribute("name")!.Value,
                    Quantity: 1,
                    Value: int.Parse(e.Attribute("value")!.Value),
                    Description: "",
                    IsStackable: false)
                )
                .Aggregate(ConsumedEssence.Empty, (ce, item) => ce.AddEssence(item));
        }
    }
}
