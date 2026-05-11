using UnceasingFear.Domain.World.Aggregates;
using UnceasingFear.Domain.World.ValueObjects;
using UnceasingFear.TestImplementation;
using UnceasingFear.Application.Repository;

namespace UnceasingFear.Persistence
{
    public class SceneProviderTest : ISceneProvider
    {
        public Scene? GetById(SceneId id) => GetTestScene(id);
        public void Save(Scene scene)
        {
            throw new NotImplementedException();
        }
        public Scene GetTestScene(SceneId id)
        {
            // Create domain scene
            var metadata = new TileMapMetadata(
                Width: 20,
                Height: 20,
                TileWidth: 64,
                TileHeight: 64,
                LayerScale: 1f
            );

            var scene = new Scene(
                id: id,
                mapMetadata: metadata
            );


            // Add test groups
            var group1 = GroupFactory.CreateGroup1Goblin();
            var group2 = GroupFactory.CreateGroup2Slime();
            var playerGroup = GroupFactory.CreateGroupPlayer();

            scene.AddGroup(group1);
            scene.AddGroup(group2);
            scene.AddGroup(playerGroup);

            // Add test transition
            var transition = new SceneTransition(
                triggerTile: new TileCoord(5, 4),   // world center ~(352, 288)
                targetScene: SceneId.From("NextScene"),
                nextSceneTile: new WorldPosition(600, 600)
            );
            scene.AddTransition(transition);

            return scene;

        }

    }
}
