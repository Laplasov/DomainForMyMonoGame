// UnceasingFear.Presentation/Data/XmlSpriteRepository.cs
using System.Xml.Linq;
using UnceasingFear.Presentation.Data;

namespace UnceasingFear.Persistence.XML
{
    public class XmlSpriteRepository
    {
        private readonly string _filePath;

        private Dictionary<string, GroupSpriteData>? _groupSprites;
        private Dictionary<string, UnitSpriteData>? _unitSprites;
        private Dictionary<string, TileSetData>? _tileSets;

        public XmlSpriteRepository(string filePath)
        {
            _filePath = filePath;
        }

        public GroupSpriteData GetGroupSprite(string groupId)
        {
            var cache = LoadGroupSprites();
            if (cache.TryGetValue(groupId, out var data)) return data;
            if (cache.TryGetValue("Generic", out var generic)) return generic;
            throw new KeyNotFoundException($"No GroupSprite for '{groupId}'.");
        }

        public UnitSpriteData GetUnitSprite(string unitId)
        {
            var cache = LoadUnitSprites();
            if (cache.TryGetValue(unitId, out var data)) return data;
            if (cache.TryGetValue("Generic", out var generic)) return generic;
            throw new KeyNotFoundException($"No UnitSprite for '{unitId}'.");
        }

        public TileSetData GetTileSet(string sceneId)
        {
            var cache = LoadTileSets();
            if (cache.TryGetValue(sceneId, out var data)) return data;
            if (cache.TryGetValue("Generic", out var generic)) return generic;
            throw new KeyNotFoundException($"No TileSet for scene '{sceneId}'.");
        }

        // ── Private loaders ─────────────────────────────────────────────────

        private Dictionary<string, GroupSpriteData> LoadGroupSprites()
        {
            if (_groupSprites != null) return _groupSprites;
            var root = LoadRoot();
            _groupSprites = root.Element("WorldSprites")!
                .Elements("GroupSprite")
                .Select(el => new GroupSpriteData(
                    el.Attribute("id")!.Value,
                    el.Attribute("animation")!.Value,
                    el.Attribute("texture")!.Value))
                .ToDictionary(d => d.Id);
            return _groupSprites;
        }

        private Dictionary<string, UnitSpriteData> LoadUnitSprites()
        {
            if (_unitSprites != null) return _unitSprites;
            var root = LoadRoot();
            _unitSprites = root.Element("BattleSprites")!
                .Elements("UnitSprite")
                .Select(el => new UnitSpriteData(
                    el.Attribute("id")!.Value,
                    el.Attribute("animation")!.Value,
                    el.Attribute("texture")!.Value))
                // In case of duplicate ids (your XML has three Goblin entries),
                // keep the first one encountered.
                .GroupBy(d => d.Id)
                .ToDictionary(g => g.Key, g => g.First());
            return _unitSprites;
        }

        private Dictionary<string, TileSetData> LoadTileSets()
        {
            if (_tileSets != null) return _tileSets;
            var root = LoadRoot();
            _tileSets = root.Element("TileSets")!
                .Elements("TileSet")
                .Select(el => new TileSetData(
                    el.Attribute("sceneId")!.Value,
                    el.Attribute("texture")!.Value,
                    el.Attribute("csv")!.Value))
                .ToDictionary(d => d.SceneId);
            return _tileSets;
        }

        private XElement LoadRoot()
        {
            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"Sprite data file not found: {_filePath}");
            return XDocument.Load(_filePath).Root!;
        }
    }
}