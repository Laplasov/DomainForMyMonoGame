using System.Xml.Linq;
using UnceasingFear.Application.Repository;
using UnceasingFear.Domain.World.Aggregates;
using UnceasingFear.Domain.World.ValueObjects;
using UnceasingFear.Persistence.Xml.Mappers;

namespace UnceasingFear.Persistence.Xml
{
    /// <summary>
    /// Implements ISceneProvider using a scenes.xml file.
    /// Groups referenced by a scene are resolved through XmlGroupRepository.
    /// </summary>
    public class XmlSceneRepository : ISceneProvider
    {
        private readonly string _filePath;
        private readonly XmlGroupRepository _groupRepo;

        public XmlSceneRepository(string filePath, XmlGroupRepository groupRepo)
        {
            _filePath  = filePath;
            _groupRepo = groupRepo;
        }

        // ── ISceneProvider ───────────────────────────────────────────────────

        public Scene? GetById(SceneId id)
        {
            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"Scene data file not found: {_filePath}");

            var doc = XDocument.Load(_filePath);
            var sceneEl = doc.Root!
                .Elements("Scene")
                .FirstOrDefault(e => e.Attribute("id")?.Value == id.Value);

            if (sceneEl == null) return null;

            // ✅ 1. Pre-parse ALL TileMapMetadata from the document into a lookup
            var metadataLookup = doc.Root!.Elements("Scene")
                .ToDictionary(
                    s => SceneId.From(s.Attribute("id")!.Value),
                    s => ParseMapMetadata(s.Element("TileMapMetadata")!)
                );

            // ✅ 2. Pass the lookup to the scene mapper
            return SceneXmlMapper.FromXml(sceneEl,
                groupId => _groupRepo.GetById(new GroupId(groupId)),
                metadataLookup);
        }

        // Helper to parse metadata (move out of mapper to avoid duplication)
        private static TileMapMetadata ParseMapMetadata(XElement el) => new TileMapMetadata(
            Width: int.Parse(el.Attribute("width")!.Value),
            Height: int.Parse(el.Attribute("height")!.Value),
            TileWidth: int.Parse(el.Attribute("tileWidth")!.Value),
            TileHeight: int.Parse(el.Attribute("tileHeight")!.Value),
            LayerScale: float.Parse(el.Attribute("layerScale")!.Value)
        );

        public void Save(Scene scene)
        {
            // Load existing document (or create empty root) so we do an upsert.
            XDocument doc;
            XElement  root;

            if (File.Exists(_filePath))
            {
                doc  = XDocument.Load(_filePath);
                root = doc.Root!;
            }
            else
            {
                root = new XElement("Scenes");
                doc  = new XDocument(new XDeclaration("1.0", "utf-8", null), root);
            }

            // Remove existing entry with the same id, then add the updated one.
            root.Elements("Scene")
                .Where(e => e.Attribute("id")?.Value == scene.Id.Value)
                .Remove();

            root.Add(SceneXmlMapper.ToXml(scene));

            // Persist any groups that belong to this scene so the group file
            // stays in sync (new groups added at runtime will be written here).
            _groupRepo.Save(scene.Groups);

            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            doc.Save(_filePath);
        }
    }
}
