using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Domain.World.ValueObjects;
using UnceasingFear.Persistence.Xml.Mappers;

namespace UnceasingFear.Persistence.Xml
{
    public class XmlDialogueRepository
    {
        private readonly string _filePath;
        private readonly Func<string, Template> _resolveTemplate;
        private Dictionary<string, DialogueTree>? _cache;

        public XmlDialogueRepository(string filePath, Func<string, Template> resolveTemplate)
        {
            _filePath = filePath;
            _resolveTemplate = resolveTemplate;
        }

        public DialogueTree GetById(string id)
        {
            var cache = LoadAll();
            if (cache.TryGetValue(id, out var tree))
                return tree;

            return DialogueTree.Empty;
        }

        public IReadOnlyDictionary<string, DialogueTree> LoadAll()
        {
            if (_cache != null) return _cache;

            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"Dialogue data file not found: {_filePath}");

            var doc = XDocument.Load(_filePath);

            _cache = doc.Root!
                        .Elements("Dialog")
                        .Select(el => DialogueXmlMapper.FromXml(el, _resolveTemplate)) // ✅ Passed delegate here
                        .ToDictionary(t => t.Id);
            return _cache;
        }
    }
}
