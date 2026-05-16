using Microsoft.Xna.Framework.Graphics;
using MonoGame_Game_Library.TileLogic;

namespace MonoGame_Game_Library.Graphics
{
    public class TerrainRenderer
    {
        private const float PIXEL_TO_WORLD_SCALE = 0.01f;
        private BasicEffect _effect;
        private Mesh3D _mesh;

        public TerrainRenderer(GraphicsDevice graphicsDevice)
        {
            _effect = new BasicEffect(graphicsDevice)
            {
                TextureEnabled = true,
                VertexColorEnabled = false,
                LightingEnabled = false
            };
        }

        public void LoadFromTileMap(TileMapLayered tileMap, string layerName)
        {
            var layer = tileMap.Layers[layerName];
            _effect.Texture = layer.Tileset;

            // Create grid mesh
            float tileW = tileMap.TileWidth * PIXEL_TO_WORLD_SCALE;
            float tileH = tileMap.TileHeight * PIXEL_TO_WORLD_SCALE;
            _mesh = MeshBuilder.CreateGrid(tileMap.Width, tileMap.Height, tileW, tileH);

            // Update UVs based on tile data
            int tilesetRows = layer.Tileset.Height / layer.TileHeight;
            MeshBuilder.UpdateGridTileUVs(_mesh, layer.TileData, layer.TilesetColumns, tilesetRows);
        }

        public void Draw(CameraMatrix3D camera, GraphicsDevice device)
        {
            if (_mesh == null) return;

            device.DepthStencilState = DepthStencilState.Default;
            device.RasterizerState = RasterizerState.CullCounterClockwise;
            device.SamplerStates[0] = SamplerState.PointClamp;

            _effect.World = camera.World;
            _effect.View = camera.View;
            _effect.Projection = camera.Projection;

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _mesh.Draw(device);
            }
        }
    }
}