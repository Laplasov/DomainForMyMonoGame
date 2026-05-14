using UnceasingFear.Application.Repository;
using UnceasingFear.Domain.World.Aggregates;
using UnceasingFear.Domain.World.ValueObjects;
using UnceasingFear.Persistence.Xml;
using UnceasingFear.Persistence.XML;

namespace UnceasingFear.Persistence
{
    public class XmlSceneProvider : ISceneProvider
    {
        private readonly XmlSceneRepository _sceneRepo;

        public XmlSceneProvider(string dataDirectory)
        {
            var abilitiesFile = Path.Combine(dataDirectory, "abilities.xml");
            var templatesFile = Path.Combine(dataDirectory, "templates.xml");
            var groupsFile = Path.Combine(dataDirectory, "groups.xml");
            var scenesFile = Path.Combine(dataDirectory, "scenes.xml");

            var abilityRepo = new XmlAbilityRepository(abilitiesFile);
            var templateRepo = new XmlTemplateRepository(templatesFile, abilityRepo);
            var groupRepo = new XmlGroupRepository(groupsFile, templateRepo);
            _sceneRepo = new XmlSceneRepository(scenesFile, groupRepo);
        }

        public Scene? GetById(SceneId id) => _sceneRepo.GetById(id);
        public void Save(Scene scene) => _sceneRepo.Save(scene);
    }
}