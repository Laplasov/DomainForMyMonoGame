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
        public static Scene FromXml(XElement el, Func<string, Group> resolveGroup)
        {
            var id = SceneId.From(el.Attribute("id")!.Value);
            var metadata = MapMetadataFromXml(el.Element("TileMapMetadata")!);
            var scene = new Scene(id, metadata);

            foreach (var groupRef in el.Element("GroupRefs")!.Elements("GroupRef"))
            {
                var groupId = groupRef.Attribute("id")!.Value;
                var group = resolveGroup(groupId); // Loads from groups.xml (position = 0,0)

                // ✅ Parse scene-specific spawnX/spawnY from <GroupRef>
                var xAttr = groupRef.Attribute("spawnX");
                var yAttr = groupRef.Attribute("spawnY");

                if (xAttr != null && yAttr != null &&
                    float.TryParse(xAttr.Value, out float x) &&
                    float.TryParse(yAttr.Value, out float y))
                {
                    // ✅ Immediately override CurrentPosition for THIS scene
                    group.MoveTo(new WorldPosition(x, y));
                    group.ChangeSpawn(new WorldPosition(x, y));
                }

                scene.AddGroup(group);
            }

            // ✅ Safely handle empty/missing <Transitions>
            foreach (var transEl in el.Element("Transitions")?.Elements("Transition") ?? Enumerable.Empty<XElement>())
                scene.AddTransition(TransitionFromXml(transEl));

            return scene;
        }

        private static TileMapMetadata MapMetadataFromXml(XElement el)
        {
            return new TileMapMetadata(
                Width:      int.Parse(el.Attribute("width")!.Value),
                Height:     int.Parse(el.Attribute("height")!.Value),
                TileWidth:  int.Parse(el.Attribute("tileWidth")!.Value),
                TileHeight: int.Parse(el.Attribute("tileHeight")!.Value),
                LayerScale: float.Parse(el.Attribute("layerScale")!.Value)
            );
        }

        private static SceneTransition TransitionFromXml(XElement el)
        {
            var triggerEl = el.Element("TriggerTile")!;
            var nextEl    = el.Element("NextSceneTile")!;

            return new SceneTransition(
                triggerTile: new TileCoord(
                    int.Parse(triggerEl.Attribute("x")!.Value),
                    int.Parse(triggerEl.Attribute("y")!.Value)),
                targetScene: SceneId.From(el.Element("TargetScene")!.Value),
                nextSceneTile: new WorldPosition(
                    float.Parse(nextEl.Attribute("x")!.Value),
                    float.Parse(nextEl.Attribute("y")!.Value))
            );
        }
    }
}
