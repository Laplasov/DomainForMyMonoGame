using System.Xml.Linq;
using UnceasingFear.Domain.World.Entities;
using UnceasingFear.Domain.World.ValueObjects;
using UnceasingFear.Persistence.Xml.Mappers;
using UnceasingFear.Persistence.XML;

namespace UnceasingFear.Persistence.Xml
{
    public class XmlGroupRepository
    {
        private readonly string _filePath;
        private readonly XmlTemplateRepository _templateRepo;
        private Dictionary<string, Group>? _cache;

        public XmlGroupRepository(string filePath, XmlTemplateRepository templateRepo)
        {
            _filePath = filePath;
            _templateRepo = templateRepo;
        }

        public Group GetById(GroupId id)
        {
            var cache = LoadAll();
            if (!cache.TryGetValue(id.Value, out var group))
                throw new KeyNotFoundException($"Group '{id.Value}' not found in '{_filePath}'.");
            return group;
        }

        public IReadOnlyDictionary<string, Group> LoadAll()
        {
            if (_cache != null) return _cache;

            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"Group data file not found: {_filePath}");

            var doc = XDocument.Load(_filePath);
            _cache = doc.Root!
                        .Elements("Group")
                        .Select(el => GroupXmlMapper.FromXml(el, _templateRepo.GetById))
                        .ToDictionary(g => g.Id.Value);
            return _cache;
        }

        public void Save(IEnumerable<Group> groups)
        {
            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("Groups", groups.Select(GroupXmlMapper.ToXml))
            );
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            doc.Save(_filePath);
            _cache = null;
        }

        public void Save(Group group)
        {
            var all = LoadAll().Values.ToList();
            var idx = all.FindIndex(g => g.Id.Value == group.Id.Value);
            if (idx >= 0) all[idx] = group;
            else all.Add(group);
            Save(all);
        }
    }
}