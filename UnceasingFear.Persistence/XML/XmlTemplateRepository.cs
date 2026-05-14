using System.Xml.Linq;
using UnceasingFear.Domain.World.ValueObjects;
using UnceasingFear.Persistence.Xml.Mappers;

namespace UnceasingFear.Persistence.XML
{
    public class XmlTemplateRepository
    {
        private readonly string _filePath;
        private readonly XmlAbilityRepository _abilityRepo;
        private Dictionary<string, Template>? _cache;

        public XmlTemplateRepository(string filePath, XmlAbilityRepository abilityRepo)
        {
            _filePath = filePath;
            _abilityRepo = abilityRepo;
        }

        public Template GetById(string name)
        {
            var cache = LoadAll();
            if (!cache.TryGetValue(name, out var template))
                throw new KeyNotFoundException($"Template '{name}' not found in '{_filePath}'.");
            return template;
        }

        public IReadOnlyDictionary<string, Template> LoadAll()
        {
            if (_cache != null) return _cache;

            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"Template data file not found: {_filePath}");

            var doc = XDocument.Load(_filePath);
            _cache = doc.Root!
                        .Elements("Template")
                        .Select(el => TemplateXmlMapper.FromXml(el, _abilityRepo.GetById))
                        .ToDictionary(t => t.TemplateName);
            return _cache;
        }

        public void Save(IEnumerable<Template> templates)
        {
            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("Templates", templates.Select(TemplateXmlMapper.ToXml))
            );
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            doc.Save(_filePath);
            _cache = null;
        }
    }
}