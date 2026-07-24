using System.Xml.Linq;
using UnceasingFear.Application.Repository;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Domain.World.Aggregates;
using UnceasingFear.Domain.World.ValueObjects;
using UnceasingFear.Persistence.Xml.Mappers;
using UnceasingFear.Domain.World.Entities;

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

        private readonly string _dataDirectory;
        private readonly XmlDialogueRepository _dialogueRepo;

        public XmlSceneRepository(string filePath, XmlGroupRepository groupRepo, XmlDialogueRepository dialogueRepo, string dataDirectory)
        {
            _filePath  = filePath;
            _groupRepo = groupRepo;
            _dataDirectory = dataDirectory;
            _dialogueRepo = dialogueRepo;
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

            var metadataLookup = doc.Root!.Elements("Scene")
                .ToDictionary(
                    s => SceneId.From(s.Attribute("id")!.Value),
                    s => ParseMapData(s, _dataDirectory)  // ✅ Pass _dataDirectory
                );
            Func<string, Group> resolveGroup = (gId) => _groupRepo.GetById(new EntityId(gId));
            Func<string, DialogueTree> resolveDialogue = (dId) => _dialogueRepo.GetById(dId);

            return SceneXmlMapper.FromXml(sceneEl, resolveGroup, resolveDialogue, metadataLookup);
        }

        // Helper to parse metadata (move out of mapper to avoid duplication)
        // ✅ Helper: Parse metadata from TMX source or fallback to inline attributes
        private static TileMapMetadata ParseMapMetadata(XElement el)
        {
            var sourceAttr = el.Attribute("source")?.Value;
            var layerScale = float.Parse(el.Attribute("layerScale")?.Value ?? "1");

            // ✅ If source is provided, load metadata from TMX file
            if (!string.IsNullOrEmpty(sourceAttr))
            {
                // Resolve TMX path relative to Content root (no "Content" prefix in XML)
                var tmxPath = Path.Combine("Content", sourceAttr.Replace('\\', '/'));

                // Load TMX and extract map dimensions
                var doc = XDocument.Load(tmxPath);
                var mapEl = doc.Root ?? throw new InvalidOperationException($"Invalid TMX: {tmxPath}");

                return new TileMapMetadata(
                    Width: int.Parse(mapEl.Attribute("width")?.Value ?? "0"),
                    Height: int.Parse(mapEl.Attribute("height")?.Value ?? "0"),
                    TileWidth: int.Parse(mapEl.Attribute("tilewidth")?.Value ?? "0"),
                    TileHeight: int.Parse(mapEl.Attribute("tileheight")?.Value ?? "0"),
                    LayerScale: layerScale
                );
            }

            // ✅ Fallback to inline attributes (backward compatibility)
            return new TileMapMetadata(
                Width: int.Parse(el.Attribute("width")?.Value ?? "0"),
                Height: int.Parse(el.Attribute("height")?.Value ?? "0"),
                TileWidth: int.Parse(el.Attribute("tileWidth")?.Value ?? "0"),
                TileHeight: int.Parse(el.Attribute("tileHeight")?.Value ?? "0"),
                LayerScale: layerScale
            );
        }
        private static TileMapMetadata ParseMapMetadataOld(XElement el) => new TileMapMetadata(
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

        private static TileMapMetadata ParseMapMetadata(XElement el, string dataDirectory)
        {
            var sourceAttr = el.Attribute("source")?.Value;
            var layerScale = float.Parse(el.Attribute("layerScale")?.Value ?? "1");

            if (!string.IsNullOrEmpty(sourceAttr))
            {
                // ✅ FIX: Use AppContext.BaseDirectory to get executable location
                var baseDirectory = AppContext.BaseDirectory;
                var tmxPath = Path.Combine(baseDirectory, "Content", "TilesData", Path.GetFileName(sourceAttr));

                var doc = XDocument.Load(tmxPath);
                var mapEl = doc.Root ?? throw new InvalidOperationException($"Invalid TMX: {tmxPath}");

                return new TileMapMetadata(
                    Width: int.Parse(mapEl.Attribute("width")?.Value ?? "0"),
                    Height: int.Parse(mapEl.Attribute("height")?.Value ?? "0"),
                    TileWidth: int.Parse(mapEl.Attribute("tilewidth")?.Value ?? "0"),
                    TileHeight: int.Parse(mapEl.Attribute("tileheight")?.Value ?? "0"),
                    LayerScale: layerScale
                );
            }

            return new TileMapMetadata(
                Width: int.Parse(el.Attribute("width")?.Value ?? "0"),
                Height: int.Parse(el.Attribute("height")?.Value ?? "0"),
                TileWidth: int.Parse(el.Attribute("tileWidth")?.Value ?? "0"),
                TileHeight: int.Parse(el.Attribute("tileHeight")?.Value ?? "0"),
                LayerScale: layerScale
            );
        }
        private static Collision ParseCollisionFromTmx(string tmxPath, int width, int height)
        {
            if (!File.Exists(tmxPath))
                return new Collision(new bool[height, width]); // Default: all walkable

            var doc = System.Xml.Linq.XDocument.Load(tmxPath);
            var mapEl = doc.Root ?? throw new InvalidOperationException("Invalid TMX");

            // Find the "Collisions" layer
            var collisionLayer = mapEl.Elements("layer")
                .FirstOrDefault(l => l.Attribute("name")?.Value == "Collisions");

            if (collisionLayer == null)
                return new Collision(new bool[height, width]); // No collision layer = all walkable

            var csv = collisionLayer.Element("data")?.Value?.Trim() ?? "";
            var grid = new bool[height, width];

            var rows = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            for (int y = 0; y < height && y < rows.Length; y++)
            {
                var cols = rows[y].Split(',');
                for (int x = 0; x < width && x < cols.Length; x++)
                {
                    // Your Tiled setup: tile ID 101 = collision (from marks.tsx)
                    if (int.TryParse(cols[x].Trim(), out int tileId) && tileId == 101)
                        grid[y, x] = true;
                }
            }
            return new Collision(grid);
        }
        private static (TileMapMetadata, Collision) ParseMapData(XElement el, string basePath)
        {
            var sourceAttr = el.Element("TileMapMetadata")?.Attribute("source")?.Value;
            var layerScale = float.Parse(el.Element("TileMapMetadata")?.Attribute("layerScale")?.Value ?? "1");

            if (!string.IsNullOrEmpty(sourceAttr))
            {
                // ✅ FIX: Use basePath which should already include AppContext.BaseDirectory
                var tmxPath = Path.Combine(basePath, "TilesData", Path.GetFileName(sourceAttr));

                var doc = System.Xml.Linq.XDocument.Load(tmxPath);
                var mapEl = doc.Root ?? throw new InvalidOperationException($"Invalid TMX: {tmxPath}");

                var metadata = new TileMapMetadata(
                    Width: int.Parse(mapEl.Attribute("width")?.Value ?? "0"),
                    Height: int.Parse(mapEl.Attribute("height")?.Value ?? "0"),
                    TileWidth: int.Parse(mapEl.Attribute("tilewidth")?.Value ?? "0"),
                    TileHeight: int.Parse(mapEl.Attribute("tileheight")?.Value ?? "0"),
                    LayerScale: layerScale
                );

                var collision = ParseCollisionFromTmx(tmxPath, metadata.Width, metadata.Height);

                return (metadata, collision);
            }

            var metaEl = el.Element("TileMapMetadata");
            var fallbackMeta = new TileMapMetadata(
                Width: int.Parse(metaEl?.Attribute("width")?.Value ?? "0"),
                Height: int.Parse(metaEl?.Attribute("height")?.Value ?? "0"),
                TileWidth: int.Parse(metaEl?.Attribute("tileWidth")?.Value ?? "0"),
                TileHeight: int.Parse(metaEl?.Attribute("tileHeight")?.Value ?? "0"),
                LayerScale: layerScale
            );
            return (fallbackMeta, new Collision(new bool[fallbackMeta.Height, fallbackMeta.Width]));
        }

    }
}
