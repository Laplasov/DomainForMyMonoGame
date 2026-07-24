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
        public XmlSceneProvider(string dataDirectory, XmlAbilityRepository abilityRepo)
        {
            var baseDirectory = AppContext.BaseDirectory;
            var fullDataDirectory = Path.Combine(baseDirectory, dataDirectory);

            //var abilitiesFile = Path.Combine(fullDataDirectory, "abilities.xml");
            var templatesFile = Path.Combine(fullDataDirectory, "templates.xml");
            var groupsFile = Path.Combine(fullDataDirectory, "groups.xml");
            var scenesFile = Path.Combine(fullDataDirectory, "scenes.xml");
            var dialogsFile = Path.Combine(fullDataDirectory, "dialogs.xml");

            //var abilityRepo = new XmlAbilityRepository(abilitiesFile);
            var templateRepo = new XmlTemplateRepository(templatesFile, abilityRepo);
            var groupRepo = new XmlGroupRepository(groupsFile, templateRepo);

            // ✅ Inject template repository delegate
            var dialogueRepo = new XmlDialogueRepository(dialogsFile, name => templateRepo.GetById(name));

            _sceneRepo = new XmlSceneRepository(scenesFile, groupRepo, dialogueRepo, fullDataDirectory);
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