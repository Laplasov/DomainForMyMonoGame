using System.Xml.Linq;
using UnceasingFear.Domain.World.Aggregates;
using UnceasingFear.Domain.World.Entities;
using UnceasingFear.Domain.World.Enums;
using UnceasingFear.Domain.World.ValueObjects;

namespace UnceasingFear.Persistence.Xml.Mappers
{
    /// <summary>
    /// Maps Scene ↔ XElement.
    /// Groups are stored by reference (id only) inside the scene XML;
    /// the caller supplies the resolved Group instances on read.
    /// </summary>
    public static class SceneXmlMapper
    {
        // ── Domain → XML ────────────────────────────────────────────────────

        public static XElement ToXml(Scene scene)
        {
            return new XElement("Scene",
                new XAttribute("id", scene.Id.Value),
                MapMetadataToXml(scene.MapMetadata),
                new XElement("GroupRefs",
                    scene.Groups.Select(GroupRefToXml)),
                new XElement("Transitions",
                    scene.Transitions.Select(TransitionToXml))
            );
        }
        private static XElement GroupRefToXml(Group group)
        {
            var el = new XElement("GroupRef", new XAttribute("id", group.Id.Value));

            // ✅ Only write spawnX/spawnY if it differs from the group's default SpawnPosition
            // This keeps XML clean and avoids redundancy
            if (group.CurrentPosition != group.SpawnPosition)
            {
                el.SetAttributeValue("spawnX", group.CurrentPosition.X);
                el.SetAttributeValue("spawnY", group.CurrentPosition.Y);
            }
            return el;
        }

        private static XElement MapMetadataToXml(TileMapMetadata m)
        {
            return new XElement("TileMapMetadata",
                new XAttribute("width",       m.Width),
                new XAttribute("height",      m.Height),
                new XAttribute("tileWidth",   m.TileWidth),
                new XAttribute("tileHeight",  m.TileHeight),
                new XAttribute("layerScale",  m.LayerScale)
            );
        }

        private static XElement TransitionToXml(SceneTransition t)
        {
            return new XElement("Transition",
                new XElement("TriggerTile",
                    new XAttribute("x", t.TriggerTile.X),
                    new XAttribute("y", t.TriggerTile.Y)),
                new XElement("TargetScene", t.TargetScene.Value),
                new XElement("NextSceneTile",
                    new XAttribute("x", t.NextSceneTile.X),
                    new XAttribute("y", t.NextSceneTile.Y))
            );
        }

        // ── XML → Domain ────────────────────────────────────────────────────

        /// <summary>
        /// Reconstructs a Scene from XML.
        /// <paramref name="resolveGroup"/> is called for each GroupRef id
        /// so the repository can inject the already-loaded Group entities.
        /// </summary>
        public static Scene FromXml(
            XElement el,
            Func<string, Group> resolveGroup,
            Dictionary<SceneId, TileMapMetadata> metadataLookup) // ✅ New parameter
        {
            var id = SceneId.From(el.Attribute("id")!.Value);

            // ✅ Use pre-parsed metadata instead of parsing inline
            var metadata = metadataLookup[id];
            var scene = new Scene(id, metadata);

            foreach (var groupRef in el.Element("GroupRefs")!.Elements("GroupRef"))
            {
                var groupId = groupRef.Attribute("id")!.Value;
                var group = resolveGroup(groupId);

                // Parse spawnX/spawnY as before
                var xAttr = groupRef.Attribute("spawnX");
                var yAttr = groupRef.Attribute("spawnY");
                if (xAttr != null && yAttr != null &&
                    int.TryParse(xAttr.Value, out int x) &&
                    int.TryParse(yAttr.Value, out int y))
                {
                    var worldPos = metadata.TileToWorld(new TileCoord(x, y));
                    group.MoveTo(worldPos);
                    group.ChangeSpawn(worldPos);
                }

                scene.AddGroup(group);
            }

            // ✅ Pass metadata lookup to transition parser
            foreach (var transEl in el.Element("Transitions")?.Elements("Transition") ?? Enumerable.Empty<XElement>())
                scene.AddTransition(TransitionFromXml(transEl, metadataLookup));

            return scene;
        }
        private static SceneTransition TransitionFromXml(XElement el, Dictionary<SceneId, TileMapMetadata> metadataLookup)
        {
            var triggerEl = el.Element("TriggerTile")!;
            var nextEl = el.Element("NextSceneTile")!;
            var targetId = SceneId.From(el.Element("TargetScene")!.Value);

            // ✅ 1. Parse as TILE coordinates (integers)
            var tileCoord = new TileCoord(
                int.Parse(nextEl.Attribute("x")!.Value),
                int.Parse(nextEl.Attribute("y")!.Value));

            // ✅ 2. Convert using the TARGET scene's TileMapMetadata
            var worldPos = metadataLookup.TryGetValue(targetId, out var targetMeta)
                ? targetMeta.TileToWorld(tileCoord) // Converts tile → world center
                : new WorldPosition(tileCoord.X * 64f + 32f, tileCoord.Y * 64f + 32f); // Fallback if missing

            return new SceneTransition(
                triggerTile: new TileCoord(
                    int.Parse(triggerEl.Attribute("x")!.Value),
                    int.Parse(triggerEl.Attribute("y")!.Value)),
                targetScene: targetId,
                nextSceneTile: worldPos
            );
        }
    }
}
