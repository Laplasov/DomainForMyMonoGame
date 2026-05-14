using System.Xml.Linq;
using UnceasingFear.Domain.Shared.ValueObjects.Abilities;
using UnceasingFear.Persistence.Xml.Mappers;

namespace UnceasingFear.Persistence.XML
{
    public class XmlAbilityRepository
    {
        private readonly string _filePath;
        private Dictionary<string, Ability>? _cache;

        public XmlAbilityRepository(string filePath) => _filePath = filePath;

        public Ability GetById(string id)
        {
            var cache = LoadAll();
            if (!cache.TryGetValue(id, out var ability))
                throw new KeyNotFoundException($"Ability '{id}' not found in '{_filePath}'.");
            return ability;
        }

        public IReadOnlyDictionary<string, Ability> LoadAll()
        {
            if (_cache != null) return _cache;

            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"Ability data file not found: {_filePath}");

            var doc = XDocument.Load(_filePath);
            _cache = doc.Root!
                        .Elements("Ability")
                        .Select(AbilityXmlMapper.FromXml)
                        .ToDictionary(a => a.Id);
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
        }
    }
}