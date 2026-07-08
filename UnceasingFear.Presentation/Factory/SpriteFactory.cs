// UnceasingFear.Presentation/Data/SpriteFactory.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame_Game_Library.Graphics;
using MonoGame_Game_Library.TileLogic;
using System.Xml.Linq;
using UnceasingFear.Domain.World.ValueObjects;
using UnceasingFear.Persistence.XML;

namespace UnceasingFear.Presentation.Data
{
    public class SpriteFactory
    {
        private readonly ContentManager _content;
        private readonly XmlSpriteRepository _repo;

        // Cache loaded atlases and tilemaps so they're not reloaded per entity.
        private readonly Dictionary<string, TextureAtlas> _atlasCache = new();
        private readonly Dictionary<string, TileMapLayered> _tilemapCache = new();

        public SpriteFactory(ContentManager content, XmlSpriteRepository repo)
        {
            _content = content;
            _repo = repo;
        }

        public AnimatedSprite CreateGroupSprite(string entityId)
        {
            var data = _repo.GetGroupSprite(entityId);
            var atlas = LoadAtlas(data.AnimationPath, data.TexturePath);
            // Default to "down" animation; WorldView can switch based on velocity.
            return atlas.CreateAnimatedSprite("down");
        }

        public AnimatedSprite CreateUnitSprite(string unitId)
        {
            var data = _repo.GetUnitSprite(unitId);
            var atlas = LoadAtlas(data.AnimationPath, data.TexturePath);
            return atlas.CreateAnimatedSprite("down");
        }

        public TileMapLayered CreateTileMap(string sceneId)
        {
            var data = _repo.GetTileSet(sceneId);

            if (!_tilemapCache.TryGetValue(sceneId, out var tilemap))
            {
                tilemap = TileMapLayered.LoadFromXml(
                    Path.Combine(_content.RootDirectory, data.TmxPath));

                var texture = _content.Load<Texture2D>(data.TexturePath);
                tilemap.SetLayerTileset("Ground", texture, tilemap.TileWidth, tilemap.TileHeight);

                

                _tilemapCache[sceneId] = tilemap;
            }

            return tilemap;
        }

        // ── Private helpers ──────────────────────────────────────────────────

        private TextureAtlas LoadAtlas(string animationPath, string texturePath)
        {
            if (!_atlasCache.TryGetValue(animationPath, out var atlas))
            {
                // Load texture first
                var texture = _content.Load<Texture2D>(texturePath);

                // Build the atlas manually instead of using FromFile,
                // since our XML has no <Texture> element.
                atlas = new TextureAtlas(texture);

                string filePath = Path.Combine(_content.RootDirectory, animationPath);
                using var stream = TitleContainer.OpenStream(filePath);
                var doc = XDocument.Load(stream);
                var root = doc.Root!;

                foreach (var region in root.Element("Regions")!.Elements("Region"))
                {
                    atlas.AddRegion(
                        region.Attribute("name")!.Value,
                        int.Parse(region.Attribute("x")!.Value),
                        int.Parse(region.Attribute("y")!.Value),
                        int.Parse(region.Attribute("width")!.Value),
                        int.Parse(region.Attribute("height")!.Value));
                }

                foreach (var animEl in root.Element("Animations")!.Elements("Animation"))
                {
                    string name = animEl.Attribute("name")!.Value;
                    float delayMs = float.Parse(animEl.Attribute("delay")!.Value);
                    var frames = animEl.Elements("Frame")
                        .Select(f => atlas.GetRegion(f.Attribute("region")!.Value))
                        .ToList();
                    atlas.AddAnimation(name, new Animation(frames, TimeSpan.FromMilliseconds(delayMs)));
                }

                _atlasCache[animationPath] = atlas;
            }
            return atlas;
        }
    }
}