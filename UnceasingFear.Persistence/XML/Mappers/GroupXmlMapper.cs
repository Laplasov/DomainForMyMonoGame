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

            var template = ResolveTemplate(el, resolveTemplate);

            return new Group(
                id: new GroupId(id),
                template: template,
                movementPattern: movementPattern,
                aggroRange: new AggroRange(aggroRange),
                speed: new MovementSpeed(speed),
                startPosition: WorldPosition.Zero
            );
        }

        private static Template ResolveTemplate(XElement el, Func<string, Template> resolveTemplate)
        {
            var refs = el.Elements("TemplateRef").ToList();
            if (refs.Count == 0)
                throw new InvalidDataException($"Group '{el.Attribute("id")?.Value}' has no TemplateRef.");

            var newProfiles = new List<UnitProfile>();

            // Loop through each TemplateRef individually
            foreach (var refEl in refs)
            {
                // ✅ Resolve the specific template for THIS reference (Player, Goblin, or Slime)
                var templateId = refEl.Attribute("id")!.Value;
                var fullTemplate = resolveTemplate(templateId);

                // Grab the first profile from the resolved template as the base
                var baseProfile = fullTemplate.Profiles[0];

                var slotAttr = refEl.Attribute("slot");
                if (slotAttr != null && int.TryParse(slotAttr.Value, out int slotIndex))
                {
                    // Assign the explicit slot from the XML
                    newProfiles.Add(baseProfile.AssignToSlot(slotIndex));
                }
                else
                {
                    // Keep the original slot if none is specified
                    newProfiles.Add(baseProfile);
                }
            }

            // Since a group can now be a composite of multiple templates, 
            // it's best to name the composite template after the Group Id.
            var groupId = el.Attribute("id")!.Value;
            return new Template(groupId, newProfiles);
        }
    }
}