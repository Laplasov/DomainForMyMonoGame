namespace UnceasingFear.Presentation.Data
{
    public record GroupSpriteData(string Id, string AnimationPath, string TexturePath);
    public record UnitSpriteData(string Id, string AnimationPath, string TexturePath);
    public record TileSetData(string SceneId, string TexturePath, string TmxPath);
}