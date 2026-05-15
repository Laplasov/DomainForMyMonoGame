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

            var templateId = refs[0].Attribute("id")!.Value;
            var fullTemplate = resolveTemplate(templateId);

            // If only one ref and no slot is specified, return the template as-is
            if (refs.Count == 1 && refs[0].Attribute("slot") == null)
                return fullTemplate;

            var newProfiles = new List<UnitProfile>();

            // Map each <TemplateRef> to a profile from the template
            for (int i = 0; i < refs.Count; i++)
            {
                var refEl = refs[i];
                var slotAttr = refEl.Attribute("slot");

                // Get the corresponding profile from the template. 
                // If there are more refs than profiles, cycle back to the first one.
                var baseProfile = i < fullTemplate.Profiles.Count
                    ? fullTemplate.Profiles[i]
                    : fullTemplate.Profiles[0];

                if (slotAttr != null && int.TryParse(slotAttr.Value, out int slotIndex))
                {
                    // ✅ Explicitly assign the slot from the XML to the profile
                    newProfiles.Add(baseProfile.AssignToSlot(slotIndex));
                }
                else
                {
                    // Keep the original slot if none is specified in XML
                    newProfiles.Add(baseProfile);
                }
            }

            return new Template(fullTemplate.TemplateName, newProfiles);
        }
    }
}