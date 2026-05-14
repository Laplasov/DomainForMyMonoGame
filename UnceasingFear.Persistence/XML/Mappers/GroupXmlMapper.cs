using System.Xml.Linq;
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
                new XElement("MovementPattern", group.MovementPattern),
                new XElement("AggroRange", group.AggroRange.Value),
                new XElement("Speed", group.Speed.Value),
                new XElement("SpawnPosition",
                    new XAttribute("x", group.SpawnPosition.X),
                    new XAttribute("y", group.SpawnPosition.Y))
            );

            // Write one TemplateRef per profile, carrying the slot index
            foreach (var profile in group.Template.Profiles)
                el.Add(new XElement("TemplateRef",
                    new XAttribute("id", group.Template.TemplateName),
                    new XAttribute("slot", profile.SlotIndex)));

            return el;
        }

        // ── XML → Domain ────────────────────────────────────────────────────

        public static Group FromXml(XElement el, Func<string, Template> resolveTemplate)
        {
            var id = el.Attribute("id")!.Value;
            var movementPattern = Enum.Parse<MovementPattern>(el.Element("MovementPattern")!.Value);
            var aggroRange = float.Parse(el.Element("AggroRange")!.Value);
            var speed = float.Parse(el.Element("Speed")!.Value);

            var spawnEl = el.Element("SpawnPosition")!;
            var spawn = new WorldPosition(
                float.Parse(spawnEl.Attribute("x")!.Value),
                float.Parse(spawnEl.Attribute("y")!.Value));

            var template = ResolveTemplate(el, resolveTemplate);

            return new Group(
                id: new GroupId(id),
                template: template,
                movementPattern: movementPattern,
                aggroRange: new AggroRange(aggroRange),
                speed: new MovementSpeed(speed),
                startPosition: spawn
            );
        }

        private static Template ResolveTemplate(XElement el, Func<string, Template> resolveTemplate)
        {
            // All TemplateRefs on this group (may differ by slot)
            var refs = el.Elements("TemplateRef").ToList();
            if (refs.Count == 0)
                throw new InvalidDataException($"Group '{el.Attribute("id")?.Value}' has no TemplateRef.");

            // All refs should point to the same template id
            var templateId = refs[0].Attribute("id")!.Value;
            var fullTemplate = resolveTemplate(templateId);

            // If only one ref and it has no slot, return template as-is
            if (refs.Count == 1 && refs[0].Attribute("slot") == null)
                return fullTemplate;

            // Otherwise filter profiles to only the requested slots
            var requestedSlots = refs
                .Select(r => r.Attribute("slot") != null
                    ? (int?)int.Parse(r.Attribute("slot")!.Value)
                    : null)
                .Where(s => s != null)
                .Select(s => s!.Value)
                .ToHashSet();

            var filteredProfiles = fullTemplate.Profiles
                .Where(p => requestedSlots.Contains(p.SlotIndex))
                .ToList();

            return new Template(fullTemplate.TemplateName, filteredProfiles);
        }
    }
}