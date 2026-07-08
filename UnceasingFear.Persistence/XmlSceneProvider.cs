using UnceasingFear.Application.Repository;
using UnceasingFear.Domain.World.Aggregates;
using UnceasingFear.Domain.World.ValueObjects;
using UnceasingFear.Persistence.Xml;
using UnceasingFear.Persistence.Xml.Mappers;
using UnceasingFear.Persistence.XML;

namespace UnceasingFear.Persistence
{
    public class XmlSceneProvider : ISceneProvider
    {
        private readonly XmlSceneRepository _sceneRepo;
        private readonly Dictionary<string, Scene> _sceneCache = new();
        public XmlSceneProvider(string dataDirectory)
        {
            var abilitiesFile = Path.Combine(dataDirectory, "abilities.xml");
            var templatesFile = Path.Combine(dataDirectory, "templates.xml");
            var groupsFile = Path.Combine(dataDirectory, "groups.xml");
            var scenesFile = Path.Combine(dataDirectory, "scenes.xml");
            var dialogsFile = Path.Combine(dataDirectory, "dialogs.xml");

            var abilityRepo = new XmlAbilityRepository(abilitiesFile);
            var templateRepo = new XmlTemplateRepository(templatesFile, abilityRepo);
            var groupRepo = new XmlGroupRepository(groupsFile, templateRepo);
            var dialogueRepo = new XmlDialogueRepository(dialogsFile);
            _sceneRepo = new XmlSceneRepository(scenesFile, groupRepo, dialogueRepo, dataDirectory);

        }

        public Scene? GetById(SceneId id)
        {
            if (_sceneCache.TryGetValue(id.Value, out var cached))
                return cached;

            var scene = _sceneRepo.GetById(id);
            _sceneCache[id.Value] = scene!;
            return scene;
        }
        public void Save(Scene scene) => _sceneRepo.Save(scene);
    }
}