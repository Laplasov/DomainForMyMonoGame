using System.Xml.Linq;
using UnceasingFear.Domain.Shared.Enums;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Domain.Shared.ValueObjects.Abilities;
using UnceasingFear.Persistence.Xml.Mappers;

namespace UnceasingFear.Persistence.XML
{
    public class XmlAbilityRepository
    {
        private readonly string _filePath;

        private Dictionary<string, Ability>? _cache;

        private Dictionary<Identity, Ability>? _baseByIdentity;

        public XmlAbilityRepository(string filePath) => _filePath = filePath;

        public Ability GetById(string id)
        {
            var cache = LoadAll();
            if (!cache.TryGetValue(id, out var ability))
                throw new KeyNotFoundException($"Ability '{id}' not found in '{_filePath}'.");
            return ability;
        }
        public IReadOnlyDictionary<Identity, Ability> GetBaseAbilitiesByIdentity()
        {
            LoadAll(); 
            return _baseByIdentity!;
        }

        public IReadOnlyDictionary<string, Ability> LoadAll()
        {
            if (_cache != null) return _cache;

            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"Ability data file not found: {_filePath}");

            var doc = XDocument.Load(_filePath);
            var elements = doc.Root!.Elements("Ability").ToList();

            _cache = elements
                .Select(AbilityXmlMapper.FromXml)
                .ToDictionary(a => a.Id);

            _baseByIdentity = elements
                .Where(el => el.Attribute("inheritability")!.Value == nameof(InheritableType.Base))
                .ToDictionary(
                    el => AbilityXmlMapper.ParseIdentity(el.Element("Identity"))!.Value,
                    el => _cache[el.Attribute("id")!.Value]);

            return _cache;
        }

        public void Save(IEnumerable<Ability> abilities)
        {
            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("Abilities", abilities.Select(AbilityXmlMapper.ToXml))
            );
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            doc.Save(_filePath);
            _cache = null;
            _baseByIdentity = null;
        }
    }
}